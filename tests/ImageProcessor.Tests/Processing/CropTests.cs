using System.Drawing;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class CropTests
    {
        private const string category = "Crop";

        [Fact]
        public void CropSettingsConstructorSetsOptions()
        {
            const int Left = 1;
            const int Top = 1;
            const int Right = 1;
            const int Bottom = 1;

            var expected = new CropOptions(Left, Top, Right, Bottom, CropMode.Percentage);

            Assert.Equal(expected.Left, Left);
            Assert.Equal(expected.Top, Top);
            Assert.Equal(expected.Right, Right);
            Assert.Equal(expected.Bottom, Bottom);
        }

        [Fact]
        public void CropSettingsConstructorChecksInput()
        {
            Assert.Throws<ImageProcessingException>(() => new CropOptions(-1, 0, 0, 0));
            Assert.Throws<ImageProcessingException>(() => new CropOptions(0, -1, 0, 0));
            Assert.Throws<ImageProcessingException>(() => new CropOptions(0, 0, -1, 0));
            Assert.Throws<ImageProcessingException>(() => new CropOptions(0, 0, 0, -1));
        }

        [Fact]
        public void CropConstructorSetsOptions()
        {
            var expected = new CropOptions(1, 2, 3, 4, CropMode.Percentage);
            var processor = new Crop(expected);

            Assert.Equal(expected, processor.Options);
        }

        [Fact]
        public void FactoryCanCropRectangle()
        {
            // Test our issue crop.
            TestFile file = TestFiles.Jpeg.EXIFCropIssue559;
            var bounds = new Rectangle(939, 439, 2778, 2778);
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Crop(bounds)
                       .SaveAndCompare(file, category, bounds);
            }
        }

        [Fact]
        public void FactoryCanCropPercentile()
        {
            // Test our issue crop.
            TestFile file = TestFiles.Jpeg.Penguins;
            var settings = new CropOptions(15, 25, 10, 5, CropMode.Percentage);
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Crop(settings)
                       .SaveAndCompare(file, category, settings);
            }
        }
    }
}
