using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class ResizeTests
    {
        private const string Category = "Resize";

        [Theory]
        [InlineData(ResizeMode.Crop)]
        [InlineData(ResizeMode.Pad)]
        [InlineData(ResizeMode.BoxPad)]
        [InlineData(ResizeMode.Max)]
        [InlineData(ResizeMode.Min)]
        [InlineData(ResizeMode.Stretch)]
        public void FactoryCanResize(ResizeMode mode)
        {
            TestFile file = TestFiles.Jpeg.Penguins;
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Resize(factory.Image.Width / 2, (factory.Image.Height / 2) + 40, mode)
                       .SaveAndCompare(file, Category, mode);
            }
        }
    }
}
