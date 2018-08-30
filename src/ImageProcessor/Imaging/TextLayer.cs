// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextLayer.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates the properties required to add a layer of text to an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace ImageProcessor.Imaging
{
    using System.Drawing;
    using System.Drawing.Text;

    /// <summary>
    /// Encapsulates the properties required to add a layer of text to an image.
    /// </summary>
    public class TextLayer : IDisposable
    {
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
        private bool isDisposed = false;

        /// <summary>
        /// Gets or sets Text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Drawing.Color"/> to render the font.
        /// <remarks>
        /// <para>Defaults to black.</para>
        /// </remarks>
        /// </summary>
        public Color FontColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets the name of the font family.
        /// <remarks>
        /// <para>Defaults to generic sans-serif font family.</para>
        /// </remarks>
        /// </summary>
        public FontFamily FontFamily { get; set; } = new FontFamily(GenericFontFamilies.SansSerif);

        /// <summary>
        /// Gets or sets the size of the font in pixels.
        /// <remarks>
        /// <para>Defaults to 48 pixels.</para>
        /// </remarks>
        /// </summary>  
        public int FontSize { get; set; } = 48;

        /// <summary>
        /// Gets or sets the FontStyle of the text layer.
        /// <remarks>
        /// <para>Defaults to regular.</para>
        /// </remarks>
        /// </summary>
        public FontStyle Style { get; set; } = FontStyle.Regular;

        /// <summary>
        /// Gets or sets the Opacity of the text layer.
        /// </summary>
        public int Opacity { get; set; } = 100;

        /// <summary>
        /// Gets or sets the Position of the text layer.
        /// </summary>
        public Point? Position { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a DropShadow should be drawn.
        /// </summary>
        public bool DropShadow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the text should be rendered vertically.
        /// </summary>
        public bool Vertical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the text should be rendered right to left.
        /// </summary>
        public bool RightToLeft { get; set; }

        /// <summary>
        /// Returns a value that indicates whether the specified object is an 
        /// <see cref="TextLayer"/> object that is equivalent to 
        /// this <see cref="TextLayer"/> object.
        /// </summary>
        /// <param name="obj">
        /// The object to test.
        /// </param>
        /// <returns>
        /// True if the given object  is an <see cref="TextLayer"/> object that is equivalent to 
        /// this <see cref="TextLayer"/> object; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is TextLayer textLayer))
            {
                return false;
            }

            return this.Text == textLayer.Text
                && this.FontColor == textLayer.FontColor
                && this.FontFamily.Equals(textLayer.FontFamily)
                && this.FontSize == textLayer.FontSize
                && this.Style == textLayer.Style
                && this.DropShadow == textLayer.DropShadow
                && this.Opacity == textLayer.Opacity
                && this.Position == textLayer.Position
                && this.Vertical == textLayer.Vertical
                && this.RightToLeft == textLayer.RightToLeft;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Text?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.DropShadow.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.FontFamily?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)this.Style;
                hashCode = (hashCode * 397) ^ this.FontColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Opacity;
                hashCode = (hashCode * 397) ^ this.FontSize;
                hashCode = (hashCode * 397) ^ this.Position.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Vertical.GetHashCode();
                return (hashCode * 397) ^ this.RightToLeft.GetHashCode();
            }
        }

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        public void Dispose() => this.Dispose(true);

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">If true, the object gets disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.FontFamily?.Dispose();
                }

                this.isDisposed = true;
            }
        }
    }
}