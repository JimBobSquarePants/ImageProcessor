// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace ImageProcessor.Quantizers
{
    /// <summary>
    /// The histogram representing the distribution of color data.
    /// Adapted from <see href="https://github.com/drewnoakes/nquant" />.
    /// </summary>
    internal class Histogram
    {
        /// <summary>
        /// The side size.
        /// </summary>
        private const int SideSize = 33;

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram"/> class.
        /// </summary>
        public Histogram()
        {
            // 47,436,840 bytes
            this.Moments = new ColorMoment[SideSize, SideSize, SideSize, SideSize];
        }

        /// <summary>
        /// Gets the collection of moments.
        /// </summary>
        public ColorMoment[,,,] Moments { get; }

        /// <summary>
        /// Clears the histogram.
        /// </summary>
        internal void Clear() => Array.Clear(this.Moments, 0, SideSize * SideSize * SideSize * SideSize);
    }
}
