using System;
using System.Drawing;
using ImageProcessor.Imaging.Colors;
using NUnit.Framework;
namespace ImageProcessor.UnitTests.Imaging.Colors
{
    public class CmykColorTests
    {
        [TestFixture]
        public class when_to_string
        {
            [Test]
            public void then_should_return_empty_given_empty()
            {
                // Arrange
                var cmyk = CmykColor.Empty;

                // Act
                var s = cmyk.ToString();

                // Assert
                Assert.That(s, Is.EqualTo("CmykColor [Empty]"));
            }

            [Test]
            public void then_should_return_bracketed_cmyk_colors_given_color()
            {
                // Arrange
                var cmyk = CmykColor.FromCmykColor(100f, 100f, 100f, 10f);

                // Act
                var s = cmyk.ToString();

                // Assert
                Assert.That(s, Is.EqualTo("CmykColor [ C=100, M=100, Y=100, K=10]"));
            }
        }

        [TestFixture]
        public class when_implicitly_converting_from_color_ranges
        {
            [Test]
            public void then_should_return_cmyk_version_of_system_drawing_color_given_red()
            {
                // Arrange
                var color = Color.Red;

                // Act
                CmykColor cmyk = color;

                // Assert
                Assert.That(cmyk.C, Is.EqualTo(0));
                Assert.That(cmyk.M, Is.EqualTo(100));
                Assert.That(cmyk.Y, Is.EqualTo(100));
                Assert.That(cmyk.K, Is.EqualTo(0));
            }

            [Test]
            public void then_should_return_cmyk_version_of_rgba_color_given_red()
            {
                // Arrange
                var rgbaColor = RgbaColor.FromRgba(0xff, 0x0, 0x0);

                // Act
                var cmyk = (CmykColor)rgbaColor;

                // Assert
                Assert.That(cmyk.C, Is.EqualTo(0));
                Assert.That(cmyk.M, Is.EqualTo(100));
                Assert.That(cmyk.Y, Is.EqualTo(100));
                Assert.That(cmyk.K, Is.EqualTo(0));
            }
            
            [Test]
            public void then_should_return_cmyk_version_of_hsla_color_given_red()
            {
                // Arrange
                var hslaColor = HslaColor.FromHslaColor(0f, 1f, .5f, 1f);

                // Act
                var cmyk = (CmykColor)hslaColor;

                // Assert
                Assert.That(cmyk.C, Is.EqualTo(0));
                Assert.That(cmyk.M, Is.EqualTo(100));
                Assert.That(cmyk.Y, Is.EqualTo(100));
                Assert.That(cmyk.K, Is.EqualTo(0));
            }

            /// <summary>
            /// http://www.equasys.de/colorconversion.html
            /// </summary>
            [Test]
            public void then_should_return_cmyk_version_of_ycbcr_color_given_red()
            {
                // Arrange
                var yCbCrColor = YCbCrColor.FromColor(Color.Red); // :*( [See Below]

                // Act
                var cmyk = (CmykColor)yCbCrColor;

                // Assert
                Assert.That(cmyk.C, Is.EqualTo(0));
                Assert.That(Math.Round(cmyk.M), Is.EqualTo(100)); // See, here's the thing
                Assert.That(cmyk.Y, Is.EqualTo(100));             // YCbCr doesn't happily convert to RGB
                Assert.That(Math.Round(cmyk.K), Is.EqualTo(0));   // Sad for me
            }
        }
    }
}