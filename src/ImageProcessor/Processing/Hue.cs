// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Changes the hue component of the image.
    /// </summary>
    public class Hue : ColorMatrixRangedProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hue"/> class.
        /// </summary>
        /// <param name="degrees">The rotation angle in degrees to adjust the hue.</param>
        public Hue(float degrees)
            : base(degrees)
        {
        }

        /// <inheritdoc/>
        public override Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            ColorMatrix colorMatrix = KnownColorMatrices.CreateHueFilter(this.Options);
            this.ApplyMatrix(frame, colorMatrix);

            return frame;
        }
    }
}
