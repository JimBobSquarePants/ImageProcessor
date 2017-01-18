// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageHelpers.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The image helpers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using ImageProcessor.Configuration;
    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Web.Configuration;
    using ImageProcessor.Web.Processors;

    /// <summary>
    /// Contains helper method for parsing image formats.
    /// </summary>
    public class ImageHelpers
    {
        /// <summary>
        /// A new instance of the <see cref="T:ImageProcessor.Web.Config.ImageProcessorConfig"/> class.
        /// with lazy initialization.
        /// </summary>
        private static readonly Lazy<ImageHelpers> Lazy =
                        new Lazy<ImageHelpers>(() => new ImageHelpers());

        /// <summary>
        /// The format processor for checking extensions.
        /// </summary>
        private readonly Format formatProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageHelpers"/> class.
        /// </summary>
        public ImageHelpers()
        {
            // First check to see if the format processor is being used and test against that.
            Type processor =
                ImageProcessorConfiguration.Instance.AvailableWebGraphicsProcessors
                .Keys.FirstOrDefault(
                    p => typeof(Format) == p);

            if (processor != null)
            {
                this.formatProcessor = (Format)Activator.CreateInstance(typeof(Format));
            }
        }

        /// <summary>
        /// Gets the current instance of the <see cref="ImageHelpers"/> class.
        /// </summary>
        public static ImageHelpers Instance => Lazy.Value;

        /// <summary>
        /// The regex pattern.
        /// </summary>
        public static readonly string ExtensionRegexPattern = BuildExtensionRegexPattern();

        /// <summary>
        /// The image format regex.
        /// </summary>
        private static readonly Regex FormatRegex = new Regex(@"(\.?)(png8|" + ExtensionRegexPattern + ")", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

        /// <summary>
        /// The image format regex for matching the file format at the end of a string.
        /// </summary>
        private static readonly Regex EndFormatRegex = new Regex(@"(\.)" + ExtensionRegexPattern + "$", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

        /// <summary>
        /// Checks a given string to check whether the value contains a valid image extension.
        /// </summary>
        /// <param name="fileName">The string containing the filename to check.</param>
        /// <returns>True the value contains a valid image extension, otherwise false.</returns>
        public static bool IsValidImageExtension(string fileName)
        {
            return EndFormatRegex.IsMatch(fileName);
        }

        /// <summary>
        /// Returns the correct file extension for the given string input.
        /// <remarks>
        /// Falls back to jpeg if no extension is matched.
        /// </remarks>
        /// </summary>
        /// <param name="fullPath">The string to parse.</param>
        /// <param name="queryString">The querystring containing instructions.</param>
        /// <returns>
        /// The correct file extension for the given string input if it can find one; otherwise an empty string.
        /// </returns>
        public string GetExtension(string fullPath, string queryString)
        {
            Match match = null;

            if (this.formatProcessor != null)
            {
                match = this.formatProcessor.RegexPattern.Match(queryString);
            }

            if (match == null || !match.Success)
            {
                // Test against the path minus the querystring so any other
                // processors don't interere.
                string trimmed = fullPath;
                if (!string.IsNullOrEmpty(queryString))
                {
                    trimmed = trimmed.Replace(queryString, string.Empty);
                }

                match = FormatRegex.Match(trimmed);
            }

            if (match.Success)
            {
                string value = match.Value;

                // Clip if format processor match.
                if (match.Value.Contains("="))
                {
                    value = value.Split('=')[1];
                }

                // Ah the enigma that is the png file.
                if (value.ToLowerInvariant().EndsWith("png8"))
                {
                    return "png";
                }

                return value;
            }

            // Fall back to jpg
            return "jpg";
        }

        /// <summary>
        /// Returns the content-type/mime-type for a given image type based on it's file extension
        /// </summary>
        /// <param name="extension">
        /// Can be prefixed with '.' or not (i.e. ".jpg"  or "jpg")
        /// </param>
        /// <returns>The <see cref="string"/></returns>
        internal string GetContentTypeForExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(extension));
            }

            extension = extension.TrimStart('.');

            ISupportedImageFormat found = ImageProcessorBootstrapper.Instance.SupportedImageFormats
                .FirstOrDefault(x => x.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

            if (found != null)
            {
                return found.MimeType;
            }

            // default
            return new JpegFormat().MimeType;
        }

        /// <summary>
        /// Builds a regular expression from the <see cref="T:ImageProcessor.Imaging.Formats.ISupportedImageFormat"/> type, this allows extensibility.
        /// </summary>
        /// <returns>
        /// The <see cref="Regex"/> to match matrix filters.
        /// </returns>
        private static string BuildExtensionRegexPattern()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            int counter = 0;
            foreach (ISupportedImageFormat imageFormat in ImageProcessorBootstrapper.Instance.SupportedImageFormats)
            {
                foreach (string fileExtension in imageFormat.FileExtensions)
                {
                    if (counter == 0)
                    {
                        stringBuilder.Append(fileExtension.ToLowerInvariant());
                    }
                    else
                    {
                        stringBuilder.AppendFormat("|{0}", fileExtension.ToLowerInvariant());
                    }
                }

                counter++;
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
    }
}