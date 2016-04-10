// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessor.cs" company="James South">
//   Copyright (c) James South.
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
            // Create a source temporary file with the correct extension.
            long length = stream.Length;
            string tempFile = Path.GetTempFileName();
            string sourceFile = Path.ChangeExtension(tempFile, extension);
            File.Move(tempFile, sourceFile);

            // Save the input stream to a temp file for post processing.
            using (FileStream fileStream = File.Create(sourceFile))
            {
                stream.CopyTo(fileStream);
            }

            PostProcessingResultEventArgs result = RunProcess(sourceFile, length);

            if (result != null && result.Saving > 0)
            {
                using (FileStream fileStream = File.OpenRead(sourceFile))
                {
                    // Replace stream contents.
                    stream.SetLength(0);
                    fileStream.CopyTo(stream);
                }
            }

            // Cleanup
            File.Delete(sourceFile);

            stream.Position = 0;

            return stream;
        }

        /// <summary>
        /// Runs the process to optimize the images.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="length">The source file length in bytes.</param>
        /// <returns>
        /// The <see cref="Task{PostProcessingResultEventArgs}"/> containing post-processing information.
        /// </returns>
        private static PostProcessingResultEventArgs RunProcess(string sourceFile, long length)
        {
            PostProcessingResultEventArgs result = null;
            ProcessStartInfo start = new ProcessStartInfo("cmd")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = PostProcessorBootstrapper.Instance.WorkingPath,
                Arguments = GetArguments(sourceFile, length),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (string.IsNullOrWhiteSpace(start.Arguments))
            {
                return null;
            }

            int elapsedTime = 0;
            bool eventHandled = false;

            try
            {
                Process process = new Process
                {
                    StartInfo = start,
                    EnableRaisingEvents = true
                };

                process.Exited += (sender, args) =>
                {
                    result = new PostProcessingResultEventArgs(sourceFile, length);
                    process.Dispose();
                    eventHandled = true;
                };

                process.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Some security policies don't allow execution of programs in this way
                return null;
            }

            // Wait for Exited event, but not more than 5 seconds.
            const int SleepAmount = 100;
            while (!eventHandled)
            {
                elapsedTime += SleepAmount;
                if (elapsedTime > 5000)
                {
                    break;
                }

                Thread.Sleep(SleepAmount);
            }

            return result;
        }

        /// <summary>
        /// Gets the correct arguments to pass to the post-processor.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="length">The source file length in bytes.</param>
        /// <returns>
        /// The <see cref="string"/> containing the correct command arguments.
        /// </returns>
        private static string GetArguments(string sourceFile, long length)
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
                    return string.Format(CultureInfo.CurrentCulture, "/c png.cmd \"{0}\"", sourceFile);

                case ".jpg":
                case ".jpeg":

                    // If it's greater than 10Kb use progressive
                    // http://yuiblog.com/blog/2008/12/05/imageopt-4/
                    if (length > 10000)
                    {
                        return string.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy all -optimize -progressive \"{0}\" \"{0}\"", sourceFile);
                    }

                    return string.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy all -optimize \"{0}\" \"{0}\"", sourceFile);

                case ".gif":
                    return string.Format(CultureInfo.CurrentCulture, "/c gifsicle --no-comments --no-extensions --no-names --optimize=3 --batch \"{0}\" --output=\"{0}\"", sourceFile);
            }

            return null;
        }
    }
}
