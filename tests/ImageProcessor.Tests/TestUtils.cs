using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ImageProcessor.Tests
{
    public static class TestUtils
    {
        private static readonly object Lock = new object();

        private static IEnumerable<TestFile> Files;

        /// <summary>
        /// Gets the input image path by name.
        /// </summary>
        /// <param name="name">The name of the image to return the path for.</param>
        /// <returns>The <see cref="TestFile"/>.</returns>
        public static TestFile GetTestFileByName(string name)
        {
            return GetInputImageFiles().First(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<TestFile> GetInputImageFiles(params string[] extensions)
        {
            lock (Lock)
            {
                if (Files is null)
                {
                    string codeBase = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
                    const string Root = "../../../../../Images/";
                    var input = new DirectoryInfo(Path.GetFullPath(codeBase + Root + "Input"));
                    string expected = Path.GetFullPath(codeBase + Root + "Expected");
                    string actual = Path.GetFullPath(codeBase + Root + "Actual");

                    // TODO: Concat supported format extensions
                    Files = GetFilesByExtensions(input, expected, actual, ".jpg", ".jpeg", ".jfif", ".png", ".gif", ".tiff", ".tif", ".bmp", ".webp");
                }
            }

            if (extensions.Length > 0)
            {
                return Files.Where(x => extensions.Contains(x.Extension, StringComparer.OrdinalIgnoreCase));
            }

            return Files;
        }

        private static IEnumerable<TestFile> GetFilesByExtensions(DirectoryInfo input, string expected, string actual, params string[] extensions)
        {
            IEnumerable<FileInfo> files = input.EnumerateFiles();
            return files.Where(x => extensions.Contains(x.Extension, StringComparer.OrdinalIgnoreCase))
                        .Select(x => new TestFile(x, expected, actual));
        }
    }
}
