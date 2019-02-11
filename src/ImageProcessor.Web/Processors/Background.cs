// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Overlay.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Adds an image background to the current image.
//   If the background is larger than the image it will be scaled to match the image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Processors
{
    using System.Collections.Specialized;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Hosting;

    using ImageProcessor.Imaging;
    using ImageProcessor.Processors;
    using ImageProcessor.Web.Helpers;

    /// <summary>
    /// Adds an image background to the current image. 
    /// If the background is larger than the image it will be scaled to match the image.
    /// </summary>
    public class Background : IWebGraphicsProcessor
    {
        /// <summary>
        /// The regular expression to search strings for.
        /// </summary>
        private static readonly Regex QueryRegex = new Regex(@"background=[\w+-]+." + ImageHelpers.ExtensionRegexPattern);

        /// <summary>
        /// Initializes a new instance of the <see cref="Background"/> class.
        /// </summary>
        public Background() => this.Processor = new ImageProcessor.Processors.Background();

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
        /// <param name="queryString">The query string to search.</param>
        /// <returns>
        /// The zero-based starting position in the original string where the captured substring was found.
        /// </returns>
        public int MatchRegexIndex(string queryString)
        {
            this.SortOrder = int.MaxValue;
            Match match = this.RegexPattern.Match(queryString);

            if (match.Success)
            {
                this.SortOrder = match.Index;
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);
                Image image = this.ParseImage(queryCollection["background"]);

                Point? position = queryCollection["background.position"] != null
                      ? QueryParamParser.Instance.ParseValue<Point>(queryCollection["background.position"])
                      : (Point?)null;

                int opacity = queryCollection["background.opacity"] != null
                                  ? QueryParamParser.Instance.ParseValue<int>(queryCollection["background.opacity"])
                                  : 100;
                Size size = QueryParamParser.Instance.ParseValue<Size>(queryCollection["background.size"]);

                this.Processor.DynamicParameter = new ImageLayer
                {
                    Image = image,
                    Position = position,
                    Opacity = opacity,
                    Size = size
                };
            }

            return this.SortOrder;
        }

        /// <summary>
        /// Returns an image from the given input path.
        /// </summary>
        /// <param name="input">
        /// The input containing the value to parse.
        /// </param>
        /// <returns>
        /// The <see cref="Image"/> representing the given image path.
        /// </returns>
        public Image ParseImage(string input)
        {
            Image image = null;

            // Correctly parse the path.
            this.Processor.Settings.TryGetValue("VirtualPath", out string path);

            if (!string.IsNullOrWhiteSpace(path) && path.StartsWith("~/"))
            {
                string imagePath = HostingEnvironment.MapPath(path);
                if (imagePath != null)
                {
                    imagePath = Path.Combine(imagePath, input);
                    try
                    {
                        using (var factory = new ImageFactory())
                        {
                            factory.Load(imagePath);
                            image = new Bitmap(factory.Image);
                        }
                    }
                    catch
                    {
                        throw new HttpException((int)HttpStatusCode.NotFound, "No image exists at " + imagePath);
                    }
                }
            }

            return image;
        }
    }
}
