using System.Drawing;
using System.Runtime.Remoting.Channels;
using ImageProcessor.Imaging.Colors;
using ImageProcessor.Imaging.Quantizers.WuQuantizer;
using NUnit.Framework;

namespace ImageProcessor.UnitTests.Imaging.Quantizers.WuQuantizer
{
    public class ColorMomentUnitTests
    {
        [TestFixture]
        public static class WhenAddingColorMomentsViaOperator
        {
            [Test]
            public static void then_should_add_each_property_value_together()
            {
                // Arrange
                var colorMoment1 = new ColorMoment { Alpha = 1, Blue = 2, Green = 3, Moment = 4, Red = 5, Weight = 6};
                var colorMoment2 = new ColorMoment { Alpha = 6, Blue = 5, Green = 4, Moment = 3, Red = 2, Weight = 1};

                // Act
                var addedColor = colorMoment1 + colorMoment2;

                // Assert
                Assert.That(addedColor.Alpha, Is.EqualTo(7));
                Assert.That(addedColor.Blue, Is.EqualTo(7));
                Assert.That(addedColor.Green, Is.EqualTo(7));
                Assert.That(addedColor.Moment, Is.EqualTo(7));
                Assert.That(addedColor.Red, Is.EqualTo(7));
                Assert.That(addedColor.Weight, Is.EqualTo(7));
            }

        }

        [TestFixture]
        public static class WhenSubtractingColorMomentsViaOperator
        {
            [Test]
            public static void then_should_subtract_each_property()
            {

                // Arrange
                var colorMoment1 = new ColorMoment { Alpha = 6, Blue = 6, Green = 6, Moment = 6, Red = 6, Weight = 6 };
                var colorMoment2 = new ColorMoment { Alpha = 6, Blue = 5, Green = 4, Moment = 3, Red = 2, Weight = 1 };

                // Act
                var addedColor = colorMoment1 - colorMoment2;

                // Assert
                Assert.That(addedColor.Alpha, Is.EqualTo(0));
                Assert.That(addedColor.Blue, Is.EqualTo(1));
                Assert.That(addedColor.Green, Is.EqualTo(2));
                Assert.That(addedColor.Moment, Is.EqualTo(3));
                Assert.That(addedColor.Red, Is.EqualTo(4));
                Assert.That(addedColor.Weight, Is.EqualTo(5));
            }
        }

        [TestFixture]
        public static class WhenAddingColorMomentAndPixel
        {
            [Test]
            public static void then_should_add_each_property_value_together()
            {

                // Arrange
                var colorMoment = new ColorMoment { Alpha = 1, Blue = 2, Green = 3, Moment = 4, Red = 5, Weight = 6 };
                var color32 = new Color32(6, 5, 4, 3);

                // Act
                colorMoment.Add(color32);

                // Assert
                Assert.That(colorMoment.Alpha, Is.EqualTo(7));
                Assert.That(colorMoment.Red, Is.EqualTo(10));
                Assert.That(colorMoment.Green, Is.EqualTo(7));
                Assert.That(colorMoment.Blue, Is.EqualTo(5));
                Assert.That(colorMoment.Moment, Is.EqualTo(90f));
                Assert.That(colorMoment.Weight, Is.EqualTo(7));
            }
        }

        [TestFixture]
        public static class WhenAddingColorMomentAndPixelFast
        {
            [Test]
            public static void then_should_add_each_property_value_together()
            {

                // Arrange
                var colorMoment = new ColorMoment { Alpha = 1, Blue = 2, Green = 3, Moment = 4, Red = 5, Weight = 6 };

                // Act
                colorMoment.AddFast(ref colorMoment);

                // Assert
                Assert.That(colorMoment.Alpha, Is.EqualTo(2));
                Assert.That(colorMoment.Red, Is.EqualTo(10));
                Assert.That(colorMoment.Green, Is.EqualTo(6));
                Assert.That(colorMoment.Blue, Is.EqualTo(4));
                Assert.That(colorMoment.Moment, Is.EqualTo(8.0f));
                Assert.That(colorMoment.Weight, Is.EqualTo(12));
            }
        }

        [TestFixture]
        public static class WhenGettingAmplitude
        {
            [Test]
            public static void then_should_multiply_each_property_value_together()
            {

                // Arrange
                var colorMoment = new ColorMoment { Alpha = 1, Blue = 2, Green = 3, Moment = 4, Red = 5, Weight = 6 };

                // Act
                var amplitude = colorMoment.Amplitude();

                // Assert
                Assert.That(amplitude, Is.EqualTo(39));
            }
        }

        [TestFixture]
        public static class WhenGettingWeightedDistance
        {
            [Test]
            public static void then_should_return_amplitude_divided_by_weight_rounded_down()
            {

                // Arrange
                var colorMoment = new ColorMoment { Alpha = 1, Blue = 2, Green = 3, Moment = 4, Red = 5, Weight = 6 };

                // Act
                var amplitude = colorMoment.WeightedDistance();

                // Assert
                Assert.That(amplitude, Is.EqualTo(6));
            }
        }

        [TestFixture]
        public static class WhenGettingVariance
        {
            [Test]
            public static void then_should_return_moment_minus_amplitude_divided_by_weight()
            {

                // Arrange
                var colorMoment = new ColorMoment { Alpha = 1, Blue = 2, Green = 3, Moment = 24, Red = 5, Weight = 6 };

                // Act
                var amplitude = colorMoment.Variance();

                // Assert
                Assert.That(amplitude, Is.EqualTo(17.5f));
            }
        }
    }
}