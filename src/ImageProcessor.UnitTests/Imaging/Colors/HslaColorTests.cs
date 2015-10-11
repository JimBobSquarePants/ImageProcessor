using System;
using System.Drawing;
using ImageProcessor.Imaging.Colors;
using NUnit.Framework;

namespace ImageProcessor.UnitTests.Imaging.Colors
{
    public class HslaColorTests
    {
        [TestFixture]
        public class when_implicitly_converting_from_color_ranges
        {
            [Test]
            public void then_should_return_hsla_version_of_system_drawing_color_given_red()
            {
                // Arrange
                var color = Color.Red;

                // Act
                HslaColor hslaColor = color;

                // Assert
                Assert.That(hslaColor.H, Is.EqualTo(0));
                Assert.That(hslaColor.S, Is.EqualTo(1.0f));
                Assert.That(hslaColor.L, Is.EqualTo(.5f));
                Assert.That(hslaColor.A, Is.EqualTo(1.0f));
            }

            [Test]
            public void then_should_return_hsla_version_of_rgba_color_given_red()
            {
                // Arrange
                var rgbaColor = RgbaColor.FromRgba(0xff, 0x0, 0x0);

                // Act
                var hslaColor = (HslaColor)rgbaColor;

                // Assert
                Assert.That(hslaColor.H, Is.EqualTo(0));
                Assert.That(hslaColor.S, Is.EqualTo(1.0f));
                Assert.That(hslaColor.L, Is.EqualTo(.5f));
                Assert.That(hslaColor.A, Is.EqualTo(1.0f));
            }

            [Test]
            public void then_should_return_hsla_version_of_cmyk_color_given_red()
            {
                // Arrange
                var cmykColor = CmykColor.FromColor(Color.Red);

                // Act
                var hslaColor = (HslaColor)cmykColor;

                // Assert
                Assert.That(hslaColor.H, Is.EqualTo(0));
                Assert.That(hslaColor.S, Is.EqualTo(1.0f));
                Assert.That(hslaColor.L, Is.EqualTo(.5f));
                Assert.That(hslaColor.A, Is.EqualTo(1.0f));
            }

            [Test]
            public void then_should_return_hsla_version_of_ycbcr_color_given_red()
            {
                // Arrange
                var yCbCrColor = YCbCrColor.FromColor(Color.Red);

                // Act
                var hslaColor = (HslaColor)yCbCrColor;

                // Assert
                Assert.That(hslaColor.H, Is.EqualTo(0));
                Assert.That(hslaColor.S, Is.EqualTo(1.0f));
                Assert.That(Math.Round(hslaColor.L, 1), Is.EqualTo(.5f));   //YCbCr rounding issue
                Assert.That(hslaColor.A, Is.EqualTo(1.0f));
            }
        }

    }
}