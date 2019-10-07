using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class DetectEdgesTests
    {
        private const string category = "DetectEdges";

        // TODO: Test all operators.
        [Theory]
        [InlineData(EdgeDetectionOperators.Sobel)]
        [InlineData(EdgeDetectionOperators.Scharr)]
        [InlineData(EdgeDetectionOperators.Laplacian3x3)]
        public void FactoryCanDetectEdges(EdgeDetectionOperators mode)
        {
            TestFile file = TestFiles.Png.Penguins;
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .DetectEdges(mode)
                       .SaveAndCompare(file, category, mode);
            }
        }
    }
}
