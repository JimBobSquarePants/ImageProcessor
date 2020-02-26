// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Provides reusable adjustment methods to apply to images.
    /// </summary>
    public static class Adjustments
    {
        /// <summary>
        /// Adjust the gamma (intensity of the light) component of the given image.
        /// </summary>
        /// <param name="source">The <see cref="Image"/> source to adjust.</param>
        /// <param name="value">The value to adjust the gamma by (typically between .2 and 5).</param>
        /// <returns>
        /// The <see cref="Bitmap"/> with the gamma adjusted.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the value falls outside the acceptable range.
        /// </exception>
        public static Bitmap Gamma(Image source, float value)
        {
            if (value > 5 || value < .1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value should be between .1 and 5.");
            }

            byte[] ramp = new byte[256];
            for (int x = 0; x < 256; ++x)
            {
                ramp[x] = ((255 * Math.Pow(x / 255D, value)) + 0.5).ToByte();
            }

            int width = source.Width;
            int height = source.Height;

            using (var bitmap = new FastBitmap(source))
            {
                Parallel.For(
                    0,
                    height,
                    y =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Color composite = bitmap.GetPixel(x, y);
                            var linear = Color.FromArgb(composite.A, ramp[composite.R], ramp[composite.G], ramp[composite.B]);
                            bitmap.SetPixel(x, y, linear);
                        }
                    });
            }

            return (Bitmap)source;
        }
    }
}
