// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageFactoryExtensions.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Extends the ImageFactory class to provide a fluent API.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web
{
    using System;
    using System.Linq;

    using ImageProcessor.Web.Configuration;
    using ImageProcessor.Web.Processors;

    /// <summary>
    /// Extends the ImageFactory class to provide a fluent API.
    /// </summary>
    public static class ImageFactoryExtensions
    {
        /// <summary>
        /// Auto processes image files based on any query string parameters added to the image path.
        /// </summary>
        /// <param name="factory">
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class
        /// that this method extends.
        /// </param>
        /// <param name="graphicsProcessors">The array of graphics processors to apply.</param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        internal static ImageFactory AutoProcess(this ImageFactory factory, IWebGraphicsProcessor[] graphicsProcessors)
        {
            if (factory.ShouldProcess)
            {
                // Loop through and process the image.
                foreach (IWebGraphicsProcessor graphicsProcessor in graphicsProcessors)
                {
                    factory.CurrentImageFormat.ApplyProcessor(graphicsProcessor.Processor.ProcessImage, factory);

                    // Unwrap the dynamic parameter and dispose of any types that require it.
                    IDisposable disposable = graphicsProcessor.Processor.DynamicParameter as IDisposable;
                    disposable?.Dispose();
                }
            }

            return factory;
        }

        /// <summary>
        /// Returns an array of processors that match the given querystring.
        /// </summary>
        /// <param name="querystring">The collection of querystring parameters to process.</param>
        /// <returns>
        /// The <see cref="T:IWebGraphicsProcessor[]"/>.
        /// </returns>
        internal static IWebGraphicsProcessor[] GetMatchingProcessors(string querystring)
        {
            // Get a list of all graphics processors that have parsed and matched the query string.
            return ImageProcessorConfiguration.Instance.CreateWebGraphicsProcessors()
                    .Where(x => x.MatchRegexIndex(querystring) != int.MaxValue)
                    .OrderBy(y => y.SortOrder)
                    .ToArray();
        }
    }
}
