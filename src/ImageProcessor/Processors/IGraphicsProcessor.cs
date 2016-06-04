// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGraphicsProcessor.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Defines properties and methods for ImageProcessor Plugins.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Processors
{
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// Defines properties and methods for ImageProcessor Plugins.
    /// </summary>
    public interface IGraphicsProcessor
    {
        /// <summary>
        /// Gets or sets the DynamicParameter.
        /// </summary>
        dynamic DynamicParameter { get; set; }

        /// <summary>
        /// Gets or sets any additional settings required by the processor.
        /// </summary>
        Dictionary<string, string> Settings { get; set; }

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
        Image ProcessImage(ImageFactory factory);
    }
}
