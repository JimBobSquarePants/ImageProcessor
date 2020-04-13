// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixFilterBase.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The matrix filter base contains equality methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Filters.Photo
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    /// <summary>
    /// The matrix filter base contains equality methods.
    /// </summary>
    public abstract class MatrixFilterBase : IMatrixFilter, IEquatable<IMatrixFilter>
    {
        /// <summary>
        /// Gets the <see cref="T:System.Drawing.Imaging.ColorMatrix" /> for this filter instance.
        /// </summary>
        public abstract ColorMatrix Matrix { get; }

        /// <summary>
        /// Processes the image.
        /// </summary>
        /// <param name="source">The current image to process</param>
        /// <param name="destination">The new image to return</param>
        /// <returns>
        /// The processed <see cref="System.Drawing.Bitmap" />.
        /// </returns>
        public abstract Bitmap TransformImage(Image source, Image destination);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is IMatrixFilter matrixFilter && this.Equals(matrixFilter);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IMatrixFilter other) => other != null
            && this.GetType() == other.GetType()
            && (this.Matrix == null || other.Matrix == null ? this.Matrix == other.Matrix : (
                this.Matrix.Matrix00 == other.Matrix.Matrix00
                && this.Matrix.Matrix01 == other.Matrix.Matrix01
                && this.Matrix.Matrix02 == other.Matrix.Matrix02
                && this.Matrix.Matrix03 == other.Matrix.Matrix03
                && this.Matrix.Matrix04 == other.Matrix.Matrix04
                && this.Matrix.Matrix10 == other.Matrix.Matrix10
                && this.Matrix.Matrix11 == other.Matrix.Matrix11
                && this.Matrix.Matrix12 == other.Matrix.Matrix12
                && this.Matrix.Matrix13 == other.Matrix.Matrix13
                && this.Matrix.Matrix14 == other.Matrix.Matrix14
                && this.Matrix.Matrix20 == other.Matrix.Matrix20
                && this.Matrix.Matrix21 == other.Matrix.Matrix21
                && this.Matrix.Matrix22 == other.Matrix.Matrix22
                && this.Matrix.Matrix23 == other.Matrix.Matrix23
                && this.Matrix.Matrix24 == other.Matrix.Matrix24
                && this.Matrix.Matrix30 == other.Matrix.Matrix30
                && this.Matrix.Matrix31 == other.Matrix.Matrix31
                && this.Matrix.Matrix32 == other.Matrix.Matrix32
                && this.Matrix.Matrix33 == other.Matrix.Matrix33
                && this.Matrix.Matrix34 == other.Matrix.Matrix34
                && this.Matrix.Matrix40 == other.Matrix.Matrix40
                && this.Matrix.Matrix41 == other.Matrix.Matrix41
                && this.Matrix.Matrix42 == other.Matrix.Matrix42
                && this.Matrix.Matrix43 == other.Matrix.Matrix43
                && this.Matrix.Matrix44 == other.Matrix.Matrix44
            ));

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (
            this.GetType(),
            this.Matrix?.Matrix00,
            this.Matrix?.Matrix01,
            this.Matrix?.Matrix02,
            this.Matrix?.Matrix03,
            this.Matrix?.Matrix04,
            this.Matrix?.Matrix10,
            this.Matrix?.Matrix11,
            this.Matrix?.Matrix12,
            this.Matrix?.Matrix13,
            this.Matrix?.Matrix14,
            this.Matrix?.Matrix20,
            this.Matrix?.Matrix21,
            this.Matrix?.Matrix22,
            this.Matrix?.Matrix23,
            this.Matrix?.Matrix24,
            this.Matrix?.Matrix30,
            this.Matrix?.Matrix31,
            this.Matrix?.Matrix32,
            this.Matrix?.Matrix33,
            this.Matrix?.Matrix34,
            this.Matrix?.Matrix40,
            this.Matrix?.Matrix41,
            this.Matrix?.Matrix42,
            this.Matrix?.Matrix43,
            this.Matrix?.Matrix44).GetHashCode();
    }
}