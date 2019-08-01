// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing.Imaging;

namespace ImageProcessor.Processing
{
    internal static class KnownColorMatrices
    {
        /// <summary>
        /// Create a brightness filter matrix using the given amount.
        /// </summary>
        /// <remarks>
        /// A value of 0 will create an image that is completely black. A value of 1 leaves the input unchanged.
        /// Other values are linear multipliers on the effect. Values of an amount over 1 are allowed, providing brighter results.
        /// </remarks>
        /// <param name="amount">The proportion of the conversion. Must be greater than or equal to 0.</param>
        /// <returns>The <see cref="ColorMatrix"/>.</returns>
        public static ColorMatrix CreateBrightnessFilter(float amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Threshold must be >= 0");
            }

            // See https://cs.chromium.org/chromium/src/cc/paint/render_surface_filters.cc
            return new ColorMatrix
            {
                Matrix00 = amount,
                Matrix11 = amount,
                Matrix22 = amount,
                Matrix33 = 1F
            };
        }

        /// <summary>
        /// Create a contrast filter matrix using the given amount.
        /// </summary>
        /// <remarks>
        /// A value of 0 will create an image that is completely gray. A value of 1 leaves the input unchanged.
        /// Other values are linear multipliers on the effect. Values of an amount over 1 are allowed, providing results with more contrast.
        /// </remarks>
        /// <param name="amount">The proportion of the conversion. Must be greater than or equal to 0.</param>
        /// <returns>The <see cref="ColorMatrix"/>.</returns>
        public static ColorMatrix CreateContrastFilter(float amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Threshold must be >= 0");
            }

            // See https://cs.chromium.org/chromium/src/cc/paint/render_surface_filters.cc
            float contrast = (-.5F * amount) + .5F;

            return new ColorMatrix
            {
                Matrix00 = amount,
                Matrix11 = amount,
                Matrix22 = amount,
                Matrix33 = 1F,
                Matrix40 = contrast,
                Matrix41 = contrast,
                Matrix42 = contrast
            };
        }

        /// <summary>
        /// Create a grayscale filter matrix using the given amount using the formula as specified by ITU-R Recommendation BT.601.
        /// <see href="https://en.wikipedia.org/wiki/Luma_%28video%29#Rec._601_luma_versus_Rec._709_luma_coefficients"/>.
        /// </summary>
        /// <param name="amount">The proportion of the conversion. Must be between 0 and 1.</param>
        /// <returns>The <see cref="ColorMatrix"/>.</returns>
        public static ColorMatrix CreateGrayscaleFilter(float amount)
        {
            if (amount < 0 || amount > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Threshold must be in range 0..1");
            }

            amount = 1F - amount;

            var m = new ColorMatrix();
            m.Matrix00 = .299F + (.701F * amount);
            m.Matrix10 = .587F - (.587F * amount);
            m.Matrix20 = 1F - (m.Matrix00 + m.Matrix10);

            m.Matrix01 = .299F - (.299F * amount);
            m.Matrix11 = .587F + (.2848F * amount);
            m.Matrix21 = 1F - (m.Matrix01 + m.Matrix11);

            m.Matrix02 = .299F - (.299F * amount);
            m.Matrix12 = .587F - (.587F * amount);
            m.Matrix22 = 1F - (m.Matrix02 + m.Matrix12);
            m.Matrix33 = 1F;

            return m;
        }

        /// <summary>
        /// Create a hue filter matrix using the given angle in degrees.
        /// </summary>
        /// <param name="degrees">The angle of rotation in degrees.</param>
        /// <returns>The <see cref="ColorMatrix"/>.</returns>
        public static ColorMatrix CreateHueFilter(float degrees)
        {
            // Wrap the angle round at 360.
            degrees %= 360;

            // Make sure it's not negative.
            while (degrees < 0)
            {
                degrees += 360;
            }

            float radian = GeometryUtilities.DegreeToRadian(degrees);
            float cosRadian = (float)Math.Cos(radian);
            float sinRadian = (float)Math.Sin(radian);

            // The matrix is set up to preserve the luminance of the image.
            // See http://graficaobscura.com/matrix/index.html
            // Number are taken from https://msdn.microsoft.com/en-us/library/jj192162(v=vs.85).aspx
            return new ColorMatrix
            {
                Matrix00 = .213F + (cosRadian * .787F) - (sinRadian * .213F),
                Matrix10 = .715F - (cosRadian * .715F) - (sinRadian * .715F),
                Matrix20 = .072F - (cosRadian * .072F) + (sinRadian * .928F),

                Matrix01 = .213F - (cosRadian * .213F) + (sinRadian * .143F),
                Matrix11 = .715F + (cosRadian * .285F) + (sinRadian * .140F),
                Matrix21 = .072F - (cosRadian * .072F) - (sinRadian * .283F),

                Matrix02 = .213F - (cosRadian * .213F) - (sinRadian * .787F),
                Matrix12 = .715F - (cosRadian * .715F) + (sinRadian * .715F),
                Matrix22 = .072F + (cosRadian * .928F) + (sinRadian * .072F),
                Matrix33 = 1F
            };
        }

        /// <summary>
        /// Create an opacity filter matrix using the given amount.
        /// </summary>
        /// <param name="amount">The proportion of the conversion. Must be between 0 and 1.</param>
        /// <returns>The <see cref="ColorMatrix"/>.</returns>
        public static ColorMatrix CreateOpacityFilter(float amount)
        {
            if (amount < 0 || amount > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Threshold must be in range 0..1");
            }

            // See https://cs.chromium.org/chromium/src/cc/paint/render_surface_filters.cc
            return new ColorMatrix
            {
                Matrix00 = 1F,
                Matrix11 = 1F,
                Matrix22 = 1F,
                Matrix33 = amount,
            };
        }

        /// <summary>
        /// Create a saturation filter matrix using the given amount.
        /// </summary>
        /// <remarks>
        /// A value of 0 is completely un-saturated. A value of 1 leaves the input unchanged.
        /// Other values are linear multipliers on the effect. Values of amount over 1 are allowed, providing super-saturated results.
        /// </remarks>
        /// <param name="amount">The proportion of the conversion. Must be greater than or equal to 0.</param>
        /// <returns>The <see cref="ColorMatrix"/>.</returns>
        public static ColorMatrix CreateSaturationFilter(float amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Threshold must be >= 0");
            }

            // See https://cs.chromium.org/chromium/src/cc/paint/render_surface_filters.cc
            var m = new ColorMatrix();
            m.Matrix00 = .213F + (.787F * amount);
            m.Matrix10 = .715F - (.715F * amount);
            m.Matrix20 = 1F - (m.Matrix00 + m.Matrix10);

            m.Matrix01 = .213F - (.213F * amount);
            m.Matrix11 = .715F + (.285F * amount);
            m.Matrix21 = 1F - (m.Matrix01 + m.Matrix11);

            m.Matrix02 = .213F - (.213F * amount);
            m.Matrix12 = .715F - (.715F * amount);
            m.Matrix22 = 1F - (m.Matrix02 + m.Matrix12);
            m.Matrix33 = 1F;

            return m;
        }
    }
}
