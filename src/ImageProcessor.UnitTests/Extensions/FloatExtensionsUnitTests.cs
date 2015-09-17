namespace ImageProcessor.UnitTests.Extensions
{
    using ImageProcessor.Common.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// The float extensions unit tests.
    /// </summary>
    public class FloatExtensionsUnitTests
    {
        /// <summary>
        /// The when to byte.
        /// </summary>
        [TestFixture]
        public class WhenToByte
        {
            /// <summary>
            /// The then should constrain input to 255 given float more than 255.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            [Test]
            [TestCase(256)]
            [TestCase(512)]
            [TestCase(1024)]
            public void ThenShouldConstrainInputTo255GivenFloatMoreThan255(float value)
            {
                // Arrange // Act
                var result = value.ToByte();

                // Assert
                Assert.That(result, Is.EqualTo(255));
            }

            /// <summary>
            /// The then should constrain input to 0 given float less than 0.
            /// </summary>
            /// <param name="value">
            /// The value.
            /// </param>
            [Test]
            [TestCase(-256)]
            [TestCase(-512)]
            [TestCase(-1024)]
            public void ThenShouldConstrainInputTo0GivenFloatLessThan0(float value)
            {
                // Arrange // Act
                var result = value.ToByte();

                // Assert
                Assert.That(result, Is.EqualTo(0));
            }

        } 
    }
}