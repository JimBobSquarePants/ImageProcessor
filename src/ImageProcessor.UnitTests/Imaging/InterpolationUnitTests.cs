using ImageProcessor.Imaging;
using NUnit.Framework;

namespace ImageProcessor.UnitTests.Imaging
{
    public class InterpolationUnitTests
    {
        [TestFixture]
        public static class WhenInterpolatingLanczosKernel3
        {
            [Test]
//            [TestCase(-2.25, 0.030021091449581559d)]
            [TestCase(-2, 0)]
            [TestCase(-1, 0)]
//            [TestCase(-.5, 0.60792710185402665d)]
            [TestCase(0, 1)]
//            [TestCase(.5, 0.60792710185402665d)]
            [TestCase(1, 0)]
            [TestCase(2, 0)]
//            [TestCase(2.25, 0.030021091449581559d)]
            public static void then_should_return_value_given_input(double x, double expected)
            {
                // Arrange
                var result = Interpolation.LanczosKernel3(x);

                // Act // Assert
                Assert.That(result, Is.EqualTo(expected));
            }
        }
    }
}