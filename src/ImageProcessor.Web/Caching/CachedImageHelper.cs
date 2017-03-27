// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachedFileHelper.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides helper methods to generate cached file names and paths.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Caching
{
    using System;
    using System.IO;
    using System.Linq;

    using ImageProcessor.Web.Extensions;
    using ImageProcessor.Web.Helpers;

    /// <summary>
    /// Provides helper methods to generate cached file names and paths.
    /// </summary>
    public static class CachedImageHelper
    {
        /// <summary>
        /// Gets the cached file name from the given path.
        /// </summary>
        /// <param name="path">The path to the image. This can be the full path plus querystring.</param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetCachedImageFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            int index = path.LastIndexOf("?", StringComparison.Ordinal);

            // Prevent overflow if path ends with a "?"
            if (index == path.Length - 1)
            {
                index = 0;
            }

            return GetCachedImageFileName(path, index > 0 ? path.Substring(index + 1) : string.Empty);
        }

        /// <summary>
        /// Gets the cached file name from the given path.
        /// </summary>
        /// <param name="path">The request path to the image. This can be the full path plus querystring.</param>
        /// <param name="querystring">The request querystring.</param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetCachedImageFileName(string path, string querystring)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            // Use an sha1 hash of the full path including the querystring to create the image name.
            // That name can also be used as a key for the cached image and we should be able to use
            // The characters of that hash as sub-folders.
            string parsedExtension = ImageHelpers.Instance.GetExtension(path, querystring);
            string hashedName = (path).ToSHA1Fingerprint();

            return $"{hashedName}.{parsedExtension.Replace(".", string.Empty)}";
        }

        /// <summary>
        /// Gets the path to the cached file.
        /// </summary>
        /// <param name="cachedFolderPath">The path to the cached folder, relative or absolute.</param>
        /// <param name="cachedFileName">The cached file name.</param>
        /// <param name="makeVirtual">Whether to reverse the slashes in the path.</param>
        /// <param name="depth">How deep to nest the image files in folders.</param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetCachedPath(string cachedFolderPath, string cachedFileName, bool makeVirtual, int depth = 6)
        {
            if (string.IsNullOrWhiteSpace(cachedFolderPath))
            {
                throw new ArgumentNullException(nameof(cachedFolderPath));
            }

            if (string.IsNullOrWhiteSpace(cachedFileName))
            {
                throw new ArgumentNullException(nameof(cachedFileName));
            }

            if (depth < 0)
            {
                depth = 0;
            }

            string pathFromKey = string.Join("\\", cachedFileName.ToCharArray().Take(Math.Min(Path.GetFileNameWithoutExtension(cachedFileName).Length, depth)));

            return makeVirtual
                ? Path.Combine(cachedFolderPath, pathFromKey, cachedFileName).Replace(@"\", "/")
                : Path.Combine(cachedFolderPath, pathFromKey, cachedFileName);
        }
    }
}