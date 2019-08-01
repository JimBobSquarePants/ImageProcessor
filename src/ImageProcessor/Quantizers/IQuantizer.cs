// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;

namespace ImageProcessor.Quantizers
{
    /// <summary>
    /// Defines the contract for allowing quantization of images.
    /// </summary>
    public interface IQuantizer
    {
        /// <summary>
        /// Quantize an image and returns the resulting output bitmap.
        /// TODO: This should copy metadata.
        /// </summary>
        /// <param name="source">The image to quantize.</param>
        /// <returns><see cref="Bitmap"/>.</returns>
        Bitmap Quantize(Image source);
    }
}
