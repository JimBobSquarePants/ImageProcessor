// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageProcessor.Processing;
using ImageProcessor.Quantizers;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Defines the contract for a supported image format.
    /// </summary>
    public interface IImageFormat : IEquatable<IImageFormat>
    {
        /// <summary>
        /// Gets the file headers.
        /// </summary>
        byte[][] FileHeaders { get; }

        /// <summary>
        /// Gets the list of file extensions.
        /// </summary>
        string[] FileExtensions { get; }

        /// <summary>
        /// Gets the standard identifier used on the Internet to indicate the type of data that a file contains.
        /// </summary>
        string MimeType { get; }

        /// <summary>
        /// Gets the default file extension.
        /// </summary>
        string DefaultExtension { get; }

        /// <summary>
        /// Gets the file format of the image.
        /// </summary>
        ImageFormat ImageFormat { get; }

        /// <summary>
        /// Gets the quantizer for reducing the image palette.
        /// </summary>
        IQuantizer Quantizer { get; }

        /// <summary>
        /// Loads the image to process.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the image information.</param>
        /// <returns>
        /// The <see cref="Image"/>.
        /// </returns>
        Image Load(Stream stream);

        /// <summary>
        /// Applies the given processor the current image.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IGraphicsProcessor"/>.</typeparam>
        /// <param name="processor">The processor.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>The <see cref="Image"/>.</returns>
        /// <exception cref="ImageProcessingException">Thrown if an error occurs during processing.</exception>
        Image ApplyProcessor<T>(T processor, ImageFactory factory)
            where T : IGraphicsProcessor;

        /// <summary>
        /// Creates a deep copy of the source image.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="targetFormat">The target pixel format.</param>
        /// <param name="frameProcessingMode">The frame processing mode.</param>
        /// <param name="preserveMetaData">Whether to preserve metadata.</param>
        /// <returns>The <see cref="Bitmap"/>.</returns>
        Bitmap DeepClone(Image source, PixelFormat targetFormat, FrameProcessingMode frameProcessingMode, bool preserveMetaData);

        /// <summary>
        /// Saves the current image to the specified output stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the image information to.</param>
        /// <param name="image">The <see cref="Image"/> to save.</param>
        /// <param name="bitDepth">The color depth in number of bits per pixel to save the image with.</param>
        /// <param name="quality">The quality, if applicable, to save the image at.</param>
        void Save(Stream stream, Image image, BitDepth bitDepth, long quality);
    }
}
