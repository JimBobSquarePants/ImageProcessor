using System.Collections.Generic;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class AlphaTests
    {
        private const string Category = "Alpha";

        public static IEnumerable<object[]> AlphaFiles = new[]
        {
            new object[]{ TestFiles.Jpeg.Penguins, 25 },
            new object[]{ TestFiles.Png.Penguins, 25 },
            new object[]{ TestFiles.Png.Penguins, 75 }
        };

        [Fact]
        public void AlphaConstructorSetsOptions()
        {
            const int Expected = 50;
            var processor = new Alpha(Expected);

            Assert.Equal(Expected, processor.Options);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void AlphaConstructorChecksInput(int percentage)
        {
            Assert.Throws<ImageProcessingException>(() => new Alpha(percentage));
        }

        [Theory]
        [MemberData(nameof(AlphaFiles))]
        public void FactoryCanSetAlpha(TestFile file, int percentage)
        {
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Alpha(percentage)
                       .SaveAndCompare(file, Category, percentage);
            }
        }
    }
}
