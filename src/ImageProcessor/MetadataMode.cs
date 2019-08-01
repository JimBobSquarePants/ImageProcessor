// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor
{
    /// <summary>
    /// Enumerates the various metadata modes that control how much metadata information is stored on processing.
    /// </summary>
    public enum MetadataMode
    {
        /// <summary>
        /// Store no metadata on processing
        /// </summary>
        All,

        /// <summary>
        /// Store copyright specific metadata on processing
        /// </summary>
        Copyright,

        /// <summary>
        /// Store geolocation specific metadata on processing
        /// </summary>
        Geolocation,

        /// <summary>
        /// Store copyright and geolocation specific metadata on processing
        /// </summary>
        CopyrightAndGeolocation,

        /// <summary>
        /// Store all metadata on processing
        /// </summary>
        None
    }
}
