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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
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

            // Save the input stream to our source temp file for post processing.
            using (FileStream fileStream = File.Create(sourceFile))
            {
                stream.CopyTo(fileStream);
            }

            PostProcessingResultEventArgs result = RunProcess(context.Request.Unvalidated.Url, sourceFile, destinationFile, length);

            // If our result is good and a saving is made we replace our original stream contents with our new compressed file.
            if (result != null && result.ResultFileSize > 0 && result.Saving > 0)
            {
                using (FileStream fileStream = File.OpenRead(destinationFile))
                {
                    stream.SetLength(0);
                    fileStream.CopyTo(stream);
                }
            }

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

            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Runs the process to optimize the images.
        /// </summary>
        /// <param name="url">The current request url.</param>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The destination file.</param>
        /// <param name="length">The source file length in bytes.</param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/> containing post-processing information.
        /// </returns>
        private static PostProcessingResultEventArgs RunProcess(Uri url, string sourceFile, string destinationFile, long length)
        {
            // Create a new, hidden process to run our postprocessor command.
            // We allow no more than the set timeout (default 5 seconds) for the process to run before killing it to prevent blocking the app.
            int timeout = PostProcessorBootstrapper.Instance.Timout;
            PostProcessingResultEventArgs result = null;
            string arguments = GetArguments(sourceFile, destinationFile, length);

            if (string.IsNullOrWhiteSpace(arguments))
            {
                // Not a file we can post process.
                return null;
            }

            ProcessStartInfo start = new ProcessStartInfo("cmd")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = PostProcessorBootstrapper.Instance.WorkingPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = null;
            try
            {
                process = new Process
                {
                    StartInfo = start,
                    EnableRaisingEvents = true
                };

                // Process has completed successfully within the time limit.
                process.Exited += (sender, args) =>
                {
                    result = new PostProcessingResultEventArgs(destinationFile, length);
                };

                process.Start();

                // Recursively kill all child processes.
                void KillProcessAndChildren(int pid)
                {
                    using (var searcher = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={pid}"))
                    {
                        ManagementObjectCollection moc = searcher.Get();
                        foreach (ManagementBaseObject mo in moc)
                        {
                            KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                        }

                        try
                        {
                            Process proc = Process.GetProcessById(pid);
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

            return result;
        }

        /// <summary>
        /// Gets the correct arguments to pass to the post-processor.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The source file.</param>
        /// <param name="length">The source file length in bytes.</param>
        /// <returns>
        /// The <see cref="string"/> containing the correct command arguments.
        /// </returns>
        private static string GetArguments(string sourceFile, string destinationFile, long length)
        {
            if (!Uri.IsWellFormedUriString(sourceFile, UriKind.RelativeOrAbsolute) && !File.Exists(sourceFile))
            {
                return null;
            }

            string ext;

            string extension = Path.GetExtension(sourceFile);
            if (extension != null)
            {
                ext = extension.ToLowerInvariant();
            }
            else
            {
                return null;
            }

            switch (ext)
            {
                case ".png":
                    return string.Format(CultureInfo.CurrentCulture, "/c png.cmd \"{0}\" \"{1}\"", sourceFile, destinationFile);

                case ".jpg":
                case ".jpeg":

                    // If it's greater than 10Kb use progressive
                    // http://yuiblog.com/blog/2008/12/05/imageopt-4/
                    if (length > 10000)
                    {
                        return string.Format(CultureInfo.CurrentCulture, "/c cjpeg -quality 80,60 -smooth 5 -outfile \"{1}\" \"{0}\"", sourceFile, destinationFile);
                    }

                    return string.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy all -optimize -outfile \"{1}\" \"{0}\"", sourceFile, destinationFile);

                case ".gif":
                    return string.Format(CultureInfo.CurrentCulture, "/c gifsicle --optimize=3 \"{0}\" --output=\"{1}\"", sourceFile, destinationFile);
            }

            return null;
        }
    }
}