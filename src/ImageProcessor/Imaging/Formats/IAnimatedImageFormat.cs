// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnimatedImageFormat.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
// The IAnimatedImageFormat interface for identifying animated image formats.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Formats
{
    /// <summary>
    /// The IAnimatedImageFormat interface for identifying animated image formats.
    /// </summary>
    public interface IAnimatedImageFormat
    {
        /// <summary>
        /// Gets or sets the animation process mode.
        /// </summary>
        AnimationProcessMode AnimationProcessMode { get; set; }
    }
}
