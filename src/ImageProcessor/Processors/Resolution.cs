// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Resolution.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods to change the resolution of the image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Imaging.MetaData;

    /// <summary>
    /// Encapsulates methods to change the resolution of the image.
    /// </summary>
    public class Resolution : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Resolution"/> class.
        /// </summary>
        public Resolution()
        {
            this.Settings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the dynamic parameter.
        /// </summary>
        public dynamic DynamicParameter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any additional settings required by the processor.
        /// </summary>
        public Dictionary<string, string> Settings
        {
            get;
            set;
        }

        /// <summary>
        /// Processes the image.
        /// </summary>
        /// <param name="factory">
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class containing
        /// the image to process.
        /// </param>
        /// <returns>
        /// The processed image from the current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public Image ProcessImage(ImageFactory factory)
        {
            const float InchInCm = 0.3937007874015748f;
            Image image = factory.Image;

            try
            {
                Tuple<int, int, PropertyTagResolutionUnit> resolution = this.DynamicParameter;

                // Set the bitmap resolution data.
                // Ensure that the resolution is recalculated for bitmap since it only 
                // supports inches.
                if (resolution.Item3 == PropertyTagResolutionUnit.Cm)
                {
                    float h = resolution.Item1 / InchInCm;
                    float v = resolution.Item2 / InchInCm;
                    ((Bitmap)image).SetResolution(h, v);
                }
                else
                {
                    ((Bitmap)image).SetResolution(resolution.Item1, resolution.Item2);
                }

                if (factory.PreserveExifData && factory.ExifPropertyItems.Any())
                {
                    // Set the horizontal EXIF data.
                    int horizontalKey = (int)ExifPropertyTag.XResolution;
                    PropertyItem horizontal = this.GetResolutionItem(horizontalKey, resolution.Item1);

                    // Set the vertical EXIF data.
                    int verticalKey = (int)ExifPropertyTag.YResolution;
                    PropertyItem vertical = this.GetResolutionItem(verticalKey, resolution.Item1);

                    factory.ExifPropertyItems[horizontalKey] = horizontal;
                    factory.ExifPropertyItems[verticalKey] = vertical;

                    // Set the unit data
                    int unitKey = (int)ExifPropertyTag.ResolutionUnit;
                    PropertyItem unit = FormatUtilities.CreatePropertyItem();
                    unit.Id = unitKey;
                    unit.Len = 2;
                    unit.Type = (short)ExifPropertyTagType.Int16;
                    Int32Converter measure = (int)resolution.Item3;
                    unit.Value = new[] { measure.Byte1, measure.Byte2 };

                    // TODO: Create nice streamline getter/setters for EXIF.
                    factory.ExifPropertyItems[unitKey] = unit;
                }
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }

            return image;
        }

        /// <summary>
        /// Gets a <see cref="PropertyItem"/> for the given resolution and direction.
        /// </summary>
        /// <param name="id">
        /// The id of the tag that sets the direction.
        /// </param>
        /// <param name="resolution">The resolution.</param>
        /// <returns>
        /// The <see cref="PropertyItem"/>.
        /// </returns>
        private PropertyItem GetResolutionItem(int id, int resolution)
        {
            int length = 8;
            short type = (short)ExifPropertyTagType.Rational;
            Int32Converter denominator = 1;

            PropertyItem propertyItem = FormatUtilities.CreatePropertyItem();
            propertyItem.Id = id;
            propertyItem.Type = type;
            Int32Converter numerator = resolution;

            byte[] resolutionBytes =
                        {
                            numerator.Byte1, numerator.Byte2, numerator.Byte3, numerator.Byte4, 
                            denominator.Byte1, denominator.Byte2, denominator.Byte3, denominator.Byte4
                        };

            propertyItem.Len = length;
            propertyItem.Value = resolutionBytes;

            return propertyItem;
        }
    }
}
