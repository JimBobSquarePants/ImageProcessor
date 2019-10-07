// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using ImageProcessor.Formats;

namespace ImageProcessor.Configuration
{
    /// <summary>
    /// The bootstrapper containing initialization code for extending ImageProcessor.
    /// </summary>
    public sealed class ImageProcessorBootstrapper
    {
        /// <summary>
        /// A new instance Initializes a new instance of the <see cref="ImageProcessorBootstrapper"/> class.
        /// with lazy initialization.
        /// </summary>
        private static readonly Lazy<ImageProcessorBootstrapper> Lazy =
                        new Lazy<ImageProcessorBootstrapper>(() => new ImageProcessorBootstrapper());

        /// <summary>
        /// Prevents a default instance of the <see cref="ImageProcessorBootstrapper"/> class from being created.
        /// </summary>
        private ImageProcessorBootstrapper() => this.LoadSupportedImageFormats();

        /// <summary>
        /// Gets the current instance of the <see cref="ImageProcessorBootstrapper"/> class.
        /// </summary>
        public static ImageProcessorBootstrapper Instance => Lazy.Value;

        /// <summary>
        /// Gets the supported image formats.
        /// </summary>
        public IReadOnlyCollection<IImageFormat> ImageFormats { get; private set; }

        /// <summary>
        /// Gets the currently installed logger.
        /// </summary>
        public ILogger Logger { get; private set; } = new DefaultLogger();

        /// <summary>
        /// Gets the native binary factory for registering embedded (unmanaged) binaries.
        /// </summary>
        public NativeBinaryFactory NativeBinaryFactory { get; } = new NativeBinaryFactory();

        /// <summary>
        /// Adds the given image formats to the supported format collection.
        /// </summary>
        /// <param name="formats">The <see cref="IImageFormat"/> instances to add.</param>
        public void AddImageFormats(params IImageFormat[] formats)
        {
            var currentFormats = (List<IImageFormat>)this.ImageFormats;

            foreach (IImageFormat format in formats)
            {
                if (currentFormats.Any(x => x.Equals(format)))
                {
                    continue;
                }

                currentFormats.Add(format);
            }
        }

        /// <summary>
        /// Allows the setting of the default logger. Useful for when
        /// the type finder fails to dynamically add the custom logger implementation.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public void SetLogger(ILogger logger) => this.Logger = logger;

        /// <summary>
        /// Creates a collection of supported image formats that ImageProcessor can run.
        /// </summary>
        private void LoadSupportedImageFormats()
        {
            this.ImageFormats = new List<IImageFormat>
            {
                new BitmapFormat(),
                new GifFormat(),
                new JpegFormat(),
                new PngFormat(),
                new TiffFormat()
            };
        }
    }
}
