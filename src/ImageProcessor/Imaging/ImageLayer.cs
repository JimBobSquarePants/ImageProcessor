// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageLayer.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates the properties required to add an image layer to an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ImageProcessor.Imaging
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Encapsulates the properties required to add an image layer to an image.
    /// </summary>
    public class ImageLayer : IDisposable
    {
        /// <summary>
        /// A value indicating whether this instance of the given entity has been disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// If the entity is disposed, it must not be disposed a second
        /// time. The isDisposed field is set the first time the entity
        /// is disposed. If the isDisposed field is true, then the Dispose()
        /// method will not dispose again. This help not to prolong the entity's
        /// life in the Garbage Collector.
        /// </remarks>
        private bool isDisposed = false;

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        public Image Image { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the Opacity of the text layer.
        /// </summary>
        public int Opacity { get; set; } = 100;

        /// <summary>
        /// Gets or sets the Position of the text layer.
        /// </summary>
        public Point? Position { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ImageLayer imageLayer))
            {
                return false;
            }

            return this.Image == imageLayer.Image
                && this.Size == imageLayer.Size
                && this.Opacity == imageLayer.Opacity
                && this.Position == imageLayer.Position;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => (this.Image, this.Size, this.Opacity, this.Position).GetHashCode();

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        public void Dispose() => this.Dispose(true);

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">If true, the object gets disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.Image?.Dispose();
                }

                this.isDisposed = true;
            }
        }
    }
}