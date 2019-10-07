using System.Collections.Generic;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class BrightnessTests
    {
        private const string category = "Brightness";

        public static IEnumerable<object[]> BrightnessFiles = new[]
        {
            new object[]{ TestFiles.Gif.AnimatedPattern, 75 },
            new object[]{ TestFiles.Bmp.Penguins, -25 },
            new object[]{ TestFiles.Gif.Penguins, 75 },
            new object[]{ TestFiles.Jpeg.Penguins, -25 },
            new object[]{ TestFiles.Png.Penguins, 75 }
        };

        [Fact]
        public void BrightnessConstructorSetsOptions()
        {
            const int Expected = 50;
            var processor = new Brightness(Expected);

            Assert.Equal(Expected, processor.Options);
        }

        [Theory]
        [InlineData(-101)]
        [InlineData(101)]
        public void BrightnessConstructorChecksInput(int percentage)
        {
            Assert.Throws<ImageProcessingException>(() => new Brightness(percentage));
        }

        [Theory]
        [MemberData(nameof(BrightnessFiles))]
        public void FactoryCanSetBrightness(TestFile file, int percentage)
        {
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Brightness(percentage)
                       .SaveAndCompare(file, category, percentage);
            }
        }
    }
}
