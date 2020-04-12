// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CmykColor.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents an CMYK (cyan, magenta, yellow, keyline) color.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Colors
{
    using System;
    using System.Drawing;

    using ImageProcessor.Common.Extensions;
    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// Represents an CMYK (cyan, magenta, yellow, keyline) color.
    /// </summary>
    public readonly struct CmykColor : IEquatable<CmykColor>
    {
        /// <summary>
        /// Represents a <see cref="CmykColor"/> that is null.
        /// </summary>
        public static readonly CmykColor Empty = new CmykColor();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmykColor"/> struct.
        /// </summary>
        /// <param name="cyan">
        /// The cyan component.
        /// </param>
        /// <param name="magenta">
        /// The magenta component.
        /// </param>
        /// <param name="yellow">
        /// The yellow component.
        /// </param>
        /// <param name="keyline">
        /// The keyline black component.
        /// </param>
        private CmykColor(float cyan, float magenta, float yellow, float keyline)
        {
            this.C = Clamp(cyan);
            this.M = Clamp(magenta);
            this.Y = Clamp(yellow);
            this.K = Clamp(keyline);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmykColor"/> struct.
        /// </summary>
        /// <param name="color">
        /// The <see cref="System.Drawing.Color"/> to initialize from.
        /// </param>
        private CmykColor(Color color)
        {
            CmykColor cmykColor = color;
            this.C = cmykColor.C;
            this.M = cmykColor.M;
            this.Y = cmykColor.Y;
            this.K = cmykColor.K;
        }

        /// <summary>
        /// Gets the cyan component.
        /// <remarks>A value ranging between 0 and 100.</remarks>
        /// </summary>
        public float C { get; }

        /// <summary>
        /// Gets the magenta component.
        /// <remarks>A value ranging between 0 and 100.</remarks>
        /// </summary>
        public float M { get; }

        /// <summary>
        /// Gets the yellow component.
        /// <remarks>A value ranging between 0 and 100.</remarks>
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the keyline black component.
        /// <remarks>A value ranging between 0 and 100.</remarks>
        /// </summary>
        public float K { get; }

        /// <summary>
        /// Creates a <see cref="CmykColor"/> structure from the four 32-bit CMYK 
        /// components (cyan, magenta, yellow, and keyline) values.
        /// </summary>
        /// <param name="cyan">The cyan component.</param>
        /// <param name="magenta">The magenta component.</param>
        /// <param name="yellow">The yellow component.</param>
        /// <param name="keyline">The keyline black component.</param>
        /// <returns>
        /// The <see cref="CmykColor"/>.
        /// </returns>
        public static CmykColor FromCmykColor(float cyan, float magenta, float yellow, float keyline) => new CmykColor(cyan, magenta, yellow, keyline);

        /// <summary>
        /// Creates a <see cref="CmykColor"/> structure from the specified <see cref="System.Drawing.Color"/> structure
        /// </summary>
        /// <param name="color">
        /// The <see cref="System.Drawing.Color"/> from which to create the new <see cref="CmykColor"/>.
        /// </param>
        /// <returns>
        /// The <see cref="CmykColor"/>.
        /// </returns>
        public static CmykColor FromColor(Color color) => new CmykColor(color);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="System.Drawing.Color"/> to a 
        /// <see cref="CmykColor"/>.
        /// </summary>
        /// <param name="color">
        /// The instance of <see cref="System.Drawing.Color"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="CmykColor"/>.
        /// </returns>
        public static implicit operator CmykColor(Color color)
        {
            float c = (255f - color.R) / 255;
            float m = (255f - color.G) / 255;
            float y = (255f - color.B) / 255;

            float k = Math.Min(c, Math.Min(m, y));

            if (Math.Abs(k - 1.0) <= .0001f)
            {
                return new CmykColor(0, 0, 0, 100);
            }

            c = ((c - k) / (1 - k)) * 100;
            m = ((m - k) / (1 - k)) * 100;
            y = ((y - k) / (1 - k)) * 100;

            return new CmykColor(c, m, y, k * 100);
        }

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="RgbaColor"/> to a 
        /// <see cref="CmykColor"/>.
        /// </summary>
        /// <param name="rgbaColor">
        /// The instance of <see cref="RgbaColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="CmykColor"/>.
        /// </returns>
        public static implicit operator CmykColor(RgbaColor rgbaColor) => FromColor(rgbaColor);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="YCbCrColor"/> to a 
        /// <see cref="HslaColor"/>.
        /// </summary>
        /// <param name="ycbcrColor">
        /// The instance of <see cref="YCbCrColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="YCbCrColor"/>.
        /// </returns>
        public static implicit operator CmykColor(YCbCrColor ycbcrColor)
        {
            Color color = ycbcrColor;
            return FromColor(color);
        }

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="CmykColor"/> to a 
        /// <see cref="System.Drawing.Color"/>.
        /// </summary>
        /// <param name="cmykColor">
        /// The instance of <see cref="CmykColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="System.Drawing.Color"/>.
        /// </returns>
        public static implicit operator Color(CmykColor cmykColor)
        {
            int red = Convert.ToInt32((1 - (cmykColor.C / 100)) * (1 - (cmykColor.K / 100)) * 255.0);
            int green = Convert.ToInt32((1 - (cmykColor.M / 100)) * (1 - (cmykColor.K / 100)) * 255.0);
            int blue = Convert.ToInt32((1 - (cmykColor.Y / 100)) * (1 - (cmykColor.K / 100)) * 255.0);
            return Color.FromArgb(red.ToByte(), green.ToByte(), blue.ToByte());
        }

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="CmykColor"/> to a 
        /// <see cref="RgbaColor"/>.
        /// </summary>
        /// <param name="cmykColor">
        /// The instance of <see cref="CmykColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="RgbaColor"/>.
        /// </returns>
        public static implicit operator RgbaColor(CmykColor cmykColor)
        {
            int red = Convert.ToInt32((1 - (cmykColor.C / 100)) * (1 - (cmykColor.K / 100)) * 255.0);
            int green = Convert.ToInt32((1 - (cmykColor.M / 100)) * (1 - (cmykColor.K / 100)) * 255.0);
            int blue = Convert.ToInt32((1 - (cmykColor.Y / 100)) * (1 - (cmykColor.K / 100)) * 255.0);
            return RgbaColor.FromRgba(red.ToByte(), green.ToByte(), blue.ToByte());
        }

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="CmykColor"/> to a 
        /// <see cref="YCbCrColor"/>.
        /// </summary>
        /// <param name="cmykColor">
        /// The instance of <see cref="CmykColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="YCbCrColor"/>.
        /// </returns>
        public static implicit operator YCbCrColor(CmykColor cmykColor) => YCbCrColor.FromColor(cmykColor);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="CmykColor"/> to a 
        /// <see cref="HslaColor"/>.
        /// </summary>
        /// <param name="cmykColor">
        /// The instance of <see cref="CmykColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="HslaColor"/>.
        /// </returns>
        public static implicit operator HslaColor(CmykColor cmykColor) => HslaColor.FromColor(cmykColor);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.IsEmpty())
            {
                return "CmykColor [ Empty ]";
            }

            return string.Format("CmykColor [ C={0:#0.##}, M={1:#0.##}, Y={2:#0.##}, K={3:#0.##}]", this.C, this.M, this.Y, this.K);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is CmykColor cmykColor && this.Equals(cmykColor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(CmykColor other) =>
            this.C == other.C
            && this.M == other.M
            && this.Y == other.Y
            && this.K == other.K;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.C, this.M, this.Y, this.K).GetHashCode();

        /// <summary>
        /// Checks the range of the given value to ensure that it remains within the acceptable boundaries.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// The sanitized <see cref="float" />.
        /// </returns>
        private static float Clamp(float value) => ImageMaths.Clamp(value, 0, 100);

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </returns>
        private bool IsEmpty()
        {
            const float Epsilon = .0001f;
            return Math.Abs(this.C - 0) <= Epsilon && Math.Abs(this.M - 0) <= Epsilon
                   && Math.Abs(this.Y - 0) <= Epsilon && Math.Abs(this.K - 0) <= Epsilon;
        }
    }
}
