using System;
using System.Drawing;
using System.Drawing.Imaging;
using Xunit;

namespace ImageProcessor.Tests
{
    public class FastBitmapTests
    {
        [Theory]
        [InlineData(PixelFormat.Format1bppIndexed)]
        [InlineData(PixelFormat.Format4bppIndexed)]
        [InlineData(PixelFormat.Format8bppIndexed)]
        public void FastBitmapConstructorChecksInput(PixelFormat format)
        {
            using (var image = new Bitmap(1, 1, format))
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    using (var fast = new FastBitmap(image))
                    {
                    }
                });
            }
        }

        [Theory]
        [InlineData(PixelFormat.Format16bppRgb565)]
        [InlineData(PixelFormat.Format24bppRgb)]
        [InlineData(PixelFormat.Format32bppRgb)]
        [InlineData(PixelFormat.Format32bppArgb)]
        [InlineData(PixelFormat.Format32bppPArgb)]
        [InlineData(PixelFormat.Format48bppRgb)]
        public void FastBitmapCanManipulatePixels(PixelFormat format)
        {
            using (var image = new Bitmap(5, 5, format))
            {
                var expected = Color.FromArgb(16, 32, 64, 128);
                using (var fast = new FastBitmap(image))
                {
                    for (int y = 0; y < fast.Height; y++)
                    {
                        for (int x = 0; x < fast.Width; x++)
                        {
                            fast.SetPixel(x, y, expected);
                            Color actual = fast.GetPixel(x, y);

                            Assert.Equal(expected.ToArgb(), actual.ToArgb());
                        }
                    }
                }
            }
        }
    }
}
