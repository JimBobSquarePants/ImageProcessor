// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageProcessor.Metadata;
using ImageProcessor.Processing;
using ImageProcessor.Quantizers;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// The supported format base. Implement this class when building a supported format.
    /// </summary>
    public abstract class FormatBase : IImageFormat
    {
        /// <inheritdoc/>
        public abstract byte[][] FileHeaders { get; }

        /// <inheritdoc/>
        public abstract string[] FileExtensions { get; }

        /// <inheritdoc/>
        public abstract string MimeType { get; }

        /// <inheritdoc/>
        public string DefaultExtension => this.MimeType.Replace("image/", string.Empty);

        /// <inheritdoc/>
        public abstract ImageFormat ImageFormat { get; }

        /// <inheritdoc/>
        public virtual IQuantizer Quantizer => new OctreeQuantizer();

        /// <inheritdoc/>
        public virtual Image Load(Stream stream)
        {
            // Use color profile but don't validate the data. The windows decoders are very robust.
            return Image.FromStream(stream, true, false);
        }

        /// <inheritdoc/>
        public virtual Image ApplyProcessor<T>(T processor, ImageFactory factory)
            where T : IGraphicsProcessor
        {
            Image copy = null;
            try
            {
                // Always use premultiplied 32 bit to avoid oddities during operations.
                copy = this.DeepClone(factory.Image, PixelFormat.Format32bppArgb, factory.FrameProcessingMode, false);

                bool rotate = ShouldRotate(factory, out int orientation);
                if (rotate)
                {
                    ForwardRotateFlip(orientation, copy);
                }

                Image result = processor.ProcessImageFrame(factory, copy);

                if (rotate)
                {
                    ReverseRotateFlip(orientation, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                copy?.Dispose();
                throw new ImageProcessingException("Error processing image with " + typeof(T).Name, ex);
            }
        }

        /// <inheritdoc />
        public virtual Bitmap DeepClone(Image source, PixelFormat targetFormat, FrameProcessingMode frameProcessingMode, bool preserveMetaData)
        {
            Bitmap copy = FormatUtilities.DeepCloneImageFrame(source, targetFormat);
            if (FormatUtilities.IsIndexed(targetFormat))
            {
                Bitmap quantized = this.Quantizer.Quantize(copy);
                copy.Dispose();
                copy = quantized;
            }

            if (preserveMetaData)
            {
                CopyMetadata(source, copy);
            }

            return copy;
        }

        /// <inheritdoc/>
        public virtual void Save(Stream stream, Image image, BitDepth bitDepth, long quality)
            => image.Save(stream, this.ImageFormat);

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is IImageFormat format && this.Equals(format);

        /// <inheritdoc/>
        public bool Equals(IImageFormat other) =>
            other != null
            && EqualityComparer<byte[][]>.Default.Equals(this.FileHeaders, other.FileHeaders)
            && EqualityComparer<string[]>.Default.Equals(this.FileExtensions, other.FileExtensions)
            && this.MimeType == other.MimeType && this.DefaultExtension == other.DefaultExtension
            && EqualityComparer<ImageFormat>.Default.Equals(this.ImageFormat, other.ImageFormat);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -116763541;
            hashCode = (hashCode * -1521134295) + EqualityComparer<byte[][]>.Default.GetHashCode(this.FileHeaders);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string[]>.Default.GetHashCode(this.FileExtensions);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.MimeType);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.DefaultExtension);
            hashCode = (hashCode * -1521134295) + EqualityComparer<ImageFormat>.Default.GetHashCode(this.ImageFormat);
            return hashCode;
        }

        /// <summary>
        /// Copies the metadata from the source image to the target.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="target">The target image.</param>
        protected static void CopyMetadata(Image source, Image target)
        {
            foreach (PropertyItem item in source.PropertyItems)
            {
                try
                {
                    target.SetPropertyItem(item);
                }
                catch
                {
                    // Handle issue https://github.com/JimBobSquarePants/ImageProcessor/issues/571
                    // SetPropertyItem throws a native error if the property item is invalid for that format
                    // but there's no way to handle individual formats so we do a dumb try...catch...
                }
            }
        }

        /// <summary>
        /// System.Drawing does not respect the EXIF orientation when using graphics so we have to
        /// rotate the image before and after applying processing operations.
        /// See <see href="https://github.com/JimBobSquarePants/ImageProcessor/issues/559"/>.
        /// </summary>
        /// <param name="factory">The image factory.</param>
        /// <param name="orientation">The EXIF orientation value.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected static bool ShouldRotate(ImageFactory factory, out int orientation)
        {
            orientation = 0;
            const int Orientation = (int)ExifPropertyTag.Orientation;
            bool rotate = factory.MetadataMode != MetadataMode.None && factory.PropertyItems.ContainsKey(Orientation);

            if (rotate)
            {
                orientation = factory.PropertyItems[Orientation].Value[0];
            }

            return rotate;
        }

        /// <summary>
        /// Performs a forward rotation of an image.
        /// </summary>
        /// <param name="orientation">The EXIF orientation value.</param>
        /// <param name="image">The image to rotate.</param>
        protected static void ForwardRotateFlip(int orientation, Image image)
        {
            switch (orientation)
            {
                case 8:
                    // Rotated 90 right
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;

                case 7: // Rotated 90 right, flip horizontally
                    image.RotateFlip(RotateFlipType.Rotate270FlipX);
                    break;

                case 6: // Rotated 90 left
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;

                case 5: // Rotated 90 left, flip horizontally
                    image.RotateFlip(RotateFlipType.Rotate90FlipX);
                    break;

                case 3: // Rotate 180 left
                    image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;

                case 2: // Flip horizontally
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
            }
        }

        /// <summary>
        /// Performs an inverse rotation of an image.
        /// </summary>
        /// <param name="orientation">The EXIF orientation value.</param>
        /// <param name="image">The image to rotate.</param>
        protected static void ReverseRotateFlip(int orientation, Image image)
        {
            switch (orientation)
            {
                case 8:
                    // Rotated 90 right
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;

                case 7: // Rotated 90 right, flip horizontally
                    image.RotateFlip(RotateFlipType.Rotate90FlipX);
                    break;

                case 6: // Rotated 90 left
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;

                case 5: // Rotated 90 left, flip horizontally
                    image.RotateFlip(RotateFlipType.Rotate270FlipX);
                    break;

                case 3: // Rotate 180 left
                    image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;

                case 2: // Flip horizontally
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
            }
        }
    }
}
