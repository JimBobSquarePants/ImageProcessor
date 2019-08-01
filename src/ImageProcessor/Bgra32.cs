// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageProcessor
{
    /// <summary>
    /// Structure that defines a 32 bits per pixel Bgra color. Used for pixel manipulation not for color conversion.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Bgra32 : IEquatable<Bgra32>
    {
        /// <summary>
        /// Holds the blue component of the color.
        /// </summary>
        [FieldOffset(0)]
        public byte B;

        /// <summary>
        /// Holds the green component of the color.
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// Holds the red component of the color.
        /// </summary>
        [FieldOffset(2)]
        public byte R;

        /// <summary>
        /// Holds the alpha component of the color.
        /// </summary>
        [FieldOffset(3)]
        public byte A;

        /// <summary>
        /// Permits the color32 to be treated as a 32 bit integer.
        /// </summary>
        [FieldOffset(0)]
        public int Argb;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bgra32"/> struct.
        /// </summary>
        /// <param name="alpha">The alpha component.</param>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        public Bgra32(byte alpha, byte red, byte green, byte blue)
            : this()
        {
            this.A = alpha;
            this.R = red;
            this.G = green;
            this.B = blue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bgra32"/> struct.
        /// </summary>
        /// <param name="argb">The combined color components.</param>
        public Bgra32(int argb)
            : this() => this.Argb = argb;

        /// <summary>
        /// Gets the color for this Color32 object.
        /// </summary>
        public Color Color => Color.FromArgb(this.A, this.R, this.G, this.B);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Bgra32 color && this.Equals(color);

        /// <inheritdoc/>
        public bool Equals(Bgra32 other) => this.Argb.Equals(other.Argb);

        /// <inheritdoc/>
        public override int GetHashCode() => this.GetHashCode(this);

        private int GetHashCode(Bgra32 color)
        {
            unchecked
            {
                int hashCode = color.B.GetHashCode();
                hashCode = (hashCode * 397) ^ color.G.GetHashCode();
                hashCode = (hashCode * 397) ^ color.R.GetHashCode();
                return (hashCode * 397) ^ color.A.GetHashCode();
            }
        }
    }
}
