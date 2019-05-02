// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Format.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Sets the output of the image to a specific format.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Processors
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using ImageProcessor.Configuration;
    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Processors;
    using ImageProcessor.Web.Helpers;

    /// <summary>
    /// Sets the output of the image to a specific format.
    /// </summary>
    public class Format : IWebGraphicsProcessor
    {
        /// <summary>
        /// The regular expression to search strings for.
        /// </summary>
        private static readonly Regex QueryRegex = new Regex("format=(png8|" + ImageHelpers.ExtensionRegexPattern + ")", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="Format"/> class.
        /// </summary>
        public Format() => this.Processor = new ImageProcessor.Processors.Format();

        /// <summary>
        /// Gets the regular expression to search strings for.
        /// </summary>
        public Regex RegexPattern => QueryRegex;

        /// <summary>
        /// Gets the order in which this processor is to be used in a chain.
        /// </summary>
        public int SortOrder { get; private set; }

        /// <summary>
        /// Gets the associated graphics processor.
        /// </summary>
        public IGraphicsProcessor Processor { get; }

        /// <summary>
        /// The position in the original string where the first character of the captured substring was found.
        /// </summary>
        /// <param name="queryString">
        /// The query string to search.
        /// </param>
        /// <returns>
        /// The zero-based starting position in the original string where the captured substring was found.
        /// </returns>
        public int MatchRegexIndex(string queryString)
        {
            this.SortOrder = int.MaxValue;

            Match match = this.RegexPattern.Match(queryString);
            if (match.Success)
            {
                ISupportedImageFormat format = this.ParseFormat(match.Value.Split('=')[1]);
                if (format != null)
                {
                    this.SortOrder = match.Index;
                    this.Processor.DynamicParameter = format;
                }
            }

            return this.SortOrder;
        }

        /// <summary>
        /// Parses the input string to return the correct <see cref="ISupportedImageFormat"/>.
        /// </summary>
        /// <param name="identifier">
        /// The identifier.
        /// </param>
        /// <returns>
        /// The <see cref="ISupportedImageFormat"/>.
        /// </returns>
        private ISupportedImageFormat ParseFormat(string identifier)
        {
            identifier = identifier.ToLowerInvariant();
            string finalIdentifier = identifier.Equals("png8") ? "png" : identifier;
            ISupportedImageFormat newFormat = null;
            var formats = ImageProcessorBootstrapper.Instance.SupportedImageFormats.ToList();
            ISupportedImageFormat format = formats.Find(f => f.FileExtensions.Any(e => e.Equals(finalIdentifier, StringComparison.InvariantCultureIgnoreCase)));

            if (format != null)
            {
                // Return a new instance as we want to use instance properties.
                newFormat = Activator.CreateInstance(format.GetType()) as ISupportedImageFormat;

                if (newFormat != null)
                {
                    // I wish this wasn't hard-coded but there's no way I can
                    // find to preserve the palette.
                    if (identifier.Equals("png8"))
                    {
                        newFormat.IsIndexed = true;
                    }
                    else if (identifier.Equals("png"))
                    {
                        newFormat.IsIndexed = false;
                    }
                }
            }

            return newFormat;
        }
    }
}