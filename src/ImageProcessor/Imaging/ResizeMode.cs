// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResizeMode.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Enumerated resize modes to apply to resized images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    /// <summary>
    /// Enumerated resize modes to apply to resized images.
    /// </summary>
    public enum ResizeMode
    {
        /// <summary>
        /// Pads the resized image to fit the bounds of its container.
        /// If only one dimension is passed, will maintain the original aspect ratio.
        /// </summary>
        Pad,

        /// <summary>
        /// Stretches the resized image to fit the bounds of its container.
        /// </summary>
        Stretch,

        /// <summary>
        /// Crops the resized image to fit the bounds of its container.
        /// </summary>
        Crop,

        /// <summary>
        /// Constrains the resized image to fit the bounds of its container maintaining
        /// the original aspect ratio. 
        /// </summary>
        Max,

        /// <summary>
        /// Resizes the image until the shortest side reaches the set given dimension.
        /// Sets <see cref="ResizeLayer.Upscale"/> to <c>false</c> only allowing downscaling.
        /// </summary>
        Min,

        /// <summary>
        /// Pads the image to fit the bound of the container without resizing the 
        /// original source. Sets <see cref="ResizeLayer.Upscale"/> to <c>true</c>.
        /// When downscaling, performs the same functionality as <see cref="ResizeMode.Pad"/>
        /// </summary>
        BoxPad
    }
}
