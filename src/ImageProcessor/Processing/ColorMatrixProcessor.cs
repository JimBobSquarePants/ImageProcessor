// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// A base class for processing images via color matrices.
    /// </summary>
    public abstract class ColorMatrixProcessor : IGraphicsProcessor
    {
        /// <inheritdoc/>
        public abstract Image ProcessImageFrame(ImageFactory factory, Image frame);

        /// <summary>
        /// Performs the application of the color matrix upon the image.
        /// </summary>
        /// <param name="frame">The image frame.</param>
        /// <param name="colorMatrix">The color matrix.</param>
        protected static void ApplyMatrix(Image frame, ColorMatrix colorMatrix)
        {
            using (var graphics = Graphics.FromImage(frame))
            {
                using (var imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(
                        colorMatrix,
                        ColorMatrixFlag.Default,
                        ColorAdjustType.Bitmap);

                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(
                        frame,
                        new Rectangle(0, 0, frame.Width, frame.Height),
                        0,
                        0,
                        frame.Width,
                        frame.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);
                }
            }
        }
    }
}
