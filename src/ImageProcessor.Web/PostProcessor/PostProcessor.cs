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

namespace ImageProcessor.Web.PostProcessor
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// The image postprocessor.
    /// Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs"/>
    /// </summary>
    internal static class PostProcessor
    {
        /// <summary>
        /// Post processes the image asynchronously.
        /// </summary>
        /// <param name="stream">The source image stream.</param>
        /// <param name="extension">The image extension.</param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<MemoryStream> PostProcessImageAsync(MemoryStream stream, string extension)
        {
            // Create a source temporary file with the correct extension.
            long length = stream.Length;
            string tempFile = Path.GetTempFileName();
            string sourceFile = Path.ChangeExtension(tempFile, extension);
            File.Move(tempFile, sourceFile);

            // Save the input stream to a temp file for post processing.
            using (FileStream fileStream = File.Create(sourceFile))
            {
                await stream.CopyToAsync(fileStream);
            }

            PostProcessingResultEventArgs result = await RunProcess(sourceFile, length);

            if (result != null && result.Saving > 0)
            {
                using (FileStream fileStream = File.OpenRead(sourceFile))
                {
                    // Replace stream contents.
                    stream.SetLength(0);
                    await fileStream.CopyToAsync(stream);
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
        private static Task<PostProcessingResultEventArgs> RunProcess(string sourceFile, long length)
        {
            TaskCompletionSource<PostProcessingResultEventArgs> tcs = new TaskCompletionSource<PostProcessingResultEventArgs>();
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
                tcs.SetResult(null);
                return tcs.Task;
            }

            Process process = new Process
            {
                StartInfo = start,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(new PostProcessingResultEventArgs(sourceFile, length));
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
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
                    return string.Format(CultureInfo.CurrentCulture, "/c gifsicle --crop-transparency --no-comments --no-extensions --no-names --optimize=3 --batch \"{0}\" --output=\"{0}\"", sourceFile);
            }

            return null;
        }
    }
}
