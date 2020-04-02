// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Adjustments.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides reusable adjustment methods to apply to images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Helpers
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using ImageProcessor.Common.Extensions;

    /// <summary>
    /// Provides reusable adjustment methods to apply to images.
    /// </summary>
    public static class Adjustments
    {
        /// <summary>
        /// The array of bytes representing each possible value of color component
        /// converted from sRGB to the linear color space.
        /// </summary>
        private static readonly Lazy<byte[]> LinearBytes = new Lazy<byte[]>(GetLinearBytes);

        /// <summary>
        /// The array of bytes representing each possible value of color component
        /// converted from linear to the sRGB color space.
        /// </summary>
        private static readonly Lazy<byte[]> SRGBBytes = new Lazy<byte[]>(GetSRGBBytes);

        /// <summary>
        /// Adjusts the alpha component of the given image.
        /// </summary>
        /// <param name="source">
        /// The <see cref="Image"/> source to adjust.
        /// </param>
        /// <param name="percentage">
        /// The percentage value between 0 and 100 for adjusting the opacity.
        /// </param>
        /// <param name="rectangle">The rectangle to define the bounds of the area to adjust the opacity. 
        /// If null then the effect is applied to the entire image.</param>
        /// <returns>
        /// The <see cref="Bitmap"/> with the alpha component adjusted.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the percentage value falls outside the acceptable range.
        /// </exception>
        public static Bitmap Alpha(Image source, int percentage, Rectangle? rectangle = null)
        {
            if (percentage > 100 || percentage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage should be between 0 and 100.");
            }

            float factor = (float)percentage / 100;

            Rectangle bounds = rectangle ?? new Rectangle(0, 0, source.Width, source.Height);

            // Traditional examples using a color matrix alter the rgb values also.
            using (var bitmap = new FastBitmap(source))
            {
                // Loop through the pixels.
                Parallel.For(
                    bounds.Y,
                    bounds.Bottom,
                    y =>
                    {
                        for (int x = bounds.X; x < bounds.Right; x++)
                        {
                            // ReSharper disable AccessToDisposedClosure
                            Color color = bitmap.GetPixel(x, y);
                            bitmap.SetPixel(x, y, Color.FromArgb(Convert.ToInt32(color.A * factor), color.R, color.G, color.B));
                            // ReSharper restore AccessToDisposedClosure
                        }
                    });
            }

            return (Bitmap)source;
        }

        /// <summary>
        /// Adjusts the brightness component of the given image.
        /// </summary>
        /// <param name="source">
        /// The <see cref="Image"/> source to adjust.
        /// </param>
        /// <param name="threshold">
        /// The threshold value between -100 and 100 for adjusting the brightness.
        /// </param>
        /// <param name="rectangle">The rectangle to define the bounds of the area to adjust the brightness. 
        /// If null then the effect is applied to the entire image.</param>
        /// <returns>
        /// The <see cref="Bitmap"/> with the brightness adjusted.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the threshold value falls outside the acceptable range.
        /// </exception>
        public static Bitmap Brightness(Image source, int threshold, Rectangle? rectangle = null)
        {
            if (threshold > 100 || threshold < -100)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold should be between -100 and 100.");
            }

            float brightnessFactor = (float)threshold / 100;
            Rectangle bounds = rectangle ?? new Rectangle(0, 0, source.Width, source.Height);

            var colorMatrix =
                new ColorMatrix(
                    new[]
                        {
                            new float[] { 1, 0, 0, 0, 0 },
                            new float[] { 0, 1, 0, 0, 0 },
                            new float[] { 0, 0, 1, 0, 0 },
                            new float[] { 0, 0, 0, 1, 0 },
                            new[] { brightnessFactor, brightnessFactor, brightnessFactor, 0, 1 }
                        });

            using (var graphics = Graphics.FromImage(source))
            {
                using (var imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(colorMatrix);
                    graphics.DrawImage(source, bounds, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }

            return (Bitmap)source;
        }

        /// <summary>
        /// Adjusts the contrast component of the given image.
        /// </summary>
        /// <param name="source">
        /// The <see cref="Image"/> source to adjust.
        /// </param>
        /// <param name="threshold">
        /// The threshold value between -100 and 100 for adjusting the contrast.
        /// </param>
        /// <param name="rectangle">The rectangle to define the bounds of the area to adjust the contrast. 
        /// If null then the effect is applied to the entire image.</param>
        /// <returns>
        /// The <see cref="Bitmap"/> with the contrast adjusted.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the threshold value falls outside the acceptable range.
        /// </exception>
        public static Bitmap Contrast(Image source, int threshold, Rectangle? rectangle = null)
        {
            if (threshold > 100 || threshold < -100)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold should be between -100 and 100.");
            }

            Rectangle bounds = rectangle ?? new Rectangle(0, 0, source.Width, source.Height);

            float contrastFactor = (float)threshold / 100;

            // Stop at -1 to prevent inversion.
            contrastFactor++;
            float factorTransform = 0.5f * (1.0f - contrastFactor);

            var colorMatrix =
                new ColorMatrix(
                    new[]
                    {
                        new[] { contrastFactor, 0, 0, 0, 0 },
                        new[] { 0, contrastFactor, 0, 0, 0 },
                        new[] { 0, 0, contrastFactor, 0, 0 },
                        new float[] { 0, 0, 0, 1, 0 },
                        new[] { factorTransform, factorTransform, factorTransform, 0, 1 }
                    });

            using (var graphics = Graphics.FromImage(source))
            {
                using (var imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(colorMatrix);
                    graphics.DrawImage(source, bounds, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }

            return (Bitmap)source;
        }

        /// <summary>
        /// Adjust the gamma (intensity of the light) component of the given image.
        /// </summary>
        /// <param name="source">
        /// The <see cref="Image"/> source to adjust.
        /// </param>
        /// <param name="value">
        /// The value to adjust the gamma by (typically between .2 and 5).
        /// </param>
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
                ramp[x] = ((255.0 * Math.Pow(x / 255.0, value)) + 0.5).ToByte();
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
                            // ReSharper disable once AccessToDisposedClosure
                            Color composite = bitmap.GetPixel(x, y);
                            var linear = Color.FromArgb(composite.A, ramp[composite.R], ramp[composite.G], ramp[composite.B]);
                            // ReSharper disable once AccessToDisposedClosure
                            bitmap.SetPixel(x, y, linear);
                        }
                    });
            }

            return (Bitmap)source;
        }

        /// <summary>
        /// Converts an image from an sRGB color-space to the equivalent linear color-space.
        /// </summary>
        /// <param name="source">
        /// The <see cref="Image"/> source to convert.
        /// </param>
        /// <returns>
        /// The <see cref="Bitmap"/>.
        /// </returns>
        public static Bitmap ToLinear(Image source)
        {
            // Create only once and lazily. 
            byte[] ramp = LinearBytes.Value;

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
                            // ReSharper disable once AccessToDisposedClosure
                            Color composite = bitmap.GetPixel(x, y);
                            var linear = Color.FromArgb(composite.A, ramp[composite.R], ramp[composite.G], ramp[composite.B]);
                            // ReSharper disable once AccessToDisposedClosure
                            bitmap.SetPixel(x, y, linear);
                        }
                    });
            }

            return (Bitmap)source;
        }

        /// <summary>
        /// Converts an image from a linear color-space to the equivalent sRGB color-space.
        /// </summary>
        /// <param name="source">
        /// The <see cref="Image"/> source to convert.
        /// </param>
        /// <returns>
        /// The <see cref="Bitmap"/>.
        /// </returns>
        public static Bitmap ToSRGB(Image source)
        {
            // Create only once and lazily. 
            byte[] ramp = SRGBBytes.Value;

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
                            // ReSharper disable once AccessToDisposedClosure
                            Color composite = bitmap.GetPixel(x, y);
                            var linear = Color.FromArgb(composite.A, ramp[composite.R], ramp[composite.G], ramp[composite.B]);
                            // ReSharper disable once AccessToDisposedClosure
                            bitmap.SetPixel(x, y, linear);
                        }
                    });
            }

            return (Bitmap)source;
        }

        /// <summary>
        /// Gets an array of bytes representing each possible value of color component
        /// converted from sRGB to the linear color space.
        /// </summary>
        /// <returns>
        /// The <see cref="T:byte[]"/>.
        /// </returns>
        private static byte[] GetLinearBytes()
        {
            byte[] ramp = new byte[256];
            for (int x = 0; x < 256; ++x)
            {
                ramp[x] = ((255 * SRGBToLinear(x / 255D)) + .5).ToByte();
            }

            return ramp;
        }

        /// <summary>
        /// Gets an array of bytes representing each possible value of color component
        /// converted from linear to the sRGB color space.
        /// </summary>
        /// <returns>
        /// The <see cref="T:byte[]"/>.
        /// </returns>
        private static byte[] GetSRGBBytes()
        {
            byte[] ramp = new byte[256];
            for (int x = 0; x < 256; ++x)
            {
                ramp[x] = ((255 * LinearToSRGB(x / 255D)) + .5).ToByte();
            }

            return ramp;
        }

        /// <summary>
        /// Gets the correct linear value from an sRGB signal.
        /// <see href="http://www.4p8.com/eric.brasseur/gamma.html#formulas"/>
        /// <see href="http://entropymine.com/imageworsener/srgbformula/"/>
        /// </summary>
        /// <param name="signal">The signal value to convert.</param>
        /// <returns>
        /// The <see cref="float"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double SRGBToLinear(double signal)
        {
            if (signal <= 0.04045)
            {
                return signal / 12.92;
            }

            return Math.Pow((signal + 0.055) / 1.055, 2.4);
        }

        /// <summary>
        /// Gets the correct sRGB value from an linear signal.
        /// <see href="http://www.4p8.com/eric.brasseur/gamma.html#formulas"/>
        /// <see href="http://entropymine.com/imageworsener/srgbformula/"/>
        /// </summary>
        /// <param name="signal">The signal value to convert.</param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double LinearToSRGB(double signal)
        {
            if (signal <= (0.04045 / 12.92))
            {
                return signal * 12.92;
            }

            return (1.055 * Math.Pow(signal, 1.0 / 2.4)) - 0.055;
        }
    }
}
