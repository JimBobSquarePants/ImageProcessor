// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResizeLayer.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates the properties required to resize an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    /// <summary>
    /// Encapsulates the properties required to resize an image.
    /// </summary>
    /// <seealso cref="T:System.IEquatable{ImageProcessor.Imaging.ResizeLayer}" />
    public class ResizeLayer : IEquatable<ResizeLayer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeLayer"/> class.
        /// </summary>
        /// <param name="size">
        /// The <see cref="T:System.Drawing.Size"/> containing the width and height to set the image to.
        /// </param>
        /// <param name="resizeMode">
        /// The resize mode to apply to resized image. (Default ResizeMode.Pad)
        /// </param>
        /// <param name="anchorPosition">
        /// The <see cref="AnchorPosition"/> to apply to resized image. (Default AnchorPosition.Center)
        /// </param>
        /// <param name="upscale">
        /// Whether to allow up-scaling of images. (Default true)
        /// </param>
        /// <param name="centerCoordinates">
        /// The center coordinates (Default null)
        /// </param>
        /// <param name="maxSize">
        /// The maximum size to resize an image to. 
        /// Used to restrict resizing based on calculated resizing
        /// </param>
        /// <param name="restrictedSizes">
        /// The range of sizes to restrict resizing an image to. 
        /// Used to restrict resizing based on calculated resizing
        /// </param>
        /// <param name="anchorPoint">
        /// The anchor point (Default null)
        /// </param>
        public ResizeLayer(
            Size size,
            ResizeMode resizeMode = ResizeMode.Pad,
            AnchorPosition anchorPosition = AnchorPosition.Center,
            bool upscale = true,
            float[] centerCoordinates = null,
            Size? maxSize = null,
            List<Size> restrictedSizes = null,
            Point? anchorPoint = null)
        {
            this.Size = size;
            this.Upscale = upscale;
            this.ResizeMode = resizeMode;
            this.AnchorPosition = anchorPosition;
            this.CenterCoordinates = centerCoordinates ?? new float[] { };
            this.MaxSize = maxSize;
            this.RestrictedSizes = restrictedSizes ?? new List<Size>();
            this.AnchorPoint = anchorPoint;
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the max size.
        /// </summary>
        public Size? MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the restricted range of sizes. to restrict resizing methods to.
        /// </summary>
        public List<Size> RestrictedSizes { get; set; }

        /// <summary>
        /// Gets or sets the resize mode.
        /// </summary>
        public ResizeMode ResizeMode { get; set; }

        /// <summary>
        /// Gets or sets the anchor position.
        /// </summary>
        public AnchorPosition AnchorPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow up-scaling of images.
        /// For <see cref="T:ResizeMode.BoxPad"/> this is always true.
        /// </summary>
        public bool Upscale { get; set; }

        /// <summary>
        /// Gets or sets the center coordinates.
        /// </summary>
        public float[] CenterCoordinates { get; set; }

        /// <summary>
        /// Gets or sets the anchor point.
        /// </summary>
        public Point? AnchorPoint { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is ResizeLayer resizeLayer && this.Equals(resizeLayer);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ResizeLayer other) => other != null
            && this.Size == other.Size
            && this.MaxSize == other.MaxSize
            && (this.RestrictedSizes == null || other.RestrictedSizes == null ? this.RestrictedSizes == other.RestrictedSizes : this.RestrictedSizes.SequenceEqual(other.RestrictedSizes))
            && this.ResizeMode == other.ResizeMode
            && this.AnchorPosition == other.AnchorPosition
            && this.Upscale == other.Upscale
            && (this.CenterCoordinates == null || other.CenterCoordinates == null ? this.CenterCoordinates == other.CenterCoordinates : this.CenterCoordinates.SequenceEqual(other.CenterCoordinates))
            && this.AnchorPoint == other.AnchorPoint;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.Size, this.MaxSize, this.ResizeMode, this.AnchorPosition, this.Upscale, this.AnchorPoint).GetHashCode();
    }
}