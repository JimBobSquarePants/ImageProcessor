// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CropLayer.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates the properties required to crop an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    using System;

    /// <summary>
    /// Encapsulates the properties required to crop an image.
    /// </summary>
    public class CropLayer : IEquatable<CropLayer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CropLayer"/> class.
        /// </summary>
        /// <param name="left">The left coordinate of the crop layer.</param>
        /// <param name="top">The top coordinate of the crop layer.</param>
        /// <param name="right">The right coordinate of the crop layer.</param>
        /// <param name="bottom">The bottom coordinate of the crop layer.</param>
        /// <param name="cropMode">The <see cref="CropMode"/>.</param>
        /// <remarks>
        /// If the <see cref="CropMode"/> is set to <value>CropMode.Percentage</value> then the four coordinates
        /// become percentages to reduce from each edge.
        /// </remarks>
        public CropLayer(float left, float top, float right, float bottom, CropMode cropMode = CropMode.Percentage)
        {
            if (left < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(left));
            }

            if (top < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(top));
            }

            if (right < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(right));
            }

            if (bottom < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bottom));
            }

            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.CropMode = cropMode;
        }

        /// <summary>
        /// Gets or sets the left coordinate of the crop layer.
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// Gets or sets the top coordinate of the crop layer.
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        /// Gets or sets the right coordinate of the crop layer.
        /// </summary>
        public float Right { get; set; }

        /// <summary>
        /// Gets or sets the bottom coordinate of the crop layer.
        /// </summary>
        public float Bottom { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CropMode"/>.
        /// </summary>
        public CropMode CropMode { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is CropLayer cropLayer && this.Equals(cropLayer);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(CropLayer other) => other != null
            && this.Left == other.Left
            && this.Top == other.Top
            && this.Right == other.Right
            && this.Bottom == other.Bottom
            && this.CropMode == other.CropMode;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.Left, this.Top, this.Right, this.Bottom, this.CropMode).GetHashCode();
    }
}
