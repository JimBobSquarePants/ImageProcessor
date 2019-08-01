// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;

namespace ImageProcessor.Quantizers
{
    /// <summary>
    /// Encapsulates methods to calculate the color palette of an image using
    /// a Wu color quantizer <see href="http://www.ece.mcmaster.ca/~xwu/cq.c"/>.
    /// Adapted from <see href="https://github.com/drewnoakes/nquant" />.
    /// </summary>
    public interface IWuQuantizer : IQuantizer
    {
        /// <summary>
        /// Quantizes the given image.
        /// </summary>
        /// <param name="image">The 32 bit per pixel <see cref="Image"/>.</param>
        /// <param name="alphaThreshold">
        /// The alpha threshold. All colors with an alpha value less than this will be
        /// considered fully transparent.
        /// </param>
        /// <param name="alphaFader">
        /// The alpha fader. Alpha values will be normalized to the nearest multiple of this value.
        /// </param>
        /// <returns>
        /// The quantized <see cref="Image"/>.
        /// </returns>
        Bitmap Quantize(Image image, int alphaThreshold, int alphaFader);
    }
}
