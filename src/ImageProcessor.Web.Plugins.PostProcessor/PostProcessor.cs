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
    using System.Threading;

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
        /// <param name="stream">The source image stream.</param>
        /// <param name="extension">The image extension.</param>
        /// <returns>
        /// The <see cref="MemoryStream"/>.
        /// </returns>
        public static MemoryStream PostProcessImage(MemoryStream stream, string extension)
        {
            // Create temporary files with the correct extension.
            long length = stream.Length;

            string tempSourceFile = Path.GetTempFileName();
            string sourceFile = Path.ChangeExtension(tempSourceFile, extension);
            File.Move(tempSourceFile, sourceFile);

            string destinationFile = sourceFile.Replace(extension, "-out" + extension);

            // Save the input stream to a temp file for post processing.
            using (FileStream fileStream = File.Create(sourceFile))
            {
                stream.CopyTo(fileStream);
            }

            PostProcessingResultEventArgs result = RunProcess(sourceFile, destinationFile, length);

            if (result != null && result.ResultFileSize > 0 && result.Saving > 0)
            {
                using (FileStream fileStream = File.OpenRead(destinationFile))
                {
                    // Replace stream contents.
                    stream.SetLength(0);
                    fileStream.CopyTo(stream);
                }
            }

            // Cleanup
            try
            {
                // Ensure files are not read only
                File.SetAttributes(sourceFile, FileAttributes.Normal);
                File.SetAttributes(destinationFile, FileAttributes.Normal);
                File.Delete(sourceFile);
                File.Delete(destinationFile);
            }
            catch
            {
                // No no, but logging will be excessive.
            }

            stream.Position = 0;

            return stream;
        }

        /// <summary>
        /// Runs the process to optimize the images.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The destination file.</param>
        /// <param name="length">The source file length in bytes.</param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/> containing post-processing information.
        /// </returns>
        private static PostProcessingResultEventArgs RunProcess(string sourceFile, string destinationFile, long length)
        {
            PostProcessingResultEventArgs result = null;
            ProcessStartInfo start = new ProcessStartInfo("cmd")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = PostProcessorBootstrapper.Instance.WorkingPath,
                Arguments = GetArguments(sourceFile, destinationFile, length),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (string.IsNullOrWhiteSpace(start.Arguments))
            {
                return null;
            }

            using (ManualResetEventSlim processingFinished = new ManualResetEventSlim(false))
            {
                Process process = null;
                try
                {
                    process = new Process
                    {
                        StartInfo = start,
                        EnableRaisingEvents = true
                    };

                    process.Exited += (sender, args) =>
                    {
                        result = new PostProcessingResultEventArgs(destinationFile, length);
                        process?.Dispose();
                        processingFinished.Set();
                    };

                    process.Start();

                    // Wait for processing to finish, but not more than 5 seconds.
                    const int MaxWaitTimeMs = 5000;
                    processingFinished.Wait(MaxWaitTimeMs);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    // Some security policies don't allow execution of programs in this way
                    ImageProcessorBootstrapper.Instance.Logger.Log(typeof(PostProcessor), ex.Message);

                    return null;
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