// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Provides the necessary information to support gif images.
    /// </summary>
    public sealed class GifFormat : FormatBase
    {
        /// <inheritdoc/>
        public override byte[][] FileHeaders { get; } = new[] { Encoding.ASCII.GetBytes("GIF") };

        /// <inheritdoc/>
        public override string[] FileExtensions { get; } = new[] { "gif" };

        /// <inheritdoc/>
        public override string MimeType { get; } = "image/gif";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat { get; } = ImageFormat.Gif;

        /// <inheritdoc/>
        public override Image ApplyProcessor<T>(T processor, ImageFactory factory)
        {
            var decoder = new GifDecoder(factory.Image, factory.FrameProcessingMode);
            var encoder = new GifEncoder(this.Quantizer, decoder.LoopCount);

            for (int i = 0; i < decoder.FrameCount; i++)
            {
                GifFrame frame = null;
                try
                {
                    // Frame images are already a copy so we don't have to copy again for processing.
                    frame = decoder.GetFrame(i);
                    processor.ProcessImageFrame(factory, frame.Image);
                    encoder.EncodeFrame(frame);
                }
                catch (Exception ex)
                {
                    throw new ImageProcessingException("Error processing image with " + typeof(T).Name, ex);
                }
                finally
                {
                    frame?.Dispose();
                }
            }

            return encoder.Encode();
        }

        /// <inheritdoc />
        public override Bitmap DeepClone(Image source, PixelFormat targetFormat, FrameProcessingMode frameProcessingMode, bool preserveMetaData)
        {
            var decoder = new GifDecoder(source, frameProcessingMode);
            var encoder = new GifEncoder(this.Quantizer, decoder.LoopCount);

            for (int i = 0; i < decoder.FrameCount; i++)
            {
                using (GifFrame frame = decoder.GetFrame(i))
                {
                    encoder.EncodeFrame(frame);
                }
            }

            Image copy = encoder.Encode();

            if (preserveMetaData)
            {
                CopyMetadata(source, copy);
            }

            return (Bitmap)copy;
        }

        /// <inheritdoc />
        public override void Save(Stream stream, Image image, BitDepth bitDepth, long quality)
        {
            // Never use default save for gifs. It's terrible.
            // BitDepth is ignored here since we always produce 8 bit images.
            var decoder = new GifDecoder(image, FrameProcessingMode.All);
            var encoder = new GifEncoder(this.Quantizer, decoder.LoopCount);

            for (int i = 0; i < decoder.FrameCount; i++)
            {
                using (GifFrame frame = decoder.GetFrame(i))
                {
                    encoder.EncodeFrame(frame);
                }
            }

            encoder.EncodeToStream(stream);
        }
    }
}
