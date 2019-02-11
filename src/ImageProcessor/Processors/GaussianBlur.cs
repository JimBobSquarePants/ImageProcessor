// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GaussianBlur.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Applies a Gaussian blur to the image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Imaging;

    /// <summary>
    /// Applies a Gaussian blur to the image.
    /// </summary>
    public class GaussianBlur : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianBlur"/> class.
        /// </summary>
        public GaussianBlur() => this.Settings = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the DynamicParameter.
        /// </summary>
        public dynamic DynamicParameter { get; set; }

        /// <summary>
        /// Gets or sets any additional settings required by the processor.
        /// </summary>
        public Dictionary<string, string> Settings { get; set; }

        /// <summary>
        /// Processes the image.
        /// </summary>
        /// <param name="factory">The current instance of the <see cref="T:ImageProcessor.ImageFactory" /> class containing
        /// the image to process.</param>
        /// <returns>
        /// The processed image from the current instance of the <see cref="T:ImageProcessor.ImageFactory" /> class.
        /// </returns>
        public Image ProcessImage(ImageFactory factory)
        {
            var image = (Bitmap)factory.Image;

            try
            {
                GaussianLayer gaussianLayer = this.DynamicParameter;
                var convolution = new Convolution(gaussianLayer.Sigma) { Threshold = gaussianLayer.Threshold };
                double[,] kernel = convolution.CreateGuassianBlurFilter(gaussianLayer.Size);

                return convolution.ProcessKernel(image, kernel, factory.FixGamma);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }
        }
    }
}