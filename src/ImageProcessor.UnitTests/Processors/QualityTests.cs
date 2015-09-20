namespace ImageProcessor.UnitTests.Processors
{
    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Processors;

    using NUnit.Framework;

    /// <summary>
    /// The quality tests.
    /// </summary>
    public class QualityTests
    {
        /// <summary>
        /// The when constructing quality.
        /// </summary>
        [TestFixture]
        public class WhenConstructingQuality
        {
            /// <summary>
            /// The then should initialize settings.
            /// </summary>
            [Test]
            public void ThenShouldInitializeSettings()
            {
                // Arrange
                var quality = new Quality();

                // Act

                // Assert
                Assert.That(quality.Settings, Is.Not.Null);
            }
        }

        /// <summary>
        /// The when processing image.
        /// </summary>
        [TestFixture]
        public class WhenProcessingImage
        {
            /// <summary>
            /// The then should throw image processing exception given invalid dynamic parameter.
            /// </summary>
            [Test]
            public void ThenShouldThrowImageProcessingExceptionGivenInvalidDynamicParameter()
            {
                // Arrange
                var quality = new Quality();

                // Act // Assert
                Assert.Throws<ImageProcessingException>(() => quality.ProcessImage(new ImageFactory()));
            }

        }
    }
}