// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Hue.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods to rotate the hue component of an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Imaging;
    using ImageProcessor.Imaging.Colors;

    /// <summary>
    /// Encapsulates methods to adjust the hue component of an image.
    /// </summary>
    public class Hue : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hue"/> class.
        /// </summary>
        public Hue() => this.Settings = new Dictionary<string, string>();

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
                Tuple<int, bool> parameters = this.DynamicParameter;
                int degrees = parameters.Item1;
                bool rotate = parameters.Item2;

                if (degrees == 0 && rotate)
                {
                    return image;
                }

                int width = image.Width;
                int height = image.Height;
                using (var fastBitmap = new FastBitmap(image))
                {
                    if (!rotate)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                var original = HslaColor.FromColor(fastBitmap.GetPixel(x, y));
                                var altered = HslaColor.FromHslaColor(degrees / 360f, original.S, original.L, original.A);
                                fastBitmap.SetPixel(x, y, altered);
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                var original = HslaColor.FromColor(fastBitmap.GetPixel(x, y));
                                var altered = HslaColor.FromHslaColor((original.H + (degrees / 360f)) % 1, original.S, original.L, original.A);
                                fastBitmap.SetPixel(x, y, altered);
                            }
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
