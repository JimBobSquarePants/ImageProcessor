// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessor.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The image postprocessor.
//   Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs"/>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Plugins.PostProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Web;

    using ImageProcessor.Configuration;

    /// <summary>
    /// The image postprocessor.
    /// Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs"/>
    /// </summary>
    internal static class PostProcessor
    {
        /// <summary>
        /// Post processes the image.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="stream">The source image stream.</param>
        /// <param name="extension">The image extension.</param>
        /// <returns>
        /// The <see cref="MemoryStream"/>.
        /// </returns>
        public static MemoryStream PostProcessImage(HttpContext context, MemoryStream stream, string extension)
        {
            if (!PostProcessorBootstrapper.Instance.IsInstalled)
            {
                return stream;
            }

            // Create a temporary source file with the correct extension.
            long length = stream.Length;

            string tempSourceFile = Path.GetTempFileName();
            string sourceFile = Path.ChangeExtension(tempSourceFile, extension);
            File.Move(tempSourceFile, sourceFile);

            // Give our destination file a unique name.
            string destinationFile = sourceFile.Replace(extension, "-out" + extension);

            IEnumerable<ProcessStartInfo> processes = GetProcessStartInfo(extension, length, sourceFile, destinationFile);

            if (!processes.Any())
            {
                // Not a file we can post process.
                Cleanup(sourceFile, destinationFile);

                stream.Position = 0;
                return stream;
            }

            // Save the input stream to our source temp file for post processing.
            using (FileStream fileStream = File.Create(sourceFile))
            {
                stream.CopyTo(fileStream);
            }

            PostProcessingResultEventArgs result = RunProcess(context.Request.Unvalidated.Url, processes, sourceFile, destinationFile, length);

            // If our result is good and a saving is made we replace our original stream contents with our new compressed file.
            if (result?.ResultFileSize > 0 && result.Saving > 0)
            {
                using (FileStream fileStream = File.OpenRead(destinationFile))
                {
                    stream.SetLength(0);
                    fileStream.CopyTo(stream);
                }
            }

            // Cleanup the temp files.
            Cleanup(sourceFile, destinationFile);

            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Runs the processes to optimize the images.
        /// </summary>
        /// <param name="url">The current request url.</param>
        /// <param name="processes">The collection of processes to run.</param>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The destination file.</param>
        /// <param name="length">The source file length in bytes.</param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/> containing post-processing information.
        /// </returns>
        private static PostProcessingResultEventArgs RunProcess(Uri url, IEnumerable<ProcessStartInfo> processes, string sourceFile, string destinationFile, long length)
        {
            // Create a new, hidden process to run our postprocessor command.
            // We allow no more than the set timeout (default 5 seconds) for the process to run before killing it to prevent blocking the app.
            int timeout = PostProcessorBootstrapper.Instance.Timout;
            PostProcessingResultEventArgs result = null;

            foreach (ProcessStartInfo item in processes)
            {
                // Use destination file as new source (if previous process created one).
                if (File.Exists(destinationFile))
                {
                    File.Copy(destinationFile, sourceFile, true);
                }

                item.WorkingDirectory = PostProcessorBootstrapper.Instance.WorkingPath;
                item.CreateNoWindow = true;
                item.WindowStyle = ProcessWindowStyle.Hidden;

                Process process = null;
                try
                {
                    process = new Process
                    {
                        StartInfo = item,
                        EnableRaisingEvents = true
                    };

                    // Process has completed successfully within the time limit.
                    process.Exited += (sender, args) => result = new PostProcessingResultEventArgs(destinationFile, length);

                    process.Start();

                    // Recursively kill all child processes.
                    void KillProcessAndChildren(int pid)
                    {
                        using (var searcher = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={pid}"))
                        {
                            foreach (ManagementBaseObject mo in searcher.Get())
                            {
                                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                            }

                            try
                            {
                                var proc = Process.GetProcessById(pid);
                                proc.Kill();
                            }
                            catch
                            {
                                // Process already exited.
                            }
                        }
                    }

                    // Wait for processing to finish, but not more than our timeout.
                    if (!process.WaitForExit(timeout))
                    {
                        KillProcessAndChildren(process.Id);
                        ImageProcessorBootstrapper.Instance.Logger.Log(
                            typeof(PostProcessor),
                            $"Unable to post process image for request {url} within {timeout}ms. Original image returned.");
                    }
                }
                catch (Exception ex)
                {
                    // Some security policies don't allow execution of programs in this way.
                    ImageProcessorBootstrapper.Instance.Logger.Log(typeof(PostProcessor), ex.Message);
                }
                finally
                {
                    // Make sure we always dispose and release
                    process?.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the correct arguments to pass to the post-processor.
        /// </summary>
        /// <param name="extension">The processed image extension.</param>
        /// <param name="length">The source file length in bytes.</param>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The source file.</param>
        /// <returns>
        /// The <see cref="IEnumerable{ProcessStartInfo}"/> containing the correct command arguments.
        /// </returns>
        private static IEnumerable<ProcessStartInfo> GetProcessStartInfo(string extension, long length, string sourceFile, string destinationFile)
        {
            // Make sure the commands overwrite the destination file (in case multiple are executed)
            switch (extension.ToLowerInvariant())
            {
                case ".png":
                    yield return new ProcessStartInfo("pngquant.exe", $"--quality=90-99 --output \"{destinationFile}\" \"{sourceFile}\" --force");
                    yield return new ProcessStartInfo("truepng.exe", $"-o4 -y -out \"{destinationFile}\" \"{sourceFile}\"");
                    break;
                case ".jpg":
                case ".jpeg":
                    if (length > 10000)
                    {
                        // If it's greater than 10Kb use progressive
                        // http://yuiblog.com/blog/2008/12/05/imageopt-4/
                        yield return new ProcessStartInfo("cjpeg.exe", $"-quality 80,60 -smooth 5 -outfile \"{destinationFile}\" \"{sourceFile}\"");
                    }
                    else
                    {
                        yield return new ProcessStartInfo("jpegtran.exe", $"-copy all -optimize -outfile \"{destinationFile}\" \"{sourceFile}\"");
                    }
                    break;
                case ".gif":
                    yield return new ProcessStartInfo("gifsicle.exe", $"--optimize=3 \"{sourceFile}\" --output=\"{destinationFile}\"");
                    break;
            }
        }

        private static void Cleanup(string sourceFile, string destinationFile)
        {
            // Cleanup the temp files.
            try
            {
                // Ensure files exist, are not read only, and delete
                if (File.Exists(sourceFile))
                {
                    File.SetAttributes(sourceFile, FileAttributes.Normal);
                    File.Delete(sourceFile);
                }

                if (File.Exists(destinationFile))
                {
                    File.SetAttributes(destinationFile, FileAttributes.Normal);
                    File.Delete(destinationFile);
                }
            }
            catch
            {
                // Normally a No no, but logging would be excessive + temp files get cleaned up eventually.
            }
        }
    }
}