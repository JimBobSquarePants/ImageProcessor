// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Color32.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Structure that defines a 32 bit color
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Colors
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Structure that defines a 32 bits per pixel color, used for pixel manipulation not for color conversion.
    /// </summary>
    /// <remarks>
    /// This structure is used to read data from a 32 bits per pixel image
    /// in memory, and is ordered in this manner as this is the way that
    /// the data is laid out in memory
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct Color32 : IEquatable<Color32>
    {
        /// <summary>
        /// Holds the blue component of the colour
        /// </summary>
        [FieldOffset(0)]
        public byte B;

        /// <summary>
        /// Holds the green component of the colour
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// Holds the red component of the colour
        /// </summary>
        [FieldOffset(2)]
        public byte R;

        /// <summary>
        /// Holds the alpha component of the colour
        /// </summary>
        [FieldOffset(3)]
        public byte A;

        /// <summary>
        /// Permits the color32 to be treated as a 32 bit integer.
        /// </summary>
        [FieldOffset(0)]
        public int Argb;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color32"/> struct.
        /// </summary>
        /// <param name="alpha">The alpha component.</param>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        public Color32(byte alpha, byte red, byte green, byte blue)
            : this()
        {
            this.A = alpha;
            this.R = red;
            this.G = green;
            this.B = blue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color32"/> struct.
        /// </summary>
        /// <param name="argb">
        /// The combined color components.
        /// </param>
        public Color32(int argb)
            : this() => this.Argb = argb;

        /// <summary>
        /// Gets the color for this Color32 object
        /// </summary>
        public Color Color => Color.FromArgb(this.A, this.R, this.G, this.B);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is Color32 color32 && this.Equals(color32);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Color32 other) => this.Argb == other.Argb;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => this.Argb.GetHashCode();
    }
}