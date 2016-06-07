// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnimationProcessMode.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Enumerated frame process modes to apply to animated images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    /// <summary>
    /// Enumerated frame process modes to apply to animated images.
    /// </summary>
    public enum AnimationProcessMode
    {
        /// <summary>
        /// Processes and keeps all the frames of an animated image.
        /// </summary>
        All,

        /// <summary>
        /// Processes and keeps only the first frame of an animated image.
        /// </summary>
        First
    }
}
