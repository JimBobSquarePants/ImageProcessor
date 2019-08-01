using System.Collections.Generic;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class SaturationTests
    {
        private const string Category = "Saturation";

        public static IEnumerable<object[]> SaturationFiles = new[]
        {
            new object[]{ TestFiles.Gif.AnimatedPattern, 75 },
            new object[]{ TestFiles.Gif.AnimatedPattern, -25 },
            new object[]{ TestFiles.Bmp.Penguins, -25 },
            new object[]{ TestFiles.Gif.Penguins, 75 },
            new object[]{ TestFiles.Jpeg.Penguins, -25 },
            new object[]{ TestFiles.Png.Penguins, 75 }
        };

        [Fact]
        public void SaturationConstructorSetsOptions()
        {
            const int Expected = 50;
            var processor = new Saturation(Expected);

            Assert.Equal(Expected, processor.Options);
        }

        [Theory]
        [InlineData(-101)]
        [InlineData(101)]
        public void SaturationConstructorChecksInput(int percentage)
        {
            Assert.Throws<ImageProcessingException>(() => new Saturation(percentage));
        }

        [Theory]
        [MemberData(nameof(SaturationFiles))]
        public void FactoryCanSetSaturation(TestFile file, int percentage)
        {
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Saturation(percentage)
                       .SaveAndCompare(file, Category, percentage);
            }
        }
    }
}
