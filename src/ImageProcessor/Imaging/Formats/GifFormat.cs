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
        /// <summary>
        /// Gets or sets the process mode for frames in animated images.
        /// </summary>
        public AnimationProcessMode AnimationProcessMode { get; set; }

        /// <summary>
        /// Gets the file headers.
        /// </summary>
        public override byte[][] FileHeaders => new[] { Encoding.ASCII.GetBytes("GIF") };

        /// <summary>
        /// Gets the list of file extensions.
        /// </summary>
        public override string[] FileExtensions => new[] { "gif" };

        /// <summary>
        /// Gets the standard identifier used on the Internet to indicate the type of data that a file contains. 
        /// </summary>
        public override string MimeType => "image/gif";

        /// <summary>
        /// Gets the <see cref="ImageFormat" />.
        /// </summary>
        public override ImageFormat ImageFormat => ImageFormat.Gif;

        /// <summary>
        /// Gets or sets the quantizer for reducing the image palette.
        /// </summary>
        public IQuantizer Quantizer { get; set; } = new OctreeQuantizer(255, 8);

        /// <summary>
        /// Applies the given processor the current image.
        /// </summary>
        /// <param name="processor">The processor delegate.</param>
        /// <param name="factory">The <see cref="ImageFactory" />.</param>
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
        public override Image Save(Stream stream, Image image, long bitDepth)
        {
            // Never use default save for gifs. It's terrible.
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

        /// <inheritdoc />
        public override Image Save(string path, Image image, long bitDepth)
        {
            // Never use default save for gifs. It's terrible.
            using (FileStream fs = File.OpenWrite(path))
            {
                var decoder = new GifDecoder(image, AnimationProcessMode.All);
                var encoder = new GifEncoder(null, null, decoder.LoopCount);

                for (int i = 0; i < decoder.FrameCount; i++)
                {
                    GifFrame frame = decoder.GetFrame(image, i);
                    frame.Image = this.Quantizer.Quantize(frame.Image);
                    encoder.AddFrame(frame);
                }

                encoder.Save(fs);
                return encoder.Save();
            }
        }
    }
}