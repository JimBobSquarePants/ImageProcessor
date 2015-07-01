// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathLock.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides a mechanism by which a lock can be applied against a given path.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Provides a mechanism by which a lock can be applied against a given path.
    /// </summary>
    internal static class PathLock
    {
        /// <summary>
        /// The dictionary containing the asynchronous locks.
        /// </summary>
        private static readonly ConcurrentDictionary<string, AsyncLock> PathLocks = new ConcurrentDictionary<string, AsyncLock>();

        /// <summary>
        /// Returns a lock for the given path.
        /// </summary>
        /// <param name="path">
        /// The path to return the lock for.
        /// </param>
        /// <returns>
        /// The <see cref="AsyncLock"/>.
        /// </returns>
        public static AsyncLock GetLock(string path)
        {
            return PathLocks.GetOrAdd(path, new AsyncLock());
        }
    }
}
