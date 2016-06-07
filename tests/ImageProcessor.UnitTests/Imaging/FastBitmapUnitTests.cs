// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FastBitmapUnitTests.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ImageProcessor.UnitTests.Imaging
{
    using System.Drawing;
    using FluentAssertions;
    using ImageProcessor.Imaging;
    using NUnit.Framework;

    /// <summary>
    /// Test harness for the <see cref="ImageProcessor.Imaging.FastBitmap"/> class
    /// </summary>
    [TestFixture]
    public class FastBitmapUnitTests
    {
        /// <summary>
        /// Tests that the bitmap's data is read by the fast bitmap
        /// </summary>
        [Test]
        public void BitmapIsRead()
        {
            List<string> files = new List<string>
            {
                ImageSources.GetFilePathByName("format-Penguins.jpg"),
                ImageSources.GetFilePathByName("format-Penguins.png"),
            };

            foreach (string file in files)
            {
                Bitmap bmp = new Bitmap(file);

                using (FastBitmap fbmp = new FastBitmap(bmp))
                {
                    fbmp.Width.Should().Be(bmp.Width, "because the bitmap should have been read");
                    fbmp.Height.Should().Be(bmp.Height, "because the bitmap should have been read");
                }
            }
        }

        /// <summary>
        /// Tests that modifications on the fast bitmap's bitmap are actually done
        /// </summary>
        [Test]
        public void FastBitmapModificationsAreApplied()
        {
            List<string> files = new List<string>
            {
                ImageSources.GetFilePathByName("format-Penguins.jpg"),
                ImageSources.GetFilePathByName("format-Penguins.png"),
            };

            foreach (string file in files)
            {
                Bitmap bmp = new Bitmap(file);
                Bitmap original = (Bitmap)bmp.Clone();

                using (FastBitmap fbmp = new FastBitmap(bmp))
                {
                    // draw a pink diagonal line
                    for (int i = 0; i < 10; i++)
                    {
                        fbmp.SetPixel(i, i, Color.Pink);
                    }
                }

                AssertionHelpers.AssertImagesAreDifferent(original, bmp, "because modifying the fast bitmap should have modified the original bitmap");
            }
        }
    }
}
