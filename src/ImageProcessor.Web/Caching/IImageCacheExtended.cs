// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IImageCacheExtended.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   An extended image cache with additional configuration options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Caching
{
    /// <summary>
    /// An extended image cache with additional configuration options.
    /// </summary>
    public interface IImageCacheExtended : IImageCache
    {
        /// <summary>
        /// Gets or sets the maximum number folder levels to nest the cached images.
        /// </summary>
        int FolderDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to periodically trim the cache.
        /// </summary>
        bool TrimCache { get; set; }
    }
}
