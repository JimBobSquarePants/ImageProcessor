// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessor.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The image post processor.
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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using ImageProcessor.Configuration;

    /// <summary>
    /// The image post processor.
    /// </summary>
    /// <remarks>
    /// Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs" />.
    /// </remarks>
    internal static class PostProcessor
    {
        /// <summary>
        /// Post processes the image.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="stream">The source image stream.</param>
        /// <param name="extension">The image extension.</param>
        /// <returns>
        /// The <see cref="MemoryStream" />.
        /// </returns>
        public static async Task<MemoryStream> PostProcessImageAsync(HttpContext context, MemoryStream stream, string extension)
        {
            var postProcessorBootstrapper = PostProcessorBootstrapper.Instance;
            if (postProcessorBootstrapper.IsInstalled && stream.Length is var length && length > 0)
            {
                string sourceFile = null, destinationFile = null;
                try
                {
                    // Get temporary file names
                    var tempPath = Path.GetTempPath();
                    var tempFile = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
                    sourceFile = Path.Combine(tempPath, Path.ChangeExtension(tempFile, extension));
                    destinationFile = Path.Combine(tempPath, Path.ChangeExtension(tempFile + "-out", extension));

                    // Get processes to start
                    var processStartInfos = GetProcessStartInfos(extension, length, sourceFile, destinationFile).ToList();
                    if (processStartInfos.Count > 0)
                    {
                        // Save the input stream to our source temp file for post processing
                        var sourceFileInfo = new FileInfo(sourceFile);
                        using (var fileStream = sourceFileInfo.Create())
                        {
                            // Try to keep the file in memory and ensure it's not indexed
                            sourceFileInfo.Attributes |= FileAttributes.Temporary | FileAttributes.NotContentIndexed;

                            await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                        }

                        // Create cancellation token with timeout
                        using (var cancellationTokenSource = new CancellationTokenSource(postProcessorBootstrapper.Timout))
                        {
                            var remainingProcesses = processStartInfos.Count;
                            foreach (var processStartInfo in processStartInfos)
                            {
                                // Set default properties
                                processStartInfo.FileName = Path.Combine(postProcessorBootstrapper.WorkingPath, processStartInfo.FileName);
                                processStartInfo.CreateNoWindow = true;
                                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                                // Run process
                                using (var processResults = await ProcessEx.RunAsync(processStartInfo, cancellationTokenSource.Token).ConfigureAwait(false))
                                {
                                    if (processResults.ExitCode == 1)
                                    {
                                        ImageProcessorBootstrapper.Instance.Logger.Log(typeof(PostProcessor), $"Unable to post process image for request {context.Request.Unvalidated.Url}, {processStartInfo.FileName} {processStartInfo.Arguments} exited with error code 1. Original image returned.");
                                        break;
                                    }
                                }

                                remainingProcesses--;

                                var destinationFileInfo = new FileInfo(destinationFile);
                                if (destinationFileInfo.Exists)
                                {
                                    // Delete the source file
                                    sourceFileInfo.IsReadOnly = false;
                                    sourceFileInfo.Delete();

                                    if (remainingProcesses > 0)
                                    {
                                        // Use destination file as new source (for the next process)
                                        destinationFileInfo.MoveTo(sourceFile);
                                    }

                                    // Swap source for destination
                                    sourceFileInfo = destinationFileInfo;

                                    // Try to keep the file in memory and ensure it's not indexed
                                    sourceFileInfo.Attributes |= FileAttributes.Temporary | FileAttributes.NotContentIndexed;
                                }
                            }
                        }

                        // Refresh source file (because it's changed by external processes)
                        sourceFileInfo.Refresh();
                        if (sourceFileInfo.Exists && sourceFileInfo.Length < length)
                        {
                            // Save result back to stream
                            using (var fileStream = sourceFileInfo.OpenRead())
                            {
                                stream.SetLength(0);
                                await fileStream.CopyToAsync(stream).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    ImageProcessorBootstrapper.Instance.Logger.Log(typeof(PostProcessor), $"Unable to post process image for request {context.Request.Unvalidated.Url} within {postProcessorBootstrapper.Timout}ms. Original image returned.");
                }
                catch (Exception ex)
                {
                    // Some security policies don't allow execution of programs in this way
                    ImageProcessorBootstrapper.Instance.Logger.Log(typeof(PostProcessor), ex.Message);
                }
                finally
                {
                    // Set position back to begin
                    stream.Position = 0;

                    // Always cleanup files
                    if (sourceFile != null)
                    {
                        try
                        {
                            var sourceFileInfo = new FileInfo(sourceFile);
                            if (sourceFileInfo.Exists)
                            {
                                sourceFileInfo.IsReadOnly = false;
                                sourceFileInfo.Delete();
                            }
                        }
                        catch
                        {
                            // Normally a no no, but logging would be excessive + temp files get cleaned up eventually
                        }
                    }

                    if (destinationFile != null)
                    {
                        try
                        {
                            var destinationFileInfo = new FileInfo(destinationFile);
                            if (destinationFileInfo.Exists)
                            {
                                destinationFileInfo.IsReadOnly = false;
                                destinationFileInfo.Delete();
                            }
                        }
                        catch
                        {
                            // Normally a no no, but logging would be excessive + temp files get cleaned up eventually
                        }
                    }
                }
            }

            // ALways return stream (even if it's not optimized)
            return stream;
        }

        /// <summary>
        /// Gets the process start infos.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="length">The length.</param>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The destination file.</param>
        /// <returns>
        /// The process start infos.
        /// </returns>
        private static IEnumerable<ProcessStartInfo> GetProcessStartInfos(string extension, long length, string sourceFile, string destinationFile)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".png":
                    yield return new ProcessStartInfo("pingo.exe", $"-s8 -q \"{sourceFile}\"");
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
    }
}