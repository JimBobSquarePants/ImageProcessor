using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ImageProcessor.UnitTests
{
    public class ImageSources
    {
        /// <summary>
        /// The list of images. Designed to speed up the tests a little.
        /// </summary>
        private static IEnumerable<FileInfo> imagesInfos;

        /// <summary>
        /// Gets all the input files in the Images folder
        /// </summary>
        /// <param name="extensions">The image extensions to retrieve. Can be null to get all.</param>
        /// <returns>The list of files.</returns>
        public static IEnumerable<FileInfo> GetInputImageFiles(params string[] extensions)
        {
            if (imagesInfos == null)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetFullPath(TestContext.CurrentContext.TestDirectory + "../../../Images"));
                imagesInfos = GetFilesByExtensions(directoryInfo, ".jpg", ".jpeg", ".png", ".gif", ".tiff", ".bmp", ".webp");
            }

            if (extensions.Any())
            {
                return imagesInfos.Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
            }

            return imagesInfos;
        }

        /// <summary>
        /// Gets the input image path by name.
        /// </summary>
        /// <param name="name">The name of the image to return the path for.</param>
        /// <returns></returns>
        public static string GetFilePathByName(string name)
        {
            return GetInputImageFiles()
                       .First(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                       .FullName;
        }

        /// <summary>
        /// Gets the files matching the given extensions.
        /// </summary>
        /// <param name="dir">The <see cref="System.IO.DirectoryInfo"/>.</param>
        /// <param name="extensions">The extensions.</param>
        /// <returns>A collection of <see cref="System.IO.FileInfo"/></returns>
        /// <exception cref="System.ArgumentNullException">
        /// The extensions variable is null.
        /// </exception>
        private static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions");
            }

            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
        }
    }
}
