// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YCbCrColor.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents an YCbCr (luminance, chroma, chroma) color conforming to the ITU-R BT.601 standard used in digital imaging systems.
//   <see href="http://en.wikipedia.org/wiki/YCbCr" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Colors
{
    using System;
    using System.Drawing;

    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// Represents an YCbCr (luminance, chroma, chroma) color conforming to the ITU-R BT.601 standard used in digital imaging systems.
    /// <see href="http://en.wikipedia.org/wiki/YCbCr"/>
    /// </summary>
    public readonly struct YCbCrColor : IEquatable<YCbCrColor>
    {
        /// <summary>
        /// Represents a <see cref="YCbCrColor"/> that is null.
        /// </summary>
        public static readonly YCbCrColor Empty = new YCbCrColor();

        /// <summary>
        /// Initializes a new instance of the <see cref="YCbCrColor"/> struct.
        /// </summary>
        /// <param name="y">The y luminance component.</param>
        /// <param name="cb">The u chroma component.</param>
        /// <param name="cr">The v chroma component.</param> 
        private YCbCrColor(float y, float cb, float cr)
        {
            this.Y = ImageMaths.Clamp(y, 0, 255);
            this.Cb = ImageMaths.Clamp(cb, 0, 255);
            this.Cr = ImageMaths.Clamp(cr, 0, 255);
        }

        /// <summary>
        /// Gets the Y luminance component.
        /// <remarks>A value ranging between 0 and 255.</remarks>
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the U chroma component.
        /// <remarks>A value ranging between 0 and 255.</remarks>
        /// </summary>
        public float Cb { get; }

        /// <summary>
        /// Gets the V chroma component.
        /// <remarks>A value ranging between 0 and 255.</remarks>
        /// </summary>
        public float Cr { get; }

        /// <summary>
        /// Creates a <see cref="YCbCrColor"/> structure from the three 32-bit YCbCr 
        /// components (luminance, chroma, and chroma) values.
        /// </summary>
        /// <param name="y">The y luminance component.</param>
        /// <param name="cb">The u chroma component.</param>
        /// <param name="cr">The v chroma component.</param> 
        /// <returns>
        /// The <see cref="YCbCrColor"/>.
        /// </returns>
        public static YCbCrColor FromYCbCr(float y, float cb, float cr) => new YCbCrColor(y, cb, cr);

        /// <summary>
        /// Creates a <see cref="YCbCrColor"/> structure from the specified <see cref="System.Drawing.Color"/> structure
        /// </summary>
        /// <param name="color">
        /// The <see cref="System.Drawing.Color"/> from which to create the new <see cref="YCbCrColor"/>.
        /// </param>
        /// <returns>
        /// The <see cref="YCbCrColor"/>.
        /// </returns>
        public static YCbCrColor FromColor(Color color)
        {
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;

            float y = (float)((0.299 * r) + (0.587 * g) + (0.114 * b));
            float cb = 128 + (float)((-0.168736 * r) - (0.331264 * g) + (0.5 * b));
            float cr = 128 + (float)((0.5 * r) - (0.418688 * g) - (0.081312 * b));

            return new YCbCrColor(y, cb, cr);
        }

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="System.Drawing.Color"/> to a 
        /// <see cref="YCbCrColor"/>.
        /// </summary>
        /// <param name="color">
        /// The instance of <see cref="System.Drawing.Color"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="YCbCrColor"/>.
        /// </returns>
        public static implicit operator YCbCrColor(Color color) => FromColor(color);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="RgbaColor"/> to a 
        /// <see cref="YCbCrColor"/>.
        /// </summary>
        /// <param name="rgbaColor">
        /// The instance of <see cref="RgbaColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="YCbCrColor"/>.
        /// </returns>
        public static implicit operator YCbCrColor(RgbaColor rgbaColor) => FromColor(rgbaColor);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="HslaColor"/> to a 
        /// <see cref="YCbCrColor"/>.
        /// </summary>
        /// <param name="hslaColor">
        /// The instance of <see cref="HslaColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="YCbCrColor"/>.
        /// </returns>
        public static implicit operator YCbCrColor(HslaColor hslaColor) => FromColor(hslaColor);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="YCbCrColor"/> to a 
        /// <see cref="System.Drawing.Color"/>.
        /// </summary>
        /// <param name="ycbcrColor">
        /// The instance of <see cref="YCbCrColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="System.Drawing.Color"/>.
        /// </returns>
        public static implicit operator Color(YCbCrColor ycbcrColor)
        {
            float y = ycbcrColor.Y;
            float cb = ycbcrColor.Cb - 128;
            float cr = ycbcrColor.Cr - 128;

            byte r = Convert.ToByte(ImageMaths.Clamp(y + (1.402 * cr), 0, 255));
            byte g = Convert.ToByte(ImageMaths.Clamp(y - (0.34414 * cb) - (0.71414 * cr), 0, 255));
            byte b = Convert.ToByte(ImageMaths.Clamp(y + (1.772 * cb), 0, 255));

            return Color.FromArgb(255, r, g, b);
        }

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
                return "YCbCrColor [ Empty ]";
            }

            return string.Format("YCbCrColor [ Y={0:#0.##}, Cb={1:#0.##}, Cr={2:#0.##}]", this.Y, this.Cb, this.Cr);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is YCbCrColor yCbCrColor && this.Equals(yCbCrColor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(YCbCrColor other) =>
            this.Y == other.Y
            && this.Cb == other.Cb
            && this.Cr == other.Cr;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.Y, this.Cb, this.Cr).GetHashCode();

        /// <summary>
        /// Returns a value indicating whether the current instance is empty.
        /// </summary>
        /// <returns>
        /// The true if this instance is empty; otherwise, false.
        /// </returns>
        private bool IsEmpty()
        {
            const float Epsilon = .0001f;
            return Math.Abs(this.Y - 0) <= Epsilon && Math.Abs(this.Cb - 0) <= Epsilon
                   && Math.Abs(this.Cr - 0) <= Epsilon;
        }
    }
}
