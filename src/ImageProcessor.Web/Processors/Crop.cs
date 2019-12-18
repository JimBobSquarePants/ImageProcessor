// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Crop.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Crops an image to the given directions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Processors
{
    using System.Text.RegularExpressions;
    using System.Web;

    using ImageProcessor.Imaging;
    using ImageProcessor.Processors;
    using ImageProcessor.Web.Helpers;

    /// <summary>
    /// Crops an image to the given directions.
    /// </summary>
    public class Crop : IWebGraphicsProcessor
    {
        /// <summary>
        /// The regular expression to search strings for.
        /// </summary>
        private static readonly Regex QueryRegex = new Regex(@"\b(?!\W+)crop\b[=]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="Crop"/> class.
        /// </summary>
        public Crop() => this.Processor = new ImageProcessor.Processors.Crop();

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

            var match = this.RegexPattern.Match(queryString);
            if (match.Success)
            {
                var queryCollection = HttpUtility.ParseQueryString(queryString);
                var coordinates = QueryParamParser.Instance.ParseValue<float[]>(queryCollection["crop"]);
                if (coordinates?.Length == 4)
                {
                    this.SortOrder = match.Index;

                    // Default CropMode.Pixels will be returned.
                    var cropMode = QueryParamParser.Instance.ParseValue<CropMode>(queryCollection["cropmode"]);
                    this.Processor.DynamicParameter = new CropLayer(coordinates[0], coordinates[1], coordinates[2], coordinates[3], cropMode);
                }
            }

            return this.SortOrder;
        }
    }
}
