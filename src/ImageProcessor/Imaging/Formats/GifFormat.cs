// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GifFormat.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides the necessary information to support gif images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Formats
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;

    using ImageProcessor.Imaging.Quantizers;

    /// <summary>
    /// Provides the necessary information to support gif images.
    /// </summary>
    public class GifFormat : FormatBase, IQuantizableImageFormat, IAnimatedImageFormat
    {
        /// <inheritdoc/>
        public AnimationProcessMode AnimationProcessMode { get; set; }

        /// <inheritdoc/>
        public override byte[][] FileHeaders => new[] { Encoding.ASCII.GetBytes("GIF") };

        /// <inheritdoc/>
        public override string[] FileExtensions => new[] { "gif" };

        /// <inheritdoc/>
        public override string MimeType => "image/gif";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat => ImageFormat.Gif;

        /// <inheritdoc/>
        public IQuantizer Quantizer { get; set; } = new OctreeQuantizer();

        /// <inheritdoc/>
        public override void ApplyProcessor(Func<ImageFactory, Image> processor, ImageFactory factory)
        {
            var decoder = new GifDecoder(factory.Image, factory.AnimationProcessMode);
            Image factoryImage = factory.Image;
            var encoder = new GifEncoder(null, null, decoder.LoopCount);

            for (int i = 0; i < decoder.FrameCount; i++)
            {
                GifFrame frame = decoder.GetFrame(factoryImage, i);
                factory.Image = frame.Image;
                frame.Image = this.Quantizer.Quantize(processor.Invoke(factory));
                encoder.AddFrame(frame);
            }

            factoryImage.Dispose();
            factory.Image = encoder.Save();
        }

        /// <inheritdoc />
        public override Image Save(Stream stream, Image image, BitDepth bitDepth)
        {
            // Never use default save for gifs. It's terrible.
            // BitDepth is ignored here since we always produce 8 bit images.
            var decoder = new GifDecoder(image, AnimationProcessMode.All);
            var encoder = new GifEncoder(null, null, decoder.LoopCount);

            for (int i = 0; i < decoder.FrameCount; i++)
            {
                GifFrame frame = decoder.GetFrame(image, i);
                frame.Image = this.Quantizer.Quantize(frame.Image);
                encoder.AddFrame(frame);
            }

            encoder.Save(stream);
            return encoder.Save();
        }
    }
}