// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundedCornerLayer.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates the properties required to add rounded corners to an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    using System;

    /// <summary>
    /// Encapsulates the properties required to add rounded corners to an image.
    /// </summary>
    public class RoundedCornerLayer : IEquatable<RoundedCornerLayer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoundedCornerLayer"/> class.
        /// </summary>
        /// <param name="radius">
        /// The radius at which the corner will be rounded.
        /// </param>
        /// <param name="topLeft">
        /// Set if top left is rounded
        /// </param>
        /// <param name="topRight">
        /// Set if top right is rounded
        /// </param>
        /// <param name="bottomLeft">
        /// Set if bottom left is rounded
        /// </param>
        /// <param name="bottomRight">
        /// Set if bottom right is rounded
        /// </param>
        public RoundedCornerLayer(int radius, bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
        {
            this.Radius = radius;
            this.TopLeft = topLeft;
            this.TopRight = topRight;
            this.BottomLeft = bottomLeft;
            this.BottomRight = bottomRight;
        }

        /// <summary>
        /// Gets or sets the radius of the corners.
        /// </summary>
        public int Radius { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether top left corners are to be added.
        /// </summary>
        public bool TopLeft { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether top right corners are to be added.
        /// </summary>
        public bool TopRight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether bottom left corners are to be added.
        /// </summary>
        public bool BottomLeft { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether bottom right corners are to be added.
        /// </summary>
        public bool BottomRight { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is RoundedCornerLayer roundedCornerLayer && this.Equals(roundedCornerLayer);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(RoundedCornerLayer other) => other != null
            && this.Radius == other.Radius
            && this.TopLeft == other.TopLeft
            && this.TopRight == other.TopRight
            && this.BottomLeft == other.BottomLeft
            && this.BottomRight == other.BottomRight;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => (this.Radius, this.TopLeft, this.TopRight, this.BottomLeft, this.BottomRight).GetHashCode();
    }
}
