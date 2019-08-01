using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class BackgroundColorTests
    {
        private const string Category = "BackgroundColor";

        public static IEnumerable<object[]> BackgroundColorFiles = new[]
        {
            new object[]{ TestFiles.Png.Penguins, Color.HotPink }
        };

        [Fact]
        public void BackgroundColorConstructorSetsOptions()
        {
            Color expected = Color.HotPink;
            var processor = new BackgroundColor(expected);

            Assert.Equal(expected, processor.Options);
        }

        [Theory]
        [MemberData(nameof(BackgroundColorFiles))]
        public void FactoryCanSetBackgroundColor(TestFile file, Color color)
        {
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .BackgroundColor(color)
                       .SaveAndCompare(file, Category, color);
            }
        }
    }
}
