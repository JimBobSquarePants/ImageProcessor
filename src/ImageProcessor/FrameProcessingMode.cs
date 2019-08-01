// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor
{
    /// <summary>
    /// Enumerated frame process modes to apply to multiframe images.
    /// </summary>
    public enum FrameProcessingMode
    {
        /// <summary>
        /// Processes and keeps all the frames of a multiframe image.
        /// </summary>
        All,

        /// <summary>
        /// Processes and keeps only the first frame of a multiframe image.
        /// </summary>
        First
    }
}
