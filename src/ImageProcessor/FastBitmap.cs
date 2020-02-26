// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessor
{
    /// <summary>
    /// Allows fast access to <see cref="Bitmap"/>'s pixel data.
    /// </summary>
    public unsafe class FastBitmap : IDisposable
    {
        /// <summary>
        /// The bitmap.
        /// </summary>
        private readonly Bitmap bitmap;

        /// <summary>
        /// The number of bytes in a row.
        /// </summary>
        private int bytesPerRow;

        /// <summary>
        /// The bitmap data.
        /// </summary>
        private BitmapData bitmapData;

        /// <summary>
        /// The position of the first pixel in the bitmap.
        /// </summary>
        private byte* pixelBase;

        /// <summary>
        /// A value indicating whether this instance of the given entity has been disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastBitmap"/> class.
        /// </summary>
        /// <param name="bitmap">The input bitmap.</param>
        public FastBitmap(Image bitmap)
        {
            if (FormatUtilities.IsIndexed(bitmap.PixelFormat))
            {
                throw new ArgumentException("Cannot use FastBitmap on indexed images.", nameof(bitmap));
            }

            this.bitmap = (Bitmap)bitmap;
            this.Width = this.bitmap.Width;
            this.Height = this.bitmap.Height;
            this.LockBitmap();
        }

        /// <summary>
        /// Gets the width, in pixels of the <see cref="Bitmap"/>.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height, in pixels of the <see cref="Bitmap"/>.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the pixel data for the given position.
        /// </summary>
        /// <param name="x">The x position of the pixel.</param>
        /// <param name="y">The y position of the pixel.</param>
        /// <returns>
        /// The <see cref="Bgra32"/>.
        /// </returns>
        private Bgra32* this[int x, int y] => (Bgra32*)(this.pixelBase + (y * this.bytesPerRow) + (x * 4));

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="FastBitmap"/> to a <see cref="Image"/>.
        /// </summary>
        /// <param name="fastBitmap">The instance of <see cref="FastBitmap"/> to convert.</param>
        /// <returns>
        /// An instance of <see cref="Image"/>.
        /// </returns>
        public static implicit operator Image(FastBitmap fastBitmap) => fastBitmap.bitmap;

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="FastBitmap"/> to a <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="fastBitmap">The instance of <see cref="FastBitmap"/> to convert.</param>
        /// <returns>
        /// An instance of <see cref="Bitmap"/>.
        /// </returns>
        public static implicit operator Bitmap(FastBitmap fastBitmap) => fastBitmap.bitmap;

        /// <summary>
        /// Gets the color at the specified pixel of the <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>The <see cref="Color"/> at the given pixel.</returns>
        public Color GetPixel(int x, int y)
        {
#if DEBUG
            if ((x < 0) || (x >= this.Width))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Value cannot be less than zero or greater than the bitmap width.");
            }

            if ((y < 0) || (y >= this.Height))
            {
                throw new ArgumentOutOfRangeException(nameof(y), "Value cannot be less than zero or greater than the bitmap height.");
            }
#endif
            Bgra32* data = this[x, y];
            return Color.FromArgb(data->A, data->R, data->G, data->B);
        }

        /// <summary>
        /// Sets the color of the specified pixel of the <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">
        /// A <see cref="Color"/> color structure that represents the
        /// color to set the specified pixel.
        /// </param>
        public void SetPixel(int x, int y, Color color)
        {
#if DEBUG
            if ((x < 0) || (x >= this.Width))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Value cannot be less than zero or greater than the bitmap width.");
            }

            if ((y < 0) || (y >= this.Height))
            {
                throw new ArgumentOutOfRangeException(nameof(y), "Value cannot be less than zero or greater than the bitmap height.");
            }
#endif
            Bgra32* data = this[x, y];
            data->Argb = color.ToArgb();
        }

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is FastBitmap fastBitmap && this.bitmap.Equals(fastBitmap.bitmap);

        /// <inheritdoc/>
        public override int GetHashCode() => this.bitmap.GetHashCode();

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">If true, the object gets disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose of any managed resources here.
                this.UnlockBitmap();
            }

            // Note disposing is done.
            this.isDisposed = true;
        }

        /// <summary>
        /// Locks the bitmap into system memory.
        /// </summary>
        private void LockBitmap()
        {
            var bounds = new Rectangle(Point.Empty, this.bitmap.Size);

            // Figure out the number of bytes in a row. This is rounded up to be a multiple
            // of 4 bytes, since a scan line in an image must always be a multiple of 4 bytes
            // in length.
            int pixelSize = Image.GetPixelFormatSize(this.bitmap.PixelFormat) / 8;
            this.bytesPerRow = bounds.Width * pixelSize;
            if (this.bytesPerRow % 4 != 0)
            {
                this.bytesPerRow = 4 * ((this.bytesPerRow / 4) + 1);
            }

            // Lock the bitmap
            this.bitmapData = this.bitmap.LockBits(bounds, ImageLockMode.ReadWrite, this.bitmap.PixelFormat);

            // Set the value to the first scan line
            this.pixelBase = (byte*)this.bitmapData.Scan0.ToPointer();
        }

        /// <summary>
        /// Unlocks the bitmap from system memory.
        /// </summary>
        private void UnlockBitmap()
        {
            // Copy the RGB values back to the bitmap and unlock the bitmap.
            this.bitmap.UnlockBits(this.bitmapData);
            this.bitmapData = null;
            this.pixelBase = null;
        }
    }
}
