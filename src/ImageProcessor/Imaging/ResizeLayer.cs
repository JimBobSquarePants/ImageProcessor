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
        /// Initializes a new instance of the <see cref="ResizeLayer" /> class.
        /// </summary>
        /// <param name="size">The <see cref="T:System.Drawing.Size" /> containing the width and height to set the image to.</param>
        /// <param name="resizeMode">The resize mode to apply to resized image. (Default ResizeMode.Pad)</param>
        /// <param name="anchorPosition">The <see cref="AnchorPosition" /> to apply to resized image. (Default AnchorPosition.Center)</param>
        /// <param name="upscale">Whether to allow up-scaling of images. (Default true)</param>
        /// <param name="centerCoordinates">The center coordinates (Default null)</param>
        /// <param name="maxSize">The maximum size to resize an image to.
        /// Used to restrict resizing based on calculated resizing</param>
        /// <param name="restrictedSizes">The range of sizes to restrict resizing an image to.
        /// Used to restrict resizing based on calculated resizing</param>
        /// <param name="anchorPoint">The anchor point (Default null)</param>
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
            this.CenterCoordinates = centerCoordinates;
            this.MaxSize = maxSize;
            this.RestrictedSizes = restrictedSizes;
            this.AnchorPoint = anchorPoint;
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the maximum size.
        /// </summary>
        /// <value>
        /// The maximum size.
        /// </value>
        public Size? MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the restricted range of sizes to restrict resizing methods to.
        /// </summary>
        /// <value>
        /// The restricted sizes.
        /// </value>
        public List<Size> RestrictedSizes { get; set; }

        /// <summary>
        /// Gets or sets the resize mode.
        /// </summary>
        /// <value>
        /// The resize mode.
        /// </value>
        public ResizeMode ResizeMode { get; set; }

        /// <summary>
        /// Gets or sets the anchor position.
        /// </summary>
        /// <value>
        /// The anchor position.
        /// </value>
        public AnchorPosition AnchorPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow up-scaling of images.
        /// For <see cref="T:ResizeMode.BoxPad" /> this is always true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if up-scaling is allowed; otherwise, <c>false</c>.
        /// </value>
        public bool Upscale { get; set; }

        /// <summary>
        /// Gets or sets the center coordinates (Y,X).
        /// </summary>
        /// <value>
        /// The center coordinates (Y,X).
        /// </value>
        public float[] CenterCoordinates { get; set; }

        /// <summary>
        /// Gets the center coordinates as <see cref="Nullable{PointF}" />.
        /// </summary>
        /// <value>
        /// The center coordinates as <see cref="Nullable{PointF}" />.
        /// </value>
        /// <remarks>
        /// Use this property instead of <see cref="CenterCoordinates" />, as it's validated and the X/Y positions are unambiguous.
        /// </remarks>
        internal PointF? Center => this.CenterCoordinates is float[] center && center.Length == 2 ? (PointF?)new PointF(center[1], center[0]) : null;

        /// <summary>
        /// Gets or sets the anchor point.
        /// </summary>
        /// <value>
        /// The anchor point.
        /// </value>
        public Point? AnchorPoint { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj)
            => this.Equals(obj as ResizeLayer);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(ResizeLayer other)
            => other != null
            && this.Size == other.Size
            && this.MaxSize == other.MaxSize
            && (this.RestrictedSizes == null || other.RestrictedSizes == null ? this.RestrictedSizes == other.RestrictedSizes : this.RestrictedSizes.SequenceEqual(other.RestrictedSizes))
            && this.ResizeMode == other.ResizeMode
            && this.AnchorPosition == other.AnchorPosition
            && this.Upscale == other.Upscale
            && this.Center == other.Center
            && this.AnchorPoint == other.AnchorPoint;

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = -1040263854;
            hashCode = (hashCode * -1521134295) + this.Size.GetHashCode();
            hashCode = (hashCode * -1521134295) + (this.MaxSize == null ? 0 : this.MaxSize.GetHashCode());
            hashCode = (hashCode * -1521134295) + (this.RestrictedSizes == null ? 0 : this.RestrictedSizes.GetHashCode());
            hashCode = (hashCode * -1521134295) + this.ResizeMode.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.AnchorPosition.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.Upscale.GetHashCode();
            hashCode = (hashCode * -1521134295) + (this.Center == null ? 0 : this.Center.GetHashCode());
            hashCode = (hashCode * -1521134295) + (this.AnchorPoint == null ? 0 : this.AnchorPoint.GetHashCode());
            return hashCode;
        }
    }
}
