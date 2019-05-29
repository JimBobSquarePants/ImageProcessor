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
    using System.Configuration;

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
			if (!PostProcessorBootstrapper.Instance.IsInstalled)
			{
				return stream;
			}

			string sourceFile = null, destinationFile = null;
			var timeout = PostProcessorBootstrapper.Instance.Timout;
			try
			{
				// Save source file length
				var length = stream.Length;

				// Create a temporary source file with the correct extension
				var tempSourceFile = Path.GetTempFileName();
				sourceFile = Path.ChangeExtension(tempSourceFile, extension);
				File.Move(tempSourceFile, sourceFile);

				// Give our destination file a unique name
				destinationFile = sourceFile.Replace(extension, "-out" + extension);

				// Get processes to start
				var processStartInfos = GetProcessStartInfos(extension, length, sourceFile, destinationFile).ToList();
				if (processStartInfos.Count > 0)
				{
					// Save the input stream to our source temp file for post processing
					using (var fileStream = File.Create(sourceFile))
					{
						await stream.CopyToAsync(fileStream).ConfigureAwait(false);
					}

					// Create cancellation token with timeout
					using (var cancellationTokenSource = new CancellationTokenSource(timeout))
					{
						foreach (var processStartInfo in processStartInfos)
						{
							// Use destination file as new source (if previous process created one).
							if (File.Exists(destinationFile))
							{
								File.Copy(destinationFile, sourceFile, true);
							}

							// Set default properties
							processStartInfo.FileName = Path.Combine(PostProcessorBootstrapper.Instance.WorkingPath, processStartInfo.FileName);
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
						}
					}

					// Save result
					var result = new PostProcessingResultEventArgs(destinationFile, length);
					if (result.ResultFileSize > 0 && result.Saving > 0)
					{
						using (var fileStream = File.OpenRead(destinationFile))
						{
							stream.SetLength(0);
							await fileStream.CopyToAsync(stream).ConfigureAwait(false);
						}
					}
				}
			}
			catch (OperationCanceledException)
			{
				ImageProcessorBootstrapper.Instance.Logger.Log(typeof(PostProcessor), $"Unable to post process image for request {context.Request.Unvalidated.Url} within {timeout}ms. Original image returned.");
			}
			catch (Exception ex)
			{
				// Some security policies don't allow execution of programs in this way
				ImageProcessorBootstrapper.Instance.Logger.Log(typeof(PostProcessor), ex.Message);
			}
			finally
			{
				// Always cleanup files
				try
				{
					// Ensure files exist, are not read only, and delete
					if (sourceFile != null && File.Exists(sourceFile))
					{
						File.SetAttributes(sourceFile, FileAttributes.Normal);
						File.Delete(sourceFile);
					}

					if (destinationFile != null && File.Exists(destinationFile))
					{
						File.SetAttributes(destinationFile, FileAttributes.Normal);
						File.Delete(destinationFile);
					}
				}
				catch
				{
					// Normally a no no, but logging would be excessive + temp files get cleaned up eventually.
				}
			}

			// ALways return stream (even if it's not optimized)
			stream.Position = 0;
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
            // allow optional disabling of TruePng as some systems (such as Azure WebApps) don't support it's execution
            string disableTruePngKeyName = "ImageProcessor.Web.PostProcessor.DisableTruePNG";
            var disableTruePngValue = ConfigurationManager.AppSettings[disableTruePngKeyName];
            bool.TryParse(disableTruePngValue, out var disableTruePng);

            // Make sure the commands overwrite the destination file (in case multiple are executed)
            switch (extension.ToLowerInvariant())
			{
				case ".png":
					yield return new ProcessStartInfo("pngquant.exe", $"--quality=90-99 --output \"{destinationFile}\" \"{sourceFile}\"");
                    if (!disableTruePng) { 
					    yield return new ProcessStartInfo("truepng.exe", $"-o4 -y -out \"{destinationFile}\" \"{sourceFile}\"");
                    }
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