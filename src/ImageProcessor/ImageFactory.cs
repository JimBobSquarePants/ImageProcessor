// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageFactory.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods for processing image files in a fluent manner.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Common.Extensions;
    using ImageProcessor.Configuration;
    using ImageProcessor.Imaging;
    using ImageProcessor.Imaging.Filters.EdgeDetection;

    // using ImageProcessor.Imaging.Filters.ObjectDetection;
    using ImageProcessor.Imaging.Filters.Photo;
    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Imaging.MetaData;
    using ImageProcessor.Processors;

    /// <summary>
    /// Encapsulates methods for processing image files in a fluent manner.
    /// </summary>
    public class ImageFactory : IDisposable
    {
        /// <summary>
        /// The default quality for image files.
        /// </summary>
        private const int DefaultQuality = 90;

        /// <summary>
        /// Whether to preserve exif metadata
        /// </summary>
        private bool preserveExifData;

        /// <summary>
        /// The backup supported image format.
        /// </summary>
        private ISupportedImageFormat backupFormat;

        /// <summary>
        /// The backup collection of property items containing EXIF metadata.
        /// </summary>
        private ConcurrentDictionary<int, PropertyItem> backupExifPropertyItems;

        /// <summary>
        /// A value indicating whether this instance of the given entity has been disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// If the entity is disposed, it must not be disposed a second
        /// time. The isDisposed field is set the first time the entity
        /// is disposed. If the isDisposed field is true, then the Dispose()
        /// method will not dispose again. This help not to prolong the entity's
        /// life in the Garbage Collector.
        /// </remarks>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFactory"/> class.
        /// </summary>
        /// <param name="preserveExifData">
        /// Whether to preserve exif metadata. Defaults to false.
        /// </param>
        public ImageFactory(bool preserveExifData = false)
            : this(preserveExifData, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFactory"/> class.
        /// </summary>
        /// <param name="preserveExifData">
        /// Whether to preserve exif metadata. Defaults to false.
        /// </param>
        /// <param name="fixGamma">
        /// Whether to fix the gamma component of the image.
        /// </param>
        public ImageFactory(bool preserveExifData, bool fixGamma)
            : this(!preserveExifData ? MetaDataMode.None : MetaDataMode.All, fixGamma)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFactory"/> class.
        /// </summary>
        /// <param name="metaDataMode">The metadata mode to use</param>
        public ImageFactory(MetaDataMode metaDataMode)
           : this(metaDataMode, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFactory"/> class.
        /// </summary>
        /// <param name="metaDataMode">The metadata mode to use</param>
        /// <param name="fixGamma">Whether to fix the gamma component of the image.</param>
        public ImageFactory(MetaDataMode metaDataMode, bool fixGamma)
        {
            // Note the order here. 
            // We need to set MetaDataMode after PreserveExifData as the first option doesn't allow the granular control allowed by the constructor.
            this.PreserveExifData = metaDataMode != MetaDataMode.None;
            this.MetaDataMode = metaDataMode;
            this.ExifPropertyItems = new ConcurrentDictionary<int, PropertyItem>();
            this.backupExifPropertyItems = new ConcurrentDictionary<int, PropertyItem>();
            this.FixGamma = fixGamma;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ImageFactory"/> class.
        /// </summary>
        /// <remarks>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </remarks>
        ~ImageFactory()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the color depth in number of bits per pixel to save the image with.
        /// This can be used to change the bit depth of images that can be saved with different
        /// bit depths such as TIFF.
        /// </summary>
        public long CurrentBitDepth { get; internal set; }

        /// <summary>
        /// Gets the path to the local image for manipulation.
        /// </summary>
        public string ImagePath { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the image factory should process the file.
        /// </summary>
        public bool ShouldProcess { get; private set; }

        /// <summary>
        /// Gets the supported image format.
        /// </summary>
        public ISupportedImageFormat CurrentImageFormat { get; private set; }

        /// <summary>
        /// Gets the metadata mode.
        /// </summary>
        public MetaDataMode MetaDataMode { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to preserve exif metadata.
        /// This property is only settable for backwards compatibility. Set <see cref="MetaDataMode"/> via the constructor instead.
        /// </summary>
        public bool PreserveExifData
        {
            get => this.preserveExifData;
            set
            {
                this.preserveExifData = value;
                this.MetaDataMode = this.preserveExifData ? MetaDataMode.All : MetaDataMode.None;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to fix the gamma component of the current image.
        /// </summary>
        public bool FixGamma { get; set; }

        /// <summary>
        /// Gets or the current gamma value.
        /// </summary>
        public float CurrentGamma { get; private set; }

        /// <summary>
        /// Gets or sets the collection of property items containing EXIF metadata.
        /// </summary>
        public ConcurrentDictionary<int, PropertyItem> ExifPropertyItems { get; set; }

        /// <summary>
        /// Gets or the local image for manipulation.
        /// </summary>
        public Image Image { get; internal set; }

        /// <summary>
        /// Gets or sets the process mode for frames in animated images.
        /// </summary>
        public AnimationProcessMode AnimationProcessMode { get; set; }

        /// <summary>
        /// Gets or sets the stream for storing any input stream to prevent disposal.
        /// </summary>
        internal Stream InputStream { get; set; }

        /// <summary>
        /// Loads the image to process. Always call this method first.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="T:System.IO.Stream"/> containing the image information.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Load(Stream stream)
        {
            var memoryStream = new MemoryStream();

            // Copy the stream. Disposal of the input stream is the responsibility
            // of the user.
            stream.CopyTo(memoryStream);

            // Set the position to 0 afterward.
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            ISupportedImageFormat format = FormatUtilities.GetFormat(memoryStream);
            if (format == null)
            {
                throw new ImageFormatException("Input stream is not a supported format.");
            }

            // Set our image as the memory stream value.
            this.Image = format.Load(memoryStream);

            // Save the bit depth
            this.CurrentBitDepth = Image.GetPixelFormatSize(this.Image.PixelFormat);

            // Store the stream so we can dispose of it later.
            this.InputStream = memoryStream;

            // Set the other properties.
            format.Quality = DefaultQuality;
            format.IsIndexed = FormatUtilities.IsIndexed(this.Image);

            this.backupFormat = format;
            this.CurrentImageFormat = format;

            // Always load the data.
            // TODO. Some custom data doesn't seem to get copied by default methods.
            foreach (int id in this.Image.PropertyIdList)
            {
                this.ExifPropertyItems[id] = this.Image.GetPropertyItem(id);
            }

            if (this.CurrentImageFormat is IAnimatedImageFormat imageFormat)
            {
                imageFormat.AnimationProcessMode = this.AnimationProcessMode;
            }

            this.backupExifPropertyItems = new ConcurrentDictionary<int, PropertyItem>(this.ExifPropertyItems);

            // Ensure the image is in the most efficient format but don't reserve exif data.
            Image formatted = this.Image.Copy(this.AnimationProcessMode);

            this.Image.Dispose();
            this.Image = formatted;

            this.ShouldProcess = true;

            return this;
        }

        /// <summary>
        /// Loads the image to process. Always call this method first.
        /// </summary>
        /// <param name="imagePath">The absolute path to the image to load.</param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Load(string imagePath)
        {
            var fileInfo = new FileInfo(imagePath);
            if (fileInfo.Exists)
            {
                this.ImagePath = imagePath;

                // Open a file stream to prevent the need for lock.
                using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    ISupportedImageFormat format = FormatUtilities.GetFormat(fileStream);

                    if (format == null)
                    {
                        throw new ImageFormatException("Input stream is not a supported format.");
                    }

                    var memoryStream = new MemoryStream();

                    // Copy the stream.
                    fileStream.CopyTo(memoryStream);

                    // Set the position to 0 afterward.
                    memoryStream.Position = 0;

                    // Set our image as the memory stream value.
                    this.Image = format.Load(memoryStream);

                    // Save the bit depth
                    this.CurrentBitDepth = Image.GetPixelFormatSize(this.Image.PixelFormat);

                    // Store the stream so we can dispose of it later.
                    this.InputStream = memoryStream;

                    // Set the other properties.
                    format.Quality = DefaultQuality;
                    format.IsIndexed = FormatUtilities.IsIndexed(this.Image);

                    this.backupFormat = format;
                    this.CurrentImageFormat = format;

                    // Always load the data.
                    foreach (PropertyItem propertyItem in this.Image.PropertyItems)
                    {
                        this.ExifPropertyItems[propertyItem.Id] = propertyItem;
                    }

                    this.backupExifPropertyItems = new ConcurrentDictionary<int, PropertyItem>(this.ExifPropertyItems);

                    if (this.CurrentImageFormat is IAnimatedImageFormat imageFormat)
                    {
                        imageFormat.AnimationProcessMode = this.AnimationProcessMode;
                    }

                    // Ensure the image is in the most efficient format but don't reserve exif data.
                    Image formatted = this.Image.Copy(this.AnimationProcessMode);

                    this.Image.Dispose();
                    this.Image = formatted;

                    this.ShouldProcess = true;
                }
            }
            else
            {
                throw new FileNotFoundException(imagePath);
            }

            return this;
        }

        /// <summary>
        /// Loads the image to process from an array of bytes. Always call this method first.
        /// </summary>
        /// <param name="bytes">
        /// The <see cref="T:System.Byte"/> containing the image information.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Load(byte[] bytes)
        {
            var memoryStream = new MemoryStream(bytes);

            ISupportedImageFormat format = FormatUtilities.GetFormat(memoryStream);

            if (format == null)
            {
                throw new ImageFormatException("Input stream is not a supported format.");
            }

            // Set our image as the memory stream value.
            this.Image = format.Load(memoryStream);

            // Save the bit depth
            this.CurrentBitDepth = Image.GetPixelFormatSize(this.Image.PixelFormat);

            // Store the stream so we can dispose of it later.
            this.InputStream = memoryStream;

            // Set the other properties.
            format.Quality = DefaultQuality;
            format.IsIndexed = FormatUtilities.IsIndexed(this.Image);

            this.backupFormat = format;
            this.CurrentImageFormat = format;

            // Always load the data.
            foreach (int id in this.Image.PropertyIdList)
            {
                this.ExifPropertyItems[id] = this.Image.GetPropertyItem(id);
            }

            if (this.CurrentImageFormat is IAnimatedImageFormat imageFormat)
            {
                imageFormat.AnimationProcessMode = this.AnimationProcessMode;
            }

            // Ensure the image is in the most efficient format but don't reserve exif data.
            Image formatted = this.Image.Copy(this.AnimationProcessMode);

            this.Image.Dispose();
            this.Image = formatted;

            this.ShouldProcess = true;

            return this;
        }

        /// <summary>
        /// Loads the image to process from an array of bytes. Always call this method first.
        /// </summary>
        /// <param name="image">
        /// The <see cref="T:System.Drawing.Image"/> to load.
        /// The original image is untouched during manipulation as a copy is made. Disposal of the input image is the responsibility of the user.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Load(Image image)
        {
            // Try saving with the raw format. This might not be possible if the image was created
            // in-memory so we fall back to BMP to keep in line with the default in System.Drawing
            // if no format found.
            var memoryStream = new MemoryStream();
            ISupportedImageFormat format = new BitmapFormat();
            try
            {
                image.Save(memoryStream, image.RawFormat);
                format = ImageProcessorBootstrapper.Instance.SupportedImageFormats
                        .First(f => f.ImageFormat.Equals(image.RawFormat));
            }
            catch
            {
                image.Save(memoryStream, ImageFormat.Bmp);
            }

            if (format is IAnimatedImageFormat imageFormat)
            {
                imageFormat.AnimationProcessMode = this.AnimationProcessMode;
            }

            // Ensure the image is in the most efficient format.
            // Set our image.
            this.Image = image.Copy(this.AnimationProcessMode);

            // Save the bit depth
            this.CurrentBitDepth = Image.GetPixelFormatSize(this.Image.PixelFormat);

            // Store the stream so we can dispose of it later.
            this.InputStream = memoryStream;

            // Set the other properties.
            format.Quality = DefaultQuality;
            format.IsIndexed = FormatUtilities.IsIndexed(this.Image);

            this.backupFormat = format;
            this.CurrentImageFormat = format;

            // Always load the data.
            foreach (int id in this.Image.PropertyIdList)
            {
                this.ExifPropertyItems[id] = this.Image.GetPropertyItem(id);
            }

            this.ShouldProcess = true;

            return this;
        }

        /// <summary>
        /// Resets the current image to its original loaded state.
        /// </summary>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Reset()
        {
            if (this.ShouldProcess)
            {
                // Set our new image as the memory stream value.
                if (this.InputStream.CanSeek)
                {
                    this.InputStream.Position = 0;
                }

                // Reset properties.
                this.CurrentImageFormat = this.backupFormat;
                this.ExifPropertyItems = new ConcurrentDictionary<int, PropertyItem>(this.backupExifPropertyItems);
                this.CurrentImageFormat.Quality = DefaultQuality;

                Image newImage = this.backupFormat.Load(this.InputStream);

                // Dispose and reassign the image.
                // Ensure the image is in the most efficient format.
                Image formatted = newImage.Copy(this.AnimationProcessMode);

                newImage.Dispose();
                this.Image.Dispose();
                this.Image = formatted;
            }

            return this;
        }

        /// <summary>
        /// Changes the opacity of the current image.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images opacity.
        /// Any integer between 0 and 100.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Alpha(int percentage)
        {
            if (this.ShouldProcess)
            {
                // Sanitize the input.
                // You can't make an image less transparent.
                if (percentage < 0 || percentage > 99)
                {
                    return this;
                }

                var alpha = new Alpha { DynamicParameter = percentage };
                this.backupFormat.ApplyProcessor(alpha.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Performs auto-rotation to ensure that EXIF defined rotation is reflected in
        /// the final image.
        /// </summary>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory AutoRotate()
        {
            if (this.ShouldProcess)
            {
                var autoRotate = new AutoRotate();
                this.backupFormat.ApplyProcessor(autoRotate.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Alters the bit depth of the current image.
        /// <remarks>
        /// This can only be used to change the bit depth of images that can be saved
        /// by <see cref="System.Drawing"/> with different bit depths such as TIFF.
        /// </remarks>
        /// </summary>
        /// <param name="bitDepth">A value over 0.</param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory BitDepth(long bitDepth)
        {
            if (bitDepth > 0 && this.ShouldProcess)
            {
                this.CurrentBitDepth = bitDepth;
            }

            return this;
        }

        /// <summary>
        /// Changes the brightness of the current image.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images brightness.
        /// Any integer between -100 and 100.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Brightness(int percentage)
        {
            if (this.ShouldProcess)
            {
                // Sanitize the input.
                if (percentage > 100 || percentage < -100 || percentage == 0)
                {
                    return this;
                }

                var brightness = new Brightness { DynamicParameter = percentage };
                this.backupFormat.ApplyProcessor(brightness.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Changes the background color of the current image.
        /// </summary>
        /// <param name="color">
        /// The <see cref="T:System.Drawing.Color"/> to paint the image with.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory BackgroundColor(Color color)
        {
            if (this.ShouldProcess)
            {
                var backgroundColor = new BackgroundColor { DynamicParameter = color };
                this.backupFormat.ApplyProcessor(backgroundColor.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Constrains the current image, resizing it to fit within the given dimensions whilst keeping its aspect ratio.
        /// </summary>
        /// <param name="size">
        /// The <see cref="T:System.Drawing.Size"/> containing the maximum width and height to set the image to.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Constrain(Size size)
        {
            if (this.ShouldProcess)
            {
                var layer = new ResizeLayer(size, ResizeMode.Max);

                return this.Resize(layer);
            }

            return this;
        }

        /// <summary>
        /// Changes the contrast of the current image.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images contrast.
        /// Any integer between -100 and 100.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Contrast(int percentage)
        {
            if (this.ShouldProcess)
            {
                // Sanitize the input.
                if (percentage > 100 || percentage < -100)
                {
                    return this;
                }

                var contrast = new Contrast { DynamicParameter = percentage };
                this.backupFormat.ApplyProcessor(contrast.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Crops the current image to the given location and size.
        /// </summary>
        /// <param name="rectangle">
        /// The <see cref="T:System.Drawing.Rectangle"/> containing the coordinates to crop the image to.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Crop(Rectangle rectangle)
        {
            if (this.ShouldProcess)
            {
                var cropLayer = new CropLayer(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, CropMode.Pixels);
                return this.Crop(cropLayer);
            }

            return this;
        }

        /// <summary>
        /// Crops the current image to the given location and size.
        /// </summary>
        /// <param name="cropLayer">
        /// The <see cref="Imaging.CropLayer"/> containing the coordinates and mode to crop the image with.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Crop(CropLayer cropLayer)
        {
            if (this.ShouldProcess)
            {
                var crop = new Crop { DynamicParameter = cropLayer };
                this.backupFormat.ApplyProcessor(crop.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Detects the edges in the current image.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="IEdgeFilter"/> to detect edges with.
        /// </param>
        /// <param name="greyscale">
        /// Whether to convert the image to greyscale first - Defaults to true.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory DetectEdges(IEdgeFilter filter, bool greyscale = true)
        {
            if (this.ShouldProcess)
            {
                var detectEdges = new DetectEdges { DynamicParameter = new Tuple<IEdgeFilter, bool>(filter, greyscale) };
                this.backupFormat.ApplyProcessor(detectEdges.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Sets the resolution of the image.
        /// <remarks>
        /// This method sets both the bitmap data and EXIF resolution if available.
        /// </remarks>
        /// </summary>
        /// <param name="horizontal">The horizontal resolution.</param>
        /// <param name="vertical">The vertical resolution.</param>
        /// <param name="unit">
        /// The unit of measure for the horizontal resolution and the vertical resolution.
        /// Defaults to inches
        /// </param>
        /// <returns>
        /// The <see cref="ImageFactory"/>.
        /// </returns>
        public ImageFactory Resolution(int horizontal, int vertical, PropertyTagResolutionUnit unit = PropertyTagResolutionUnit.Inch)
        {
            if (this.ShouldProcess)
            {
                // Sanitize the input.
                if (horizontal < 0 || vertical < 0)
                {
                    return this;
                }

                var resolution =
                    new Tuple<int, int, PropertyTagResolutionUnit>(horizontal, vertical, unit);

                var dpi = new Resolution { DynamicParameter = resolution };
                this.backupFormat.ApplyProcessor(dpi.ProcessImage, this);
            }

            return this;
        }

        // public ImageFactory DetectObjects(HaarCascade cascade, bool drawRectangles = true, Color color = default(Color))
        // {
        //    if (this.ShouldProcess)
        //    {
        //        DetectObjects detectObjects = new DetectObjects { DynamicParameter = cascade };
        //        this.backupFormat.ApplyProcessor(detectObjects.ProcessImage, this);
        //    }
        //     return this;
        // }

        /// <summary>
        /// Crops an image to the area of greatest entropy.
        /// </summary>
        /// <param name="threshold">
        /// The threshold in bytes to control the entropy.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory EntropyCrop(byte threshold = 128)
        {
            if (this.ShouldProcess)
            {
                var autoCrop = new EntropyCrop { DynamicParameter = threshold };
                this.backupFormat.ApplyProcessor(autoCrop.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Applies a filter to the current image. Use the <see cref="MatrixFilters"/> class to
        /// assign the correct filter.
        /// </summary>
        /// <param name="matrixFilter">
        /// The <see cref="IMatrixFilter"/> of the filter to add to the image.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Filter(IMatrixFilter matrixFilter)
        {
            if (this.ShouldProcess)
            {
                var filter = new Filter { DynamicParameter = matrixFilter };
                this.backupFormat.ApplyProcessor(filter.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Flips the current image either horizontally or vertically.
        /// </summary>
        /// <param name="flipVertically">
        /// Whether to flip the image vertically.
        /// </param>
        /// <param name="flipBoth">
        /// Whether to flip the image both vertically and horizontally.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Flip(bool flipVertically = false, bool flipBoth = false)
        {
            if (this.ShouldProcess)
            {
                RotateFlipType rotateFlipType;
                if (flipBoth)
                {
                    rotateFlipType = RotateFlipType.RotateNoneFlipXY;
                }
                else
                {
                    rotateFlipType = flipVertically
                        ? RotateFlipType.RotateNoneFlipY
                        : RotateFlipType.RotateNoneFlipX;
                }

                var flip = new Flip { DynamicParameter = rotateFlipType };
                this.backupFormat.ApplyProcessor(flip.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Sets the output format of the current image to the matching <see cref="T:System.Drawing.Imaging.ImageFormat"/>.
        /// </summary>
        /// <param name="format">The <see cref="ISupportedImageFormat"/>. to set the image to.</param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Format(ISupportedImageFormat format)
        {
            if (this.ShouldProcess)
            {
                this.CurrentImageFormat = format;

                // Apply any fomatting quirks.
                // this.backupFormat.ApplyProcessor(factory => factory.Image, this);
            }

            return this;
        }

        /// <summary>
        /// Adjust the gamma (intensity of the light) component of the given image.
        /// </summary>
        /// <param name="value">
        /// The value to adjust the gamma by (typically between .2 and 5).
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Gamma(float value)
        {
            if (this.ShouldProcess)
            {
                // Sanitize the input.
                if (value > 5 || value < .1)
                {
                    return this;
                }

                this.CurrentGamma = value;
                var gamma = new Gamma { DynamicParameter = value };
                this.backupFormat.ApplyProcessor(gamma.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Uses a Gaussian kernel to blur the current image.
        /// <remarks>
        /// <para>
        /// The sigma and threshold values applied to the kernel are
        /// 1.4 and 0 respectively.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="size">
        /// The size to set the Gaussian kernel to.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory GaussianBlur(int size)
        {
            if (this.ShouldProcess && size > 0)
            {
                var layer = new GaussianLayer(size);
                return this.GaussianBlur(layer);
            }

            return this;
        }

        /// <summary>
        /// Uses a Gaussian kernel to blur the current image.
        /// </summary>
        /// <param name="gaussianLayer">
        /// The <see cref="T:ImageProcessor.Imaging.GaussianLayer"/> for applying sharpening and
        /// blurring methods to an image.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory GaussianBlur(GaussianLayer gaussianLayer)
        {
            if (this.ShouldProcess)
            {
                var gaussianBlur = new GaussianBlur { DynamicParameter = gaussianLayer };
                this.backupFormat.ApplyProcessor(gaussianBlur.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Uses a Gaussian kernel to sharpen the current image.
        /// <remarks>
        /// <para>
        /// The sigma and threshold values applied to the kernel are
        /// 1.4 and 0 respectively.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="size">
        /// The size to set the Gaussian kernel to.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory GaussianSharpen(int size)
        {
            if (this.ShouldProcess && size > 0)
            {
                var layer = new GaussianLayer(size);
                return this.GaussianSharpen(layer);
            }

            return this;
        }

        /// <summary>
        /// Uses a Gaussian kernel to sharpen the current image.
        /// </summary>
        /// <param name="gaussianLayer">
        /// The <see cref="T:ImageProcessor.Imaging.GaussianLayer"/> for applying sharpening and
        /// blurring methods to an image.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory GaussianSharpen(GaussianLayer gaussianLayer)
        {
            if (this.ShouldProcess)
            {
                var gaussianSharpen = new GaussianSharpen { DynamicParameter = gaussianLayer };
                this.backupFormat.ApplyProcessor(gaussianSharpen.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Alters the hue of the current image changing the overall color.
        /// </summary>
        /// <param name="degrees">
        /// The angle by which to alter the images hue.
        /// Any integer between 0 and 360.
        /// </param>
        /// <param name="rotate">
        /// Whether to rotate the hue of the current image altering each color
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Hue(int degrees, bool rotate = false)
        {
            // Sanitize the input.
            if (degrees > 360 || degrees < 0 || (degrees == 0 && rotate))
            {
                return this;
            }

            if (this.ShouldProcess)
            {
                var hue = new Hue { DynamicParameter = new Tuple<int, bool>(degrees, rotate) };
                this.backupFormat.ApplyProcessor(hue.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Converts the current image to a CMYK halftone representation of that image.
        /// </summary>
        /// <param name="comicMode">
        /// Whether to trace over the current image and add borders to add a comic book effect.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Halftone(bool comicMode = false)
        {
            if (this.ShouldProcess)
            {
                var halftone = new Halftone { DynamicParameter = comicMode };
                this.backupFormat.ApplyProcessor(halftone.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Applies the given image mask to the current image.
        /// </summary>
        /// <param name="imageLayer">
        /// The <see cref="T:ImageProcessor.Imaging.ImageLayer"/> containing the <see cref="Image"/>
        /// and <see cref="Point"/> properties necessary to mask the image.
        /// <para>
        /// The point property is used to place the image mask if it not the same dimensions as the original image.
        /// If no position is set, the mask will be centered within the image.
        /// </para>
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Mask(ImageLayer imageLayer)
        {
            if (this.ShouldProcess)
            {
                var mask = new Mask { DynamicParameter = imageLayer };
                this.backupFormat.ApplyProcessor(mask.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Adds a image overlay to the current image.
        /// </summary>
        /// <param name="imageLayer">
        /// The <see cref="T:ImageProcessor.Imaging.ImageLayer"/> containing the properties necessary to add
        /// the image overlay to the image.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Overlay(ImageLayer imageLayer)
        {
            if (this.ShouldProcess)
            {
                var watermark = new Overlay { DynamicParameter = imageLayer };
                this.backupFormat.ApplyProcessor(watermark.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Pixelates an image with the given size.
        /// </summary>
        /// <param name="pixelSize">
        /// The size of the pixels to create.</param>
        /// <param name="rectangle">
        /// The area in which to pixelate the image. If not set, the whole image is pixelated.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Pixelate(int pixelSize, Rectangle? rectangle = null)
        {
            if (this.ShouldProcess && pixelSize > 0)
            {
                var pixelate = new Pixelate { DynamicParameter = new Tuple<int, Rectangle?>(pixelSize, rectangle) };
                this.backupFormat.ApplyProcessor(pixelate.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Alters the output quality of the current image.
        /// <remarks>
        /// This method will only effect the output quality of jpeg images
        /// </remarks>
        /// </summary>
        /// <param name="percentage">A value between 1 and 100 to set the quality to.</param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Quality(int percentage)
        {
            if (percentage <= 100 && percentage >= 0 && this.ShouldProcess)
            {
                this.CurrentImageFormat.Quality = percentage;
            }

            return this;
        }

        /// <summary>
        /// Replaces a color within the current image.
        /// </summary>
        /// <param name="target">
        /// The target <see cref="System.Drawing.Color"/>.
        /// </param>
        /// <param name="replacement">
        /// The replacement <see cref="System.Drawing.Color"/>.
        /// </param>
        /// <param name="fuzziness">
        /// A value between 0 and 128 with which to alter the target detection accuracy.
        /// </param>
        /// <returns>
        /// The <see cref="ImageFactory"/>.
        /// </returns>
        public ImageFactory ReplaceColor(Color target, Color replacement, int fuzziness = 0)
        {
            // Sanitize the input.
            if (fuzziness < 0 || fuzziness > 128)
            {
                return this;
            }

            if (this.ShouldProcess && target != Color.Empty && replacement != Color.Empty)
            {
                var replaceColor = new ReplaceColor
                {
                    DynamicParameter = new Tuple<Color, Color, int>(target, replacement, fuzziness)
                };
                this.backupFormat.ApplyProcessor(replaceColor.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// </summary>
        /// <param name="size">
        /// The <see cref="T:System.Drawing.Size"/> containing the width and height to set the image to.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Resize(Size size)
        {
            if (this.ShouldProcess)
            {
                int width = size.Width;
                int height = size.Height;

                var resizeLayer = new ResizeLayer(new Size(width, height));
                return this.Resize(resizeLayer);
            }

            return this;
        }

        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// </summary>
        /// <param name="resizeLayer">
        /// The <see cref="ResizeLayer"/> containing the properties required to resize the image.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Resize(ResizeLayer resizeLayer)
        {
            if (this.ShouldProcess)
            {
                var resizeSettings = new Dictionary<string, string>
                {
                    { "MaxWidth", resizeLayer.Size.Width.ToString("G") },
                    { "MaxHeight", resizeLayer.Size.Height.ToString("G") }
                };

                var resize = new Resize { DynamicParameter = resizeLayer, Settings = resizeSettings };
                this.backupFormat.ApplyProcessor(resize.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Rotates the current image by the given angle.
        /// </summary>
        /// <param name="degrees">
        /// The angle at which to rotate the image in degrees.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Rotate(float degrees)
        {
            if (this.ShouldProcess)
            {
                var rotate = new Rotate { DynamicParameter = degrees };
                this.backupFormat.ApplyProcessor(rotate.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Rotates the image without expanding the canvas to fit the image.
        /// </summary>
        /// <param name="degrees">
        /// The angle at which to rotate the image in degrees.
        /// </param>
        /// <param name="keepSize">
        /// Whether to keep the original image dimensions.
        /// <para>
        /// If set to true, the image is zoomed to fit the bounding area.
        /// </para>
        /// <para>
        /// If set to false, the area is cropped to fit the rotated image.
        /// </para>
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory" /> class.
        /// </returns>
        public ImageFactory RotateBounded(float degrees, bool keepSize = false)
        {
            if (this.ShouldProcess)
            {
                var rotate = new RotateBounded { DynamicParameter = new Tuple<float, bool>(degrees, keepSize) };
                this.backupFormat.ApplyProcessor(rotate.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Adds rounded corners to the current image.
        /// </summary>
        /// <param name="radius">
        /// The radius at which the corner will be rounded.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory RoundedCorners(int radius)
        {
            if (this.ShouldProcess)
            {
                if (radius < 0)
                {
                    radius = 0;
                }

                var roundedCornerLayer = new RoundedCornerLayer(radius);

                var roundedCorners = new RoundedCorners { DynamicParameter = roundedCornerLayer };
                this.backupFormat.ApplyProcessor(roundedCorners.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Adds rounded corners to the current image.
        /// </summary>
        /// <param name="roundedCornerLayer">
        /// The <see cref="T:ImageProcessor.Imaging.RoundedCornerLayer"/> containing the properties to round corners on the image.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory RoundedCorners(RoundedCornerLayer roundedCornerLayer)
        {
            if (this.ShouldProcess)
            {
                if (roundedCornerLayer.Radius < 0)
                {
                    roundedCornerLayer.Radius = 0;
                }

                var roundedCorners = new RoundedCorners { DynamicParameter = roundedCornerLayer };
                this.backupFormat.ApplyProcessor(roundedCorners.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Changes the saturation of the current image.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images saturation.
        /// Any integer between -100 and 100.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Saturation(int percentage)
        {
            if (this.ShouldProcess)
            {
                // Sanitize the input.
                if (percentage > 100 || percentage < -100)
                {
                    return this;
                }

                var saturate = new Saturation { DynamicParameter = percentage };
                this.backupFormat.ApplyProcessor(saturate.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Tints the current image with the given color.
        /// </summary>
        /// <param name="color">
        /// The <see cref="T:System.Drawing.Color"/> to tint the image with.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Tint(Color color)
        {
            if (this.ShouldProcess)
            {
                var tint = new Tint { DynamicParameter = color };
                this.backupFormat.ApplyProcessor(tint.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Converts the current image to black and white using the given threshold
        /// </summary>
        /// <param name="thresh">The threshold to use to seperate black and white values</param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Threshold(int thresh)
        {
            if (this.ShouldProcess)
            {
                var threshold = new Threshold { DynamicParameter = thresh };
                this.backupFormat.ApplyProcessor(threshold.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Adds a vignette image effect to the current image.
        /// </summary>
        /// <param name="color">
        /// The <see cref="T:System.Drawing.Color"/> to tint the image with. Defaults to black.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Vignette(Color? color = null)
        {
            if (this.ShouldProcess)
            {
                var vignette = new Vignette
                {
                    DynamicParameter = color.HasValue && !color.Equals(Color.Transparent)
                                        ? color.Value
                                        : Color.Black
                };

                this.backupFormat.ApplyProcessor(vignette.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Adds a text based watermark to the current image.
        /// </summary>
        /// <param name="textLayer">
        /// The <see cref="T:ImageProcessor.Imaging.TextLayer"/> containing the properties necessary to add
        /// the text based watermark to the image.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Watermark(TextLayer textLayer)
        {
            if (this.ShouldProcess)
            {
                var watermark = new Watermark { DynamicParameter = textLayer };
                this.backupFormat.ApplyProcessor(watermark.ProcessImage, this);
            }

            return this;
        }

        /// <summary>
        /// Saves the current image to the specified file path. If the extension does not
        /// match the correct extension for the current format it will be replaced by the
        /// correct default value.
        /// </summary>
        /// <param name="filePath">The path to save the image to.</param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Save(string filePath)
        {
            if (this.ShouldProcess)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                this.SetMetaData();
                this.Image = this.CurrentImageFormat.Save(filePath, this.Image, this.CurrentBitDepth);
            }

            return this;
        }

        /// <summary>
        /// Saves the current image to the specified output stream.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="T:System.IO.MemoryStream"/> to save the image information to.
        /// </param>
        /// <returns>
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public ImageFactory Save(Stream stream)
        {
            if (this.ShouldProcess)
            {
                // Allow the same stream to be used as for input.
                if (stream.CanSeek)
                {
                    stream.SetLength(0);
                }

                this.SetMetaData();
                this.Image = this.CurrentImageFormat.Save(stream, this.Image, this.CurrentBitDepth);
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
            }

            return this;
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
                if (this.Image != null)
                {
                    // Dispose of the memory stream from Load and the image.
                    if (this.InputStream != null)
                    {
                        this.InputStream.Dispose();
                        this.InputStream = null;
                    }

                    this.Image.Dispose();
                    this.Image = null;
                }
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // Note disposing is done.
            this.isDisposed = true;
        }

        /// <summary>
        /// Sets the metadata based on the the current <see cref="MetaDataMode"/>
        /// The image 
        /// </summary>
        private void SetMetaData()
        {
            if (this.MetaDataMode == MetaDataMode.All)
            {
                foreach (KeyValuePair<int, PropertyItem> item in this.ExifPropertyItems)
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
            switch (this.MetaDataMode)
            {
                case MetaDataMode.Copyright:
                    tags = ExifPropertyTagConstants.CopyrightPropertyItems;
                    break;
                case MetaDataMode.CopyrightAndGeolocation:
                    tags = ExifPropertyTagConstants.CopyrightAndGeolocationPropertyItems;
                    break;
            }

            foreach (KeyValuePair<int, PropertyItem> item in this.ExifPropertyItems)
            {
                try
                {
                    if (tags.Contains((ExifPropertyTag)item.Key))
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
    }
}