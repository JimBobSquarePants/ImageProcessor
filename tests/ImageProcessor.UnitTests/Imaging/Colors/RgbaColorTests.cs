using System;
using System.Drawing;
using ImageProcessor.Imaging.Colors;
using NUnit.Framework;

namespace ImageProcessor.UnitTests.Imaging.Colors
{
    public class RgbaColorTests
    {
        [TestFixture]
        public class when_implicitly_converting_from_color_ranges
        {
            [Test]
            public void then_should_return_rgba_version_of_system_drawing_color_given_red()
            {
                // Arrange
                var color = Color.Red;

                // Act
                RgbaColor rgbaColor = color;

                // Assert
                Assert.That(rgbaColor.R, Is.EqualTo(255));
                Assert.That(rgbaColor.G, Is.EqualTo(0));
                Assert.That(rgbaColor.B, Is.EqualTo(0));
                Assert.That(rgbaColor.A, Is.EqualTo(255));
            }

            [Test]
            public void then_should_return_rgba_version_of_cmyk_color_given_red()
            {
                // Arrange
                var cmykColor = CmykColor.FromColor(Color.Red);

                // Act
                var rgbaColor = (RgbaColor)cmykColor;

                // Assert
                Assert.That(rgbaColor.R, Is.EqualTo(255));
                Assert.That(rgbaColor.G, Is.EqualTo(0));
                Assert.That(rgbaColor.B, Is.EqualTo(0));
                Assert.That(rgbaColor.A, Is.EqualTo(255));
            }

            [Test]
            public void then_should_return_rgba_version_of_hsla_color_given_red()
            {
                // Arrange
                var hslaColor = HslaColor.FromColor(Color.Red);

                // Act
                var rgbaColor = (RgbaColor)hslaColor;

                // Assert
                Assert.That(rgbaColor.R, Is.EqualTo(255));
                Assert.That(rgbaColor.G, Is.EqualTo(0));
                Assert.That(rgbaColor.B, Is.EqualTo(0));
                Assert.That(rgbaColor.A, Is.EqualTo(255));
            }

            [Test]
            public void then_should_return_rgba_version_of_ycbcr_color_given_red()
            {
                // Arrange
                var yCbCrColor = YCbCrColor.FromColor(Color.Red);

                // Act
                var rgbaColor = (RgbaColor)yCbCrColor;

                // Assert
                Assert.That(rgbaColor.R, Is.EqualTo(254)); //Conversion not perfect
                Assert.That(rgbaColor.G, Is.EqualTo(0));
                Assert.That(rgbaColor.B, Is.EqualTo(0));
                Assert.That(rgbaColor.A, Is.EqualTo(255));
            }
        }

    }
}