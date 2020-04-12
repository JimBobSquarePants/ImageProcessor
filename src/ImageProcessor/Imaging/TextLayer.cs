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
    public class TextLayer : IDisposable, IEquatable<TextLayer>
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
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is TextLayer textLayer && this.Equals(textLayer);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(TextLayer other) => other != null
            && this.Text == other.Text
            && this.FontColor == other.FontColor
            && (this.FontFamily == null || other.FontFamily == null ? this.FontFamily == other.FontFamily : this.FontFamily.Equals(other.FontFamily))
            && this.FontSize == other.FontSize
            && this.Style == other.Style
            && this.Opacity == other.Opacity
            && this.Position == other.Position
            && this.DropShadow == other.DropShadow
            && this.Vertical == other.Vertical
            && this.RightToLeft == other.RightToLeft;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => (this.Text, this.FontColor, this.FontFamily, this.FontSize, this.Style, this.Opacity, this.Position, this.DropShadow, this.Vertical, this.RightToLeft).GetHashCode();

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