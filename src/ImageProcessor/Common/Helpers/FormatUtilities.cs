// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using ImageProcessor.Configuration;
using ImageProcessor.Formats;

namespace ImageProcessor
{
    /// <summary>
    /// Utility methods for working with supported image formats.
    /// </summary>
    public static class FormatUtilities
    {
        private static readonly ConstructorInfo PropertyItemConstructor = typeof(PropertyItem).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);

        /// <summary>
        /// Gets the correct <see cref="IImageFormat"/> from the given stream.
        /// <see href="http://stackoverflow.com/questions/55869/determine-file-type-of-an-image"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <returns>The <see cref="IImageFormat"/>.</returns>
        public static IImageFormat GetFormat(Stream stream)
        {
            // We need a seekable stream to work with when detecting the image formats.
            if (!stream.CanSeek)
            {
                throw new ImageFormatException("Cannot detect format from an unseekable stream.");
            }

            IReadOnlyCollection<IImageFormat> imageFormats = ImageProcessorBootstrapper.Instance.ImageFormats;
            int numberOfBytesToRead = imageFormats.Max(f => f.FileHeaders.Max(h => h.Length));

            // Reset the position of the stream to ensure we're reading the correct part.
            stream.Position = 0;
            Span<byte> buffer = stackalloc byte[numberOfBytesToRead];
            stream.Read(buffer);
            stream.Position = 0;

            foreach (IImageFormat format in imageFormats)
            {
                foreach (byte[] header in format.FileHeaders)
                {
                    if (header.AsSpan().SequenceEqual(buffer.Slice(0, header.Length)))
                    {
                        return format;
                    }
                }
            }

            throw new ImageFormatException("Input stream is not a supported format.");
        }

        /// <summary>
        /// Gets the correct <see cref="IImageFormat"/> from the given file path.
        /// </summary>
        /// <param name="path">The path string from which to get the format.</param>
        /// <returns>The <see cref="IImageFormat"/>.</returns>
        public static IImageFormat GetFormat(string path)
        {
            string ext = Path.GetExtension(path);
            foreach (IImageFormat format in ImageProcessorBootstrapper.Instance.ImageFormats)
            {
                if (format.FileExtensions.Any(x => ("." + x).Equals(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    return format;
                }
            }

            throw new ImageFormatException($"Output path extension {ext} does not match a supported format.");
        }

        /// <summary>
        /// Creates a deep copy of the source image frame allowing you to set the pixel format.
        /// <remarks>
        /// Images with an indexed <see cref="PixelFormat"/> cannot deep copied using a <see cref="Graphics"/>
        /// surface so have to be copied to <see cref="PixelFormat.Format32bppArgb"/> instead.
        /// </remarks>
        /// </summary>
        /// <param name="source">The source image frame.</param>
        /// <param name="targetFormat">The target pixel format.</param>
        /// <returns>The <see cref="Bitmap"/>.</returns>
        public static Bitmap DeepCloneImageFrame(Image source, PixelFormat targetFormat)
        {
            // Create a new image and draw the original pixel data over the top.
            // This will automatically remap the pixel layout.
            PixelFormat pixelFormat = IsIndexed(targetFormat) ? PixelFormat.Format32bppArgb : targetFormat;
            var copy = new Bitmap(source.Width, source.Height, pixelFormat);
            copy.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = Graphics.FromImage(copy))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.PageUnit = GraphicsUnit.Pixel;

                graphics.Clear(Color.Transparent);
                graphics.DrawImageUnscaled(source, 0, 0);
            }

            return copy;
        }

        /// <summary>
        /// Creates an empty image frame with the same pixel format and resolution as the original.
        /// </summary>
        /// <param name="source">The source image frame.</param>
        /// <returns>The <see cref="Bitmap"/>.</returns>
        public static Bitmap CreateEmptyFrameFrom(Image source)
        {
            var copy = new Bitmap(source.Width, source.Height, source.PixelFormat);
            copy.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            return copy;
        }

        /// <summary>
        /// Gets the color depth in our suppported range in bits per pixel for the given pixel format.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>The <see cref="BitDepth"/>.</returns>
        public static BitDepth GetSupportedBitDepth(PixelFormat pixelFormat)
        {
            switch ((long)Image.GetPixelFormatSize(pixelFormat))
            {
                case 1L:
                    return BitDepth.Bit1;
                case 4L:
                    return BitDepth.Bit4;
                case 8L:
                    return BitDepth.Bit8;
                case 16L:
                    return BitDepth.Bit16;
                case 24L:
                    return BitDepth.Bit24;
                default:
                    return BitDepth.Bit32;
            }
        }

        /// <summary>
        /// Gets the default pixel format for the given bit depth.
        /// </summary>
        /// <param name="bitDepth">The color depth in bits per pixel.</param>
        /// <returns>The <see cref="PixelFormat"/>.</returns>
        public static PixelFormat GetPixelFormatForBitDepth(BitDepth bitDepth)
        {
            switch (bitDepth)
            {
                case BitDepth.Bit1:
                    return PixelFormat.Format1bppIndexed;
                case BitDepth.Bit4:
                    return PixelFormat.Format4bppIndexed;
                case BitDepth.Bit8:
                    return PixelFormat.Format8bppIndexed;
                case BitDepth.Bit16:
                    return PixelFormat.Format16bppRgb565;
                case BitDepth.Bit24:
                    return PixelFormat.Format24bppRgb;
                default:
                    return PixelFormat.Format32bppArgb;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the given image has an alpha channel.
        /// </summary>
        /// <param name="image">
        /// The <see cref="Image"/> to test.
        /// </param>
        /// <returns>
        /// The true if the image has an alpha channel; otherwise, false.
        /// </returns>
        public static bool HasAlpha(Image image)
            => ((ImageFlags)image.Flags & ImageFlags.HasAlpha) == ImageFlags.HasAlpha;

        /// <summary>
        /// Returns a value indicating whether the given image is animated.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to test.</param>
        /// <returns>
        /// The true if the image is animated; otherwise, false.
        /// </returns>
        public static bool IsAnimated(Image image) => ImageAnimator.CanAnimate(image);

        /// <summary>
        /// Returns a value indicating whether the given pixel format is indexed.
        /// </summary>
        /// <param name="format">The <see cref="PixelFormat"/> to test.</param>
        /// <returns>
        /// The true if the image is indexed; otherwise, false.
        /// </returns>
        public static bool IsIndexed(PixelFormat format)
        {
            return format == PixelFormat.Indexed
                || format == PixelFormat.Format1bppIndexed
                || format == PixelFormat.Format4bppIndexed
                || format == PixelFormat.Format8bppIndexed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyItem"/> class.
        /// </summary>
        /// <returns>The <see cref="PropertyItem"/>.</returns>
        public static PropertyItem CreatePropertyItem() => (PropertyItem)PropertyItemConstructor.Invoke(null);
    }
}
