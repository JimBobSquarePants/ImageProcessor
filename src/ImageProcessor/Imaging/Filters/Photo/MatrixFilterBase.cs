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
            && this.Matrix == other.Matrix;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.GetType(), this.Matrix).GetHashCode();
    }
}