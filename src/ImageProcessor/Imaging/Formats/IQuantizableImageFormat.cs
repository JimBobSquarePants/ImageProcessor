// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQuantizableImageFormat.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The IndexedImageFormat interface for identifying quantizable image formats.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Formats
{
    using ImageProcessor.Imaging.Quantizers;

    /// <summary>
    /// The IndexedImageFormat interface for identifying quantizable image formats.
    /// </summary>
    public interface IQuantizableImageFormat
    {
        /// <summary>
        /// Gets or sets the quantizer for reducing the image palette.
        /// </summary>
        IQuantizer Quantizer { get; set; }
    }
}
