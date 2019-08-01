using System.Collections.Generic;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class HueTests
    {
        private const string Category = "Hue";

        public static IEnumerable<object[]> HueFiles = new[]
        {
            new object[]{ TestFiles.Gif.AnimatedPattern, 45 },
            new object[]{ TestFiles.Gif.AnimatedPattern, 90 },
            new object[]{ TestFiles.Bmp.Penguins, 135 },
            new object[]{ TestFiles.Gif.Penguins, 180 },
            new object[]{ TestFiles.Jpeg.Penguins, 225 },
            new object[]{ TestFiles.Png.Penguins, 270 }
        };

        [Fact]
        public void HueConstructorSetsOptions()
        {
            const int Expected = 50;
            var processor = new Hue(Expected);

            Assert.Equal(Expected, processor.Options);
        }

        [Theory]
        [MemberData(nameof(HueFiles))]
        public void FactoryCanSetHue(TestFile file, int percentage)
        {
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .Hue(percentage)
                       .SaveAndCompare(file, Category, percentage);
            }
        }
    }
}
