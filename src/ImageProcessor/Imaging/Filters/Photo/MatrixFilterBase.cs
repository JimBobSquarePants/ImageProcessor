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
    using System.Drawing;
    using System.Drawing.Imaging;

    /// <summary>
    /// The matrix filter base contains equality methods.
    /// </summary>
    public abstract class MatrixFilterBase : IMatrixFilter
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
        /// The processed <see cref="System.Drawing.Bitmap"/>.
        /// </returns>
        public abstract Bitmap TransformImage(Image source, Image destination);

        /// <summary>
        /// Determines whether the specified <see cref="IMatrixFilter" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="IMatrixFilter" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="IMatrixFilter" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is IMatrixFilter filter))
            {
                return false;
            }

            return this.GetType().Name == filter.GetType().Name
                   && this.Matrix.Equals(filter.Matrix);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.GetType().Name.GetHashCode();
                return (hashCode * 397) ^ this.Matrix.GetHashCode();
            }
        }
    }
}