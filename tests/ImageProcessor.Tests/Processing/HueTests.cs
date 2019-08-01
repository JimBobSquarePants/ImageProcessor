using System.Collections.Generic;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class ContrastTests
    {
        private const string Category = "Contrast";

        public static IEnumerable<object[]> ContrastFiles = new[]
        {
            new object[]{ TestFiles.Gif.AnimatedPattern, 75 },
            new object[]{ TestFiles.Gif.AnimatedPattern, -25 },
            new object[]{ TestFiles.Bmp.Penguins, -25 },
            new object[]{ TestFiles.Gif.Penguins, 75 },
            new object[]{ TestFiles.Jpeg.Penguins, -25 },
            new object[]{ TestFiles.Png.Penguins, 75 }
        };

        [Fact]
        public void ContrastConstructorSetsOptions()
        {
            const int Expected = 50;
            var processor = new Contrast(Expected);

            Assert.Equal(Expected, processor.Options);
        }

        [Theory]
        [InlineData(-101)]
        [InlineData(101)]
        public void ContrastConstructorChecksInput(int percentage)
        {
            Assert.Throws<ImageProcessingException>(() => new Contrast(percentage));
        }

        [Theory]
        [MemberData(nameof(ContrastFiles))]
        public void FactoryCanSetContrast(TestFile file, int percentage)
        {
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Contrast(percentage)
                       .SaveAndCompare(file, Category, percentage);
            }
        }
    }
}
