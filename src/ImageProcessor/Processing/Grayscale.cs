// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Changes the grayscale component of the image using the formula as specified by ITU-R Recommendation BT.601.
    /// <see href="https://en.wikipedia.org/wiki/Luma_%28video%29#Rec._601_luma_versus_Rec._709_luma_coefficients"/>.
    /// </summary>
    public class Grayscale : ColorMatrixRangedProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Grayscale"/> class.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images opacity. Range 0..100.
        /// </param>
        public Grayscale(float percentage)
            : base(percentage)
        {
        }

        /// <inheritdoc/>
        public override Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            float amount = this.Options / 100;
            ColorMatrix colorMatrix = KnownColorMatrices.CreateGrayscaleFilter(amount);
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
