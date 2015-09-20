// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DirectoryInfoExtensionsUnitTests.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.UnitTests.Extensions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Web.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// The directory info extensions unit tests.
    /// </summary>
    public class DirectoryInfoExtensionsUnitTests
    {
        /// <summary>
        /// The when safe enumerable directories.
        /// </summary>
        [TestFixture]
        public class WhenSafeEnumerableDirectories
        {
            /// <summary>
            /// The test directory root.
            /// </summary>
            private const string TestDirectoryRoot = "DirectoryInfoExtensionsTests";

            /// <summary>
            /// The directory count.
            /// </summary>
            private const int DirectoryCount = 4;

            /// <summary>
            /// The directory list.
            /// </summary>
            private IEnumerable<string> _directoryList;

            /// <summary>
            /// The setup directories.
            /// </summary>
            [SetUp]
            public void SetupDirectories()
            {
                _directoryList = Enumerable.Range(1, DirectoryCount).Select(i => string.Format("{0}/TestDirectory{1}",TestDirectoryRoot, i));
                foreach (var directory in _directoryList)
                {
                    Directory.CreateDirectory(directory);
                }
            }

            /// <summary>
            /// The remove directories.
            /// </summary>
            [TearDown]
            public void RemoveDirectories()
            {
                Directory.Delete("DirectoryInfoExtensionsTests", true);
            }

            /// <summary>
            /// The then should return enumerable directories given path with sub directories.
            /// </summary>
            [Test]
            public void ThenShouldReturnEnumerableDirectoriesGivenPathWithSubDirectories()
            {
                // Arrange
                var info = new DirectoryInfo(TestDirectoryRoot);

                // Act
                var directories = info.SafeEnumerateDirectories();

                // Assert
                Assert.That(directories, Is.EquivalentTo(_directoryList.Select(s => new DirectoryInfo(s))));
            }
            /// <summary>
            /// The then should return empty enumerable directories given path with invalid directory
            /// </summary>
            [Test]
            public void ThenShouldReturnEmptyEnumerableDirectoriesGivenPathWithInvalidDirectory()
            {
                // Arrange
                var info = new DirectoryInfo(string.Format("Bad{0}", TestDirectoryRoot));

                // Act
                var directories = info.SafeEnumerateDirectories();

                // Assert
                Assert.That(directories, Is.EquivalentTo(Enumerable.Empty<DirectoryInfo>()));
            }
        }

        /// <summary>
        /// The when safe enumerable directories async.
        /// </summary>
        [TestFixture]
        public class WhenSafeEnumerableDirectoriesAsync
        {
            /// <summary>
            /// The test directory root.
            /// </summary>
            private const string TestDirectoryRoot = "DirectoryInfoExtensionsTests";

            /// <summary>
            /// The directory count.
            /// </summary>
            private const int DirectoryCount = 6;

            /// <summary>
            /// The directory list.
            /// </summary>
            private IEnumerable<string> _directoryList;

            /// <summary>
            /// The setup directories.
            /// </summary>
            [SetUp]
            public void SetupDirectories()
            {
                _directoryList = Enumerable.Range(1, DirectoryCount).Select(i => string.Format("{0}/TestDirectory{1}",TestDirectoryRoot, i));
                foreach (var directory in _directoryList)
                {
                    Directory.CreateDirectory(directory);
                }
            }

            /// <summary>
            /// The remove directories.
            /// </summary>
            [TearDown]
            public void RemoveDirectories()
            {
                Directory.Delete("DirectoryInfoExtensionsTests", true);
            }
            /// <summary>
            /// Then should return enumerable directories asynchronously given path with subdirectories
            /// </summary>
            [Test]
            public async void ThenShouldReturnEnumerableDirectoriesAsyncGivenPathWithSubDirectories()
            {
                // Arrange
                var info = new DirectoryInfo(TestDirectoryRoot);
                var asyncResult = info.SafeEnumerateDirectoriesAsync();

                // Act
                var directories = await asyncResult;

                // Assert
                Assert.That(directories, Is.EquivalentTo(_directoryList.Select(s => new DirectoryInfo(s))));
            }

            /// <summary>
            /// Then return empty enumerable directories asynchronously given invalid directory
            /// </summary>
            [Test]
            public async void ThenReturnEmptyEnumerableGivenInvalidDirectory()
            {
                // Arrange
                var info = new DirectoryInfo(string.Format("Bad{0}", TestDirectoryRoot));
                var asyncResult = info.SafeEnumerateDirectoriesAsync();

                // Act
                var directories = await asyncResult;

                // Assert
                Assert.That(directories, Is.EqualTo(Enumerable.Empty<DirectoryInfo>()));
            }

        }
    }
}