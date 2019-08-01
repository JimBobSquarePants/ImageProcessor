using System.Drawing;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class PixelateTests
    {
        private const string Category = "Pixelate";

        [Fact]
        public void PixelateSettingsConstructorSetsOptions()
        {
            const int Size = 1;
            var bounds = new Rectangle(1, 2, 3, 4);

            var expected = new PixelateOptions(Size, bounds);

            Assert.Equal(expected.Size, Size);
            Assert.Equal(expected.Rectangle, bounds);
        }

        [Fact]
        public void PixelateSettingsConstructorChecksInput()
        {
            Assert.Throws<ImageProcessingException>(() => new PixelateOptions(-1, default));
            Assert.Throws<ImageProcessingException>(() => new PixelateOptions(0, default));
            Assert.Throws<ImageProcessingException>(() => new PixelateOptions(-1, new Rectangle(1, 2, 3, 4)));
            Assert.Throws<ImageProcessingException>(() => new PixelateOptions(0, new Rectangle(1, 2, 3, 4)));
        }

        [Fact]
        public void PixelateConstructorSetsOptions()
        {
            var expected = new PixelateOptions(1, new Rectangle(1, 2, 3, 4));
            var processor = new Pixelate(expected);

            Assert.Equal(expected, processor.Options);
        }

        [Fact]
        public void FactoryCanPixelate()
        {
            TestFile file = TestFiles.Bmp.Penguins;
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Pixelate(4)
                       .SaveAndCompare(file, Category);
            }
        }

        [Fact]
        public void FactoryCanPixelateRectangle()
        {
            TestFile file = TestFiles.Bmp.Penguins;
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName);

                int width = factory.Image.Width;
                int height = factory.Image.Height;
                var bounds = new Rectangle(width / 4, height / 4, width / 2, height / 2);
                var options = new PixelateOptions(4, bounds);

                factory.Pixelate(options)
                       .SaveAndCompare(file, Category, bounds);
            }
        }
    }
}
