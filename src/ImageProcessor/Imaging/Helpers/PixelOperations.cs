// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PixelOperations.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Performs per-pixel operations.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Helpers
{
    using System;
    using System.Drawing;
    using ImageProcessor.Common.Extensions;

    /// <summary>
    /// Performs per-pixel operations.
    /// </summary>
    public static class PixelOperations
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
        /// The array of bytes representing each possible value of color component
        /// converted from gamma to the linear color space.
        /// </summary>
        private static readonly Lazy<byte[]> LinearGammaBytes = new Lazy<byte[]>(GetLinearGammaBytes);

        /// <summary>
        /// The array of bytes representing each possible value of color component
        /// converted from linear to the gamma color space.
        /// </summary>
        private static readonly Lazy<byte[]> GammaLinearBytes = new Lazy<byte[]>(GetGammaLinearBytes);

        /// <summary>
        /// Returns the given color adjusted by the given gamma value.
        /// </summary>
        /// <param name="color">
        /// The <see cref="Color"/> to adjust.
        /// </param>
        /// <param name="value">
        /// The gamma value - Between .1 and 5.
        /// </param>
        /// <returns>
        /// The adjusted <see cref="Color"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the given gamma value is out with the acceptable range.
        /// </exception>
        public static Color Gamma(Color color, float value)
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

            byte r = ramp[color.R];
            byte g = ramp[color.G];
            byte b = ramp[color.B];

            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Converts an pixel from an sRGB color-space to the equivalent linear color-space.
        /// </summary>
        /// <param name="composite">
        /// The <see cref="Color"/> to convert.
        /// </param>
        /// <returns>
        /// The <see cref="Color"/>.
        /// </returns>
        public static Color ToLinear(Color composite)
        {
            // Create only once and lazily. 
            byte[] ramp = LinearBytes.Value;

            return Color.FromArgb(composite.A, ramp[composite.R], ramp[composite.G], ramp[composite.B]);
        }

        /// <summary>
        /// Converts an image from a linear color-space to the equivalent sRGB color-space.
        /// </summary>
        /// <param name="linear">
        /// The <see cref="Color"/> to convert.
        /// </param>
        /// <returns>
        /// The <see cref="Bitmap"/>.
        /// </returns>
        public static Color ToSRGB(Color linear)
        {
            // Create only once and lazily. 
            byte[] ramp = SRGBBytes.Value;

            return Color.FromArgb(linear.A, ramp[linear.R], ramp[linear.G], ramp[linear.B]);
        }

        /// <summary>
        /// Converts an pixel from a gamma color-space to the equivalent linear color-space.
        /// </summary>
        /// <param name="composite">
        /// The <see cref="Color"/> to convert.
        /// </param>
        /// <returns>
        /// The <see cref="Color"/>.
        /// </returns>
        public static Color ToLinearFromGamma(Color composite)
        {
            // Create only once and lazily. 
            byte[] ramp = LinearGammaBytes.Value;

            return Color.FromArgb(composite.A, ramp[composite.R], ramp[composite.G], ramp[composite.B]);
        }

        /// <summary>
        /// Converts an pixel from a linear color-space to the equivalent gamma color-space.
        /// </summary>
        /// <param name="composite">
        /// The <see cref="Color"/> to convert.
        /// </param>
        /// <returns>
        /// The <see cref="Color"/>.
        /// </returns>
        public static Color ToGammaFromLinear(Color composite)
        {
            // Create only once and lazily. 
            byte[] ramp = GammaLinearBytes.Value;

            return Color.FromArgb(composite.A, ramp[composite.R], ramp[composite.G], ramp[composite.B]);
        }

        /// <summary>
        /// Gets an array of bytes representing each possible value of color component
        /// converted from gamma to the linear color space.
        /// </summary>
        /// <returns>
        /// The <see cref="T:byte[]"/>.
        /// </returns>
        private static byte[] GetLinearGammaBytes()
        {
            byte[] ramp = new byte[256];
            for (int x = 0; x < 256; ++x)
            {
                ramp[x] = (255f * Math.Pow(x / 255f, 2.2)).ToByte();
            }

            return ramp;
        }

        /// <summary>
        /// Gets an array of bytes representing each possible value of color component
        /// converted from linear to the gamma color space.
        /// </summary>
        /// <returns>
        /// The <see cref="T:byte[]"/>.
        /// </returns>
        private static byte[] GetGammaLinearBytes()
        {
            byte[] ramp = new byte[256];
            for (int x = 0; x < 256; ++x)
            {
                ramp[x] = (255f * Math.Pow(x / 255f, 1 / 2.2)).ToByte();
            }

            return ramp;
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
                ramp[x] = (255f * SRGBToLinear(x / 255f)).ToByte();
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
                ramp[x] = (255f * LinearToSRGB(x / 255f)).ToByte();
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
        private static float SRGBToLinear(float signal)
        {
            const float a = 0.055f;

            if (signal <= 0.04045)
            {
                return signal / 12.92f;
            }

            return (float)Math.Pow((signal + a) / (1 + a), 2.4);
        }

        /// <summary>
        /// Gets the correct sRGB value from an linear signal.
        /// <see href="http://www.4p8.com/eric.brasseur/gamma.html#formulas"/>
        /// <see href="http://entropymine.com/imageworsener/srgbformula/"/>
        /// </summary>
        /// <param name="signal">The signal value to convert.</param>
        /// <returns>
        /// The <see cref="float"/>.
        /// </returns>
        private static float LinearToSRGB(float signal)
        {
            const float a = 0.055f;

            if (signal <= 0.0031308)
            {
                return signal * 12.92f;
            }

            return ((float)((1 + a) * Math.Pow(signal, 1 / 2.4f))) - a;
        }
    }
}
