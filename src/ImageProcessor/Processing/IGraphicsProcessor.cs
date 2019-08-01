// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Defines the contract for graphics processors.
    /// </summary>
    public interface IGraphicsProcessor
    {
        /// <summary>
        /// Returns a new image frame from the source with the process applied.
        /// </summary>
        /// <param name="factory">The <see cref="ImageFactory"/> class.</param>
        /// <param name="frame">The source image frame.</param>
        /// <returns>The <see cref="Image"/>.</returns>
        Image ProcessImageFrame(ImageFactory factory, Image frame);
    }

    /// <summary>
    /// Defines the contract for graphics processors with options.
    /// </summary>
    /// <typeparam name="T">The type of options.</typeparam>
    public interface IGraphicsProcessor<T> : IGraphicsProcessor
    {
        /// <summary>
        /// Gets the options.
        /// </summary>
        T Options { get; }
    }
}
