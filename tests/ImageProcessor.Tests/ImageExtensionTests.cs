using System.Drawing;
using Xunit;

namespace ImageProcessor.Tests
{
    public class ImageExtensionTests
    {
        [Fact]
        public void CanDeepCloneRawImage()
        {
            using (var image = new Bitmap(10, 10))
            {
                Color expected = Color.HotPink;
                image.SetPixel(0, 0, expected);

                using (Bitmap copy = image.DeepClone())
                {
                    Assert.True(image != copy);

                    Color pixel = copy.GetPixel(0, 0);
                    Assert.Equal(expected.ToArgb(), pixel.ToArgb());
                }
            }
        }

        // TODO: Per format tests.
    }
}
