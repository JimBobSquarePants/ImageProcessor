using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class AutoRotateTests
    {
        private const string Category = "AutoRotate";

        [Theory]
        [InlineData(MetadataMode.All)]
        [InlineData(MetadataMode.Geolocation)]
        [InlineData(MetadataMode.Copyright)]
        [InlineData(MetadataMode.None)]
        public void FactoryCanAutoRotate(MetadataMode metadataMode)
        {
            using (var factory = new ImageFactory(metadataMode))
            {
                TestFile file = TestFiles.Jpeg.AutoRtotate;

                factory.Load(file.FullName)
                       .AutoRotate()
                       .SaveAndCompare(file, Category, metadataMode);
            }
        }
    }
}
