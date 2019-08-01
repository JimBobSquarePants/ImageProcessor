// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Changes the contrast component of the image.
    /// </summary>
    public class Contrast : ColorMatrixRangedProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contrast"/> class.
        /// </summary>
        /// <param name="amount">
        /// The percentage by which to alter the images contrast. Range -100..100.
        /// </param>
        public Contrast(float amount)
            : base(amount)
        {
        }

        /// <inheritdoc/>
        public override Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            float amount = (this.Options + 100) / 100;
            ColorMatrix colorMatrix = KnownColorMatrices.CreateContrastFilter(amount);
            this.ApplyMatrix(frame, colorMatrix);

            return frame;
        }

        /// <inheritdoc/>
        protected override void GuardRange(float amount)
        {
            if (amount < -100 || amount > 100)
            {
                throw new ImageProcessingException($"{nameof(amount)} must be in Range -100..100");
            }
        }
    }
}
