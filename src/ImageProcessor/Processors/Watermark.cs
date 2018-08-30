// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Watermark.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods to add a watermark text overlay to an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Text;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Imaging;
    using ImageProcessor.Imaging.MetaData;

    /// <summary>
    /// Encapsulates methods to add a watermark text overlay to an image.
    /// </summary>
    public class Watermark : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Watermark"/> class.
        /// </summary>
        public Watermark() => this.Settings = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets DynamicParameter.
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
            Image image = factory.Image;

            try
            {
                TextLayer textLayer = this.DynamicParameter;
                string text = textLayer.Text;
                int opacity = Math.Min((int)Math.Ceiling((textLayer.Opacity / 100f) * 255), 255);
                int fontSize = textLayer.FontSize;
                FontStyle fontStyle = textLayer.Style;
                bool fallbackUsed = false;

                // We want to make sure that any orientation Metadata is updated to ensure watermarks 
                // are written correctly.
                RotateFlipType? flipType = this.GetRotateFlipType(factory);
                if (flipType.HasValue)
                {
                    image.RotateFlip(flipType.Value);
                }

                using (var graphics = Graphics.FromImage(image))
                {
                    using (Font font = this.GetFont(textLayer.FontFamily, fontSize, fontStyle))
                    {
                        using (var drawFormat = new StringFormat(StringFormat.GenericTypographic))
                        {
                            StringFormatFlags? formatFlags = this.GetFlags(textLayer);
                            if (formatFlags != null)
                            {
                                drawFormat.FormatFlags = formatFlags.Value;
                            }

                            using (Brush brush = new SolidBrush(Color.FromArgb(opacity, textLayer.FontColor)))
                            {
                                Point? origin = textLayer.Position;

                                // Work out the size of the text.
                                SizeF textSize = graphics.MeasureString(text, font, new SizeF(image.Width, image.Height), drawFormat);

                                // We need to ensure that there is a position set for the watermark
                                if (origin == null)
                                {
                                    int x = textLayer.RightToLeft
                                        ? 0
                                        : (int)(image.Width - textSize.Width) / 2;
                                    int y = (int)(image.Height - textSize.Height) / 2;
                                    origin = new Point(x, y);

                                    fallbackUsed = true;
                                }

                                // Set the hinting and draw the text.
                                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                                // Create bounds for the text.
                                RectangleF bounds;
                                if (textLayer.DropShadow)
                                {
                                    // Shadow opacity should change with the base opacity.
                                    int shadowOpacity = opacity - (int)Math.Ceiling((30 / 100f) * 255);
                                    int finalShadowOpacity = shadowOpacity > 0 ? shadowOpacity : 0;

                                    using (Brush shadowBrush = new SolidBrush(Color.FromArgb(finalShadowOpacity, Color.Black)))
                                    {
                                        // Scale the shadow position to match the font size.
                                        // Magic number but it's based on artistic preference.
                                        int shadowDiff = (int)Math.Ceiling(fontSize / 24f);
                                        var shadowPoint = new Point(origin.Value.X + shadowDiff, origin.Value.Y + shadowDiff);

                                        // Set the bounds so any overlapping text will wrap.
                                        if (textLayer.RightToLeft && fallbackUsed)
                                        {
                                            bounds = new RectangleF(shadowPoint, new SizeF(image.Width - ((int)(image.Width - textSize.Width) / 2) - shadowPoint.X, image.Height - shadowPoint.Y));
                                        }
                                        else
                                        {
                                            bounds = new RectangleF(shadowPoint, new SizeF(image.Width - shadowPoint.X, image.Height - shadowPoint.Y));
                                        }

                                        graphics.DrawString(text, font, shadowBrush, bounds, drawFormat);
                                    }
                                }

                                // Set the bounds so any overlapping text will wrap.
                                if (textLayer.RightToLeft && fallbackUsed)
                                {
                                    bounds = new RectangleF(origin.Value, new SizeF(image.Width - ((int)(image.Width - textSize.Width) / 2), image.Height - origin.Value.Y));
                                }
                                else
                                {
                                    bounds = new RectangleF(origin.Value, new SizeF(image.Width - origin.Value.X, image.Height - origin.Value.Y));
                                }

                                graphics.DrawString(text, font, brush, bounds, drawFormat);
                            }
                        }
                    }

                    // Flip the image back.
                    if (flipType.HasValue)
                    {
                        RotateFlipType value = flipType.Value;

                        if (value == RotateFlipType.Rotate270FlipNone)
                        {
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }
                        else if (value == RotateFlipType.Rotate90FlipNone)
                        {
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        }
                        else
                        {
                            image.RotateFlip(value);
                        }
                    }

                    return image;
                }
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Returns the correct <see cref="T:System.Drawing.Font"/> for the given parameters.
        /// </summary>
        /// <param name="fontFamily">
        /// The name of the font.
        /// </param>
        /// <param name="fontSize">
        /// The font size.
        /// </param>
        /// <param name="fontStyle">
        /// The <see cref="T:System.Drawing.FontStyle"/> style.
        /// </param>
        /// <returns>
        /// The correct <see cref="T:System.Drawing.Font"/>
        /// </returns>
        private Font GetFont(FontFamily fontFamily, int fontSize, FontStyle fontStyle)
        {
            try
            {
                // Clone the font family and use it. Disposing of the family in the TextLayer is 
                // the responsibility of the user. 
                using (var clone = new FontFamily(fontFamily.Name))
                {
                    return new Font(clone, fontSize, fontStyle, GraphicsUnit.Pixel);
                }
            }
            catch
            {
                using (FontFamily genericFontFamily = FontFamily.GenericSansSerif)
                {
                    return new Font(genericFontFamily, fontSize, fontStyle, GraphicsUnit.Pixel);
                }
            }
        }

        /// <summary>
        /// Returns the correct flags for the given text layer.
        /// </summary>
        /// <param name="textLayer">
        /// The <see cref="TextLayer"/> to return the flags for.
        /// </param>
        /// <returns>
        /// The <see cref="StringFormatFlags"/>.
        /// </returns>
        private StringFormatFlags? GetFlags(TextLayer textLayer)
        {
            if (textLayer.Vertical && textLayer.RightToLeft)
            {
                return StringFormatFlags.DirectionVertical | StringFormatFlags.DirectionRightToLeft;
            }

            if (textLayer.Vertical)
            {
                return StringFormatFlags.DirectionVertical;
            }

            if (textLayer.RightToLeft)
            {
                return StringFormatFlags.DirectionRightToLeft;
            }

            return null;
        }

        /// <summary>
        /// Gets the correct <see cref="Nullable{RotateFlipType}"/> to ensure that the watermarked image is 
        /// correct orientation when the watermark is applied.
        /// </summary>
        /// <param name="factory">The current <see cref="ImageFactory"/>.</param>
        /// <returns>
        /// The <see cref="Nullable{RotateFlipType}"/>.
        /// </returns>
        private RotateFlipType? GetRotateFlipType(ImageFactory factory)
        {
            const int Orientation = (int)ExifPropertyTag.Orientation;
            if (factory.PreserveExifData && factory.ExifPropertyItems.ContainsKey(Orientation))
            {
                int rotationValue = factory.ExifPropertyItems[Orientation].Value[0];
                switch (rotationValue)
                {
                    case 8: // Rotated 90 right
                        // De-rotate:
                        return RotateFlipType.Rotate270FlipNone;

                    case 3: // Bottoms up
                        return RotateFlipType.Rotate180FlipNone;

                    case 6: // Rotated 90 left
                        return RotateFlipType.Rotate90FlipNone;
                }
            }

            return null;
        }
    }
}