// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RgbaColor.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Colors
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Represents an RGBA (red, green, blue, alpha) color.
    /// </summary>
    public readonly struct RgbaColor : IEquatable<RgbaColor>
    {
        /// <summary>
        /// Represents a <see cref="RgbaColor"/> that is null.
        /// </summary>
        public static readonly RgbaColor Empty = new RgbaColor();

        /// <summary>
        /// Initializes a new instance of the <see cref="RgbaColor"/> struct. 
        /// </summary>
        /// <param name="red">
        /// The red component.
        /// </param>
        /// <param name="green">
        /// The green component.
        /// </param>
        /// <param name="blue">
        /// The blue component.
        /// </param>
        /// <param name="alpha">
        /// The alpha component.
        /// </param>
        private RgbaColor(byte red, byte green, byte blue, byte alpha)
        {
            this.R = red;
            this.G = green;
            this.B = blue;
            this.A = alpha;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RgbaColor"/> struct. 
        /// </summary>
        /// <param name="color">
        /// The <see cref="System.Drawing.Color">color.</see>
        /// </param>
        private RgbaColor(Color color)
        {
            this.R = color.R;
            this.G = color.G;
            this.B = color.B;
            this.A = color.A;
        }

        /// <summary>
        /// Gets the red component.
        /// </summary>
        public byte R { get; }

        /// <summary>
        /// Gets the green component.
        /// </summary>
        public byte G { get; }

        /// <summary>
        /// Gets the blue component.
        /// </summary>
        public byte B { get; }

        /// <summary>
        /// Gets the alpha component.
        /// </summary>
        public byte A { get; }

        /// <summary>
        /// Creates a <see cref="RgbaColor"/> structure from the three 8-bit RGBA 
        /// components (red, green, and blue) values.
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        /// <returns>
        /// The <see cref="RgbaColor"/>.
        /// </returns>
        public static RgbaColor FromRgba(byte red, byte green, byte blue) => new RgbaColor(red, green, blue, 255);

        /// <summary>
        /// Creates a <see cref="RgbaColor"/> structure from the four 8-bit RGBA 
        /// components (red, green, blue, and alpha) values.
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        /// <param name="alpha">The alpha component.</param>
        /// <returns>
        /// The <see cref="RgbaColor"/>.
        /// </returns>
        public static RgbaColor FromRgba(byte red, byte green, byte blue, byte alpha) => new RgbaColor(red, green, blue, alpha);

        /// <summary>
        /// Creates a <see cref="RgbaColor"/> structure from the specified <see cref="System.Drawing.Color"/> structure
        /// </summary>
        /// <param name="color">
        /// The <see cref="System.Drawing.Color"/> from which to create the new <see cref="RgbaColor"/>.
        /// </param>
        /// <returns>
        /// The <see cref="RgbaColor"/>.
        /// </returns>
        public static RgbaColor FromColor(Color color) => new RgbaColor(color);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="System.Drawing.Color"/> to a 
        /// <see cref="RgbaColor"/>.
        /// </summary>
        /// <param name="color">
        /// The instance of <see cref="System.Drawing.Color"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="RgbaColor"/>.
        /// </returns>
        public static implicit operator RgbaColor(Color color) => FromColor(color);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="HslaColor"/> to a 
        /// <see cref="RgbaColor"/>.
        /// </summary>
        /// <param name="hslaColor">
        /// The instance of <see cref="HslaColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="RgbaColor"/>.
        /// </returns>
        public static implicit operator RgbaColor(HslaColor hslaColor) => FromColor(hslaColor);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="YCbCrColor"/> to a 
        /// <see cref="RgbaColor"/>.
        /// </summary>
        /// <param name="ycbcrColor">
        /// The instance of <see cref="YCbCrColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="RgbaColor"/>.
        /// </returns>
        public static implicit operator RgbaColor(YCbCrColor ycbcrColor) => FromColor(ycbcrColor);

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="RgbaColor"/> to a 
        /// <see cref="System.Drawing.Color"/>.
        /// </summary>
        /// <param name="rgbaColor">
        /// The instance of <see cref="RgbaColor"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="System.Drawing.Color"/>.
        /// </returns>
        public static implicit operator Color(RgbaColor rgbaColor) => Color.FromArgb(rgbaColor.A, rgbaColor.R, rgbaColor.G, rgbaColor.B);

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
        public static implicit operator YCbCrColor(RgbaColor rgbaColor) => YCbCrColor.FromColor(rgbaColor);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.R == 0 && this.G == 0 && this.B == 0 && this.A == 0)
            {
                return "RGBA [ Empty ]";
            }

            return string.Format("RGBA [R={0}, G={1}, B={2}, A={3}]", this.R, this.G, this.B, this.A);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is RgbaColor rgbaColor && this.Equals(rgbaColor);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(RgbaColor other) =>
            this.R == other.R
            && this.G == other.G
            && this.B == other.B
            && this.A == other.A;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.R, this.G, this.B, this.A).GetHashCode();
    }
}