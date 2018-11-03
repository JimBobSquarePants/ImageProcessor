// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessingResultEventArgs.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The post processing result event arguments.
//   Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs"/>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Plugins.PostProcessor
{
	using System;
	using System.IO;
	using System.Text;

	/// <summary>
	/// The post processing result event arguments.
	/// </summary>
	/// <seealso cref="System.EventArgs" />
	/// <remarks>
	/// Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs" />.
	/// </remarks>
	public class PostProcessingResultEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PostProcessingResultEventArgs" /> class.
		/// </summary>
		/// <param name="resultFileName">The original file name.</param>
		/// <param name="length">The original file length in bytes.</param>
		public PostProcessingResultEventArgs(string resultFileName, long length)
		{
			var result = new FileInfo(resultFileName);
			this.OriginalFileSize = length;
			if (result.Exists)
			{
				this.ResultFileName = result.FullName;
				this.ResultFileSize = result.Length;
			}
		}

		/// <summary>
		/// Gets or sets the original file size in bytes.
		/// </summary>
		/// <value>
		/// The size of the original file.
		/// </value>
		public long OriginalFileSize { get; set; }

		/// <summary>
		/// Gets or sets the result file size in bytes.
		/// </summary>
		/// <value>
		/// The size of the result file.
		/// </value>
		public long ResultFileSize { get; set; }

		/// <summary>
		/// Gets or sets the result file name.
		/// </summary>
		/// <value>
		/// The name of the result file.
		/// </value>
		public string ResultFileName { get; set; }

		/// <summary>
		/// Gets the difference in file size in bytes.
		/// </summary>
		/// <value>
		/// The difference in file size in bytes.
		/// </value>
		public long Saving => this.OriginalFileSize - this.ResultFileSize;

		/// <summary>
		/// Gets the difference in file size as a percentage.
		/// </summary>
		/// <value>
		/// The difference in file size as a percentage.
		/// </value>
		public double Percent => Math.Round(100 - ((this.ResultFileSize / (double)this.OriginalFileSize) * 100), 1);

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Optimized " + Path.GetFileName(this.ResultFileName));
			stringBuilder.AppendLine("Before: " + this.OriginalFileSize + " bytes");
			stringBuilder.AppendLine("After: " + this.ResultFileSize + " bytes");
			stringBuilder.AppendLine("Saving: " + this.Saving + " bytes / " + this.Percent + "%");

			return stringBuilder.ToString();
		}
	}
}