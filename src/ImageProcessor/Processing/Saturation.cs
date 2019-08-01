// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Changes the saturation component of the image.
    /// </summary>
    public class Saturation : ColorMatrixRangedProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Saturation"/> class.
        /// </summary>
        /// <param name="amount">
        /// The percentage by which to alter the images saturation. Range -100..100.
        /// </param>
        public Saturation(float amount)
            : base(amount)
        {
        }

        /// <inheritdoc/>
        public override Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            float amount = (this.Options + 100) / 100;
            ColorMatrix colorMatrix = KnownColorMatrices.CreateSaturationFilter(amount);
            this.ApplyMatrix(frame, colorMatrix);

            return frame;
        }

        /// <inheritdoc/>
        protected override void GuardRange(float amount)
        {
            if (amount < -100 || amount > 100)
            {
                throw new ImageProcessingException($"{nameof(amount)} {amount} must be in Range -100..100");
            }
        }
    }
}
