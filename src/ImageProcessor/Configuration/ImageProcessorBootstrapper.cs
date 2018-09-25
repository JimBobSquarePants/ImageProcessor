// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageProcessorBootstrapper.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The ImageProcessor bootstrapper containing initialization code for extending ImageProcessor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Common.Extensions;
    using ImageProcessor.Common.Helpers;
    using ImageProcessor.Imaging.Formats;

    /// <summary>
    /// The ImageProcessor bootstrapper containing initialization code for extending ImageProcessor.
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
        private ImageProcessorBootstrapper()
        {
            this.NativeBinaryFactory = new NativeBinaryFactory();
            this.LoadSupportedImageFormats();
            this.LoadLogger();
        }

        /// <summary>
        /// Gets the current instance of the <see cref="ImageProcessorBootstrapper"/> class.
        /// </summary>
        public static ImageProcessorBootstrapper Instance => Lazy.Value;

        /// <summary>
        /// Gets the supported image formats.
        /// </summary>
        public IEnumerable<ISupportedImageFormat> SupportedImageFormats { get; private set; }

        /// <summary>
        /// Gets the currently installed logger.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the native binary factory for registering embedded (unmanaged) binaries.
        /// </summary>
        public NativeBinaryFactory NativeBinaryFactory { get; }

        /// <summary>
        /// Adds the given image formats to the supported format list. Useful for when 
        /// the type finder fails to dynamically add the supported formats.
        /// </summary>
        /// <param name="format">
        /// The <see cref="ISupportedImageFormat"/> instance to add.
        /// </param>
        public void AddImageFormats(params ISupportedImageFormat[] format) => ((List<ISupportedImageFormat>)this.SupportedImageFormats).AddRange(format);

        /// <summary>
        /// Allows the setting of the default logger. Useful for when 
        /// the type finder fails to dynamically add the custom logger implementation.
        /// </summary>
        /// <param name="logger"></param>
        public void SetLogger(ILogger logger) => this.Logger = logger;

        /// <summary>
        /// Creates a list, using reflection, of supported image formats that ImageProcessor can run.
        /// </summary>
        private void LoadSupportedImageFormats()
        {
            var formats = new List<ISupportedImageFormat>
            {
                new BitmapFormat(),
                new GifFormat(),
                new JpegFormat(),
                new PngFormat(),
                new TiffFormat()
            };

            Type type = typeof(ISupportedImageFormat);
            if (this.SupportedImageFormats == null)
            {
                var availableTypes =
                    TypeFinder.GetAssembliesWithKnownExclusions()
                        .SelectMany(a => a.GetLoadableTypes())
                        .Where(t => type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                        .ToList();

                formats.AddRange(availableTypes.Select(f => Activator.CreateInstance(f) as ISupportedImageFormat).ToList());

                this.SupportedImageFormats = formats;
            }
        }

        /// <summary>
        /// Loads the logger.
        /// </summary>
        private void LoadLogger()
        {
            Type type = typeof(ILogger);
            if (this.Logger == null)
            {
                var availableTypes =
                    TypeFinder.GetAssembliesWithKnownExclusions()
                        .SelectMany(a => a.GetLoadableTypes())
                        .Where(t => type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                        .ToList();

                // There's more than one so load the first that is not our default.
                if (availableTypes.Count > 1)
                {
                    this.Logger = availableTypes.Where(l => l != typeof(DefaultLogger))
                                                .Select(f => (Activator.CreateInstance(f) as ILogger))
                                                .First();
                }
                else
                {
                    this.Logger = new DefaultLogger();
                }
            }
        }
    }
}
