// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Changes the alpha component of the image.
    /// </summary>
    public class Alpha : ColorMatrixRangedProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Alpha"/> class.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images opacity. Range 0..100.
        /// </param>
        public Alpha(float percentage)
            : base(percentage)
        {
        }

        /// <inheritdoc/>
        public override Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            float amount = this.Options / 100;
            ColorMatrix colorMatrix = KnownColorMatrices.CreateOpacityFilter(amount);
            this.ApplyMatrix(frame, colorMatrix);

            return frame;
        }

        /// <inheritdoc/>
        protected override void GuardRange(float amount)
        {
            if (amount < 0 || amount > 100)
            {
                throw new ImageProcessingException($"{nameof(amount)} must be in range 0..100");
            }
        }
    }
}
