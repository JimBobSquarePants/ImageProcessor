namespace ImageProcessor.UnitTests.Processors
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using ImageProcessor.Imaging;

    using NUnit.Framework;

    public class CropTests
    {
        [Test]
        [TestCase(10F, 14F, 10F, 14F, CropMode.Percentage)]
        [TestCase(20F, 28F, 160F, 144F, CropMode.Pixels)]
        public void ThenResultingImageSizeShouldBeLikeCropLayer(float left, float top, float right, float bottom, CropMode mode)
        {
            // When crop mode is percentage. The right and bottom values should represent
            // the percentage amount to remove from those sides. 
            const int SizeX = 200;
            const int SizeY = 200;
            int expectedWidth = 160;
            int expectedHeight = 144;

            CropLayer cl = new CropLayer(left, top, right, bottom, mode);

            // Arrange
            using (Bitmap bitmap = new Bitmap(SizeX, SizeY))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);

                memoryStream.Position = 0;

                using (ImageFactory imageFactory = new ImageFactory())
                using (ImageFactory resultImage = imageFactory.Load(memoryStream).Crop(cl))
                {
                    // Act // Assert
                    Assert.AreEqual(expectedWidth, resultImage.Image.Width);
                    Assert.AreEqual(expectedHeight, resultImage.Image.Height);
                }
            }
        }
    }
}