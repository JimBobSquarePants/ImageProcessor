// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;

namespace ImageProcessor.Quantizers
{
    /// <summary>
    /// The palette color history containing the sum of all pixel data.
    /// Adapted from <see href="https://github.com/drewnoakes/nquant" />.
    /// </summary>
    internal struct PaletteColorHistory
    {
        /// <summary>
        /// The alpha component.
        /// </summary>
        public ulong Alpha;

        /// <summary>
        /// The red component.
        /// </summary>
        public ulong Red;

        /// <summary>
        /// The green component.
        /// </summary>
        public ulong Green;

        /// <summary>
        /// The blue component.
        /// </summary>
        public ulong Blue;

        /// <summary>
        /// The sum of the color components.
        /// </summary>
        public ulong Sum;

        /// <summary>
        /// Normalizes the color.
        /// </summary>
        /// <returns>
        /// The normalized <see cref="Color"/>.
        /// </returns>
        public Color ToNormalizedColor() => (this.Sum != 0) ? Color.FromArgb((int)(this.Alpha /= this.Sum), (int)(this.Red /= this.Sum), (int)(this.Green /= this.Sum), (int)(this.Blue /= this.Sum)) : Color.Empty;

        /// <summary>
        /// Adds a pixel to the color history.
        /// </summary>
        /// <param name="pixel">
        /// The pixel to add.
        /// </param>
        public void AddPixel(Bgra32 pixel)
        {
            this.Alpha += pixel.A;
            this.Red += pixel.R;
            this.Green += pixel.G;
            this.Blue += pixel.B;
            this.Sum++;
        }
    }
}
