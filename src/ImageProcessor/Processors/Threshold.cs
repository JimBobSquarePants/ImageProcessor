// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Threshold.cs" company="Jonathan Norton">
//   Copyright (c) Jonathan Norton.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods to convert a image into black / white only based on a threshold.
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
    /// Encapsulates methods to perform a threshold function.
    /// </summary>
    public class Threshold : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Threshold"/> class.
        /// </summary>
        public Threshold() => this.Settings = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the dynamic parameter.
        /// </summary>
        public dynamic DynamicParameter { get; set; }

        /// <summary>
        /// Gets or sets any additional settings required by the processor.
        /// </summary>
        public Dictionary<string, string> Settings { get; set; }

        /// <summary>
        /// Processes the image.
        /// </summary>
        /// <param name="factory">
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class containing
        /// the image to process.
        /// </param>
        /// <returns>
        /// The processed image from the current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public Image ProcessImage(ImageFactory factory)
        {
            Image image = factory.Image;

            try
            {
                int threshold = this.DynamicParameter;

                // Valid numbers are 1 -> 255
                if (threshold < 1 || threshold > 255)
                {
                    throw new ArgumentOutOfRangeException("threshold must be in the range 1 -> 255");
                }

                Color white = Color.White;
                Color black = Color.Black;

                int width = image.Width;
                int height = image.Height;
                using (var fastBitmap = new FastBitmap(image))
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var original = fastBitmap.GetPixel(x, y);
                            Color altered = ((int)(original.GetBrightness() * 255) >= threshold) ? white : black;
                            fastBitmap.SetPixel(x, y, altered);
                        }
                    }
                }

                return image;
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }
        }
    }
}
