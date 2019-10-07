// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageProcessor.Formats;
using ImageProcessor.Metadata;
using ImageProcessor.Processing;

namespace ImageProcessor
{
    /// <summary>
    /// Provides a fluent API that allows the processing of images with common operations.
    /// </summary>
    public sealed partial class ImageFactory : IDisposable
    {
        private bool isDisposed;
        private bool isLoaded;
        private IImageFormat imageFormat;
        private BitDepth bitDepth;
        private MemoryStream memoryStream = new MemoryStream();

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFactory"/> class.
        /// </summary>
        public ImageFactory()
            : this(FrameProcessingMode.All, MetadataMode.All)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFactory"/> class.
        /// </summary>
        /// <param name="frameProcessingMode">The frame processing mode to use.</param>
        /// <param name="metadataMode">The metadata mode to use.</param>
        public ImageFactory(FrameProcessingMode frameProcessingMode, MetadataMode metadataMode)
        {
            this.MetadataMode = metadataMode;
            this.FrameProcessingMode = frameProcessingMode;

            // TODO: This could possible be private, accessable by methods only.
            // TODO: No reason for concurrency.
            this.PropertyItems = new ConcurrentDictionary<int, PropertyItem>();
        }

        /// <summary>
        /// Gets the loaded image.
        /// </summary>
        public Image Image { get; private set; }

        /// <summary>
        /// Gets the frame processing mode.
        /// </summary>
        public FrameProcessingMode FrameProcessingMode { get; }

        /// <summary>
        /// Gets the metadata mode.
        /// </summary>
        public MetadataMode MetadataMode { get; }

        /// <summary>
        /// Gets or sets the quality, if applicable, to save the image at.
        /// Defaults to 90.
        /// </summary>
        public long Quality { get; set; } = 90;

        /// <summary>
        /// Gets the collection of property items containing metadata.
        /// </summary>
        internal ConcurrentDictionary<int, PropertyItem> PropertyItems { get; }

        /// <summary>
        /// Loads the image to process.
        /// </summary>
        /// <param name="bytes">The array of bytes containing the encoded image data.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Load(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return this.Load(stream);
            }
        }

        /// <summary>
        /// Loads the image to process.
        /// </summary>
        /// <param name="path">The path to the image file.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("No image found at the given path", path);
            }

            using (FileStream stream = File.OpenRead(path))
            {
                return this.Load(stream);
            }
        }

        /// <summary>
        /// Loads the image to process.
        /// </summary>
        /// <param name="stream">The stream containing the image information.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Load(Stream stream)
        {
            // By doing this we can reset the factory by simply reloading the image from the stored stream.
            if (!this.isLoaded)
            {
                stream.CopyTo(this.memoryStream);
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
            }

            this.imageFormat = FormatUtilities.GetFormat(this.memoryStream);
            this.Image?.Dispose();
            this.Image = this.imageFormat.Load(this.memoryStream);
            this.bitDepth = FormatUtilities.GetSupportedBitDepth(this.Image.PixelFormat);

            // Always load the metadata.
            // TODO. Some custom data doesn't seem to get copied by default methods.
            this.PropertyItems.Clear();
            foreach (int id in this.Image.PropertyIdList)
            {
                this.PropertyItems[id] = this.Image.GetPropertyItem(id);
            }

            this.isLoaded = true;

            return this;
        }

        /// <summary>
        /// Resets the factory to its initial loaded state.
        /// </summary>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Reset()
        {
            this.CheckLoaded();
            return this.Load(this.memoryStream);
        }

        /// <summary>
        /// Saves the current image to the specified file path.
        /// </summary>
        /// <param name="path">The path to save the image to.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Save(string path) => this.Save(path, this.bitDepth);

        /// <summary>
        /// Saves the current image to the specified file path.
        /// </summary>
        /// <param name="path">The path to save the image to.</param>
        /// <param name="bitDepth">The color depth in bits per pixel.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Save(string path, BitDepth bitDepth)
        {
            this.CheckLoaded();

            IImageFormat format = FormatUtilities.GetFormat(path);
            var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(path));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            using (FileStream stream = File.OpenWrite(path))
            {
                return this.Save(stream, format, bitDepth);
            }
        }

        /// <summary>
        /// Saves the current image to the specified output stream.
        /// </summary>
        /// <param name="stream">The stream to save the image information to.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Save(Stream stream) => this.Save(stream, this.bitDepth);

        /// <summary>
        /// Saves the current image to the specified output stream.
        /// </summary>
        /// <param name="stream">The stream to save the image information to.</param>
        /// <param name="bitDepth">The color depth in bits per pixel.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Save(Stream stream, BitDepth bitDepth)
            => this.Save(stream, this.imageFormat, bitDepth);

        /// <summary>
        /// Saves the current image to the specified output stream.
        /// </summary>
        /// <param name="stream">The stream to save the image information to.</param>
        /// <param name="format">The format which which to encode the image.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Save(Stream stream, IImageFormat format)
            => this.Save(stream, format, this.bitDepth);

        /// <summary>
        /// Saves the current image to the specified output stream.
        /// </summary>
        /// <param name="stream">The stream to save the image information to.</param>
        /// <param name="format">The format which which to encode the image.</param>
        /// <param name="bitDepth">The color depth in bits per pixel.</param>
        /// <returns>The current <see cref="ImageFactory"/>.</returns>
        public ImageFactory Save(Stream stream, IImageFormat format, BitDepth bitDepth)
        {
            this.CheckLoaded();

            // Allow the same stream to be used as for input.
            if (stream.CanSeek)
            {
                stream.SetLength(0);
            }

            format.Save(stream, this.Image, bitDepth, this.Quality);
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return this;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.Image?.Dispose();
                this.Image = null;

                this.memoryStream?.Dispose();
                this.memoryStream = null;
            }

            this.isDisposed = true;
        }

        private ImageFactory ApplyProcessor(IGraphicsProcessor processor)
        {
            this.CheckLoaded();

            // Apply the process.
            // The format will throw an exception if something goes wrong.
            Image processed = this.imageFormat.ApplyProcessor(processor, this);
            this.Image.Dispose();
            this.Image = processed;

            // Update the metdata. This can be updated by individual processors.
            this.SetMetadata();

            return this;
        }

        private void SetMetadata()
        {
            if (this.MetadataMode == MetadataMode.All)
            {
                foreach (KeyValuePair<int, PropertyItem> item in this.PropertyItems)
                {
                    // Handle issue https://github.com/JimBobSquarePants/ImageProcessor/issues/571
                    // SetPropertyItem throws a native error if the property item is invalid for that format
                    // but there's no way to handle individual formats so we do a dumb try...catch...
                    try
                    {
                        this.Image.SetPropertyItem(item.Value);
                    }
                    catch
                    {
                        continue;
                    }
                }

                return;
            }

            ExifPropertyTag[] tags = ExifPropertyTagConstants.RequiredPropertyItems;
            switch (this.MetadataMode)
            {
                case MetadataMode.Copyright:
                    tags = ExifPropertyTagConstants.CopyrightPropertyItems;
                    break;
                case MetadataMode.CopyrightAndGeolocation:
                    tags = ExifPropertyTagConstants.CopyrightAndGeolocationPropertyItems;
                    break;
            }

            foreach (KeyValuePair<int, PropertyItem> item in this.PropertyItems)
            {
                try
                {
                    if (Array.IndexOf(tags, (ExifPropertyTag)item.Key) != -1)
                    {
                        this.Image.SetPropertyItem(item.Value);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private void CheckLoaded()
        {
            if (!this.isLoaded)
            {
                throw new ImageProcessingException("No image loaded to process!");
            }
        }
    }
}
