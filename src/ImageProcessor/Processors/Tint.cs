// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tint.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Tints an image with the given color.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// Tints an image with the given color.
    /// </summary>
    public class Tint : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tint"/> class.
        /// </summary>
        public Tint() => this.Settings = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets DynamicParameter.
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
            Bitmap newImage = null;
            Image image = factory.Image;

            try
            {
                Color tintColour = this.DynamicParameter;
                float[][] colorMatrixElements =
                    {
                        new[] { tintColour.R / 255f, 0, 0, 0, 0 }, // Red 
                        new[] { 0, tintColour.G / 255f, 0, 0, 0 }, // Green 
                        new[] { 0, 0, tintColour.B / 255f, 0, 0 }, // Blue  
                        new[] { 0, 0, 0, tintColour.A / 255f, 0 }, // Alpha 
                        new float[] { 0, 0, 0, 0, 1 }
                    };

                var colorMatrix = new ColorMatrix(colorMatrixElements);
                newImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb);
                newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(newImage))
                {
                    GraphicsHelper.SetGraphicsOptions(graphics);

                    using (var attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);

                        image.Dispose();
                        image = newImage;
                    }
                }
            }
            catch (Exception ex)
            {
                newImage?.Dispose();

                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }

            return image;
        }
    }
}