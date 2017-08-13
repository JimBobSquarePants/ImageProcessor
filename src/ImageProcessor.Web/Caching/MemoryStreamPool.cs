// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemoryStreamPool.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The memory stream pool manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Caching
{
    using Microsoft.IO;

    /// <summary>
    /// The memory stream pool manager.
    /// </summary>
    public static class MemoryStreamPool
    {
        /// <summary>
        /// The default shared recyclable memory stream manager
        /// </summary>
        public static RecyclableMemoryStreamManager Shared => new RecyclableMemoryStreamManager();
    }
}
