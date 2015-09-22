using System.Drawing;
using ImageProcessor.Imaging;
using NUnit.Framework;

namespace ImageProcessor.UnitTests.Imaging
{
    public class ImageLayerUnitTests
    {
        [TestFixture]
        public static class WhenEquals
        {
            [Test]
            public static void then_should_return_false_given_different_layers()
            {
                // Arrange
                var imageLayer1 = new ImageLayer { Position = new Point(1, 1) };
                var imageLayer2 = new ImageLayer { Position = new Point(10, 10) };

                // Act
                var result = imageLayer1.Equals(imageLayer2);

                // Assert
                Assert.That(result, Is.False);
            }

            [Test]
            public static void then_should_return_false_given_null_second_layer()
            {
                // Arrange
                var imageLayer1 = new ImageLayer { Position = new Point(1, 1) };
                ImageLayer imageLayer2 = null;

                // Act
                var result = imageLayer1.Equals(imageLayer2);

                // Assert
                Assert.That(result, Is.False);
            }

            [Test]
            public static void then_should_return_true_given_new_objects()
            {
                // Arrange
                var imageLayer1 = new ImageLayer();
                var imageLayer2 = new ImageLayer();

                // Act
                var result = imageLayer1.Equals(imageLayer2);

                // Assert
                Assert.That(result, Is.True);
            }
            
            [Test]
            public static void then_should_return_true_given_image_and_size_and_opacity_and_position_same()
            {
                // Arrange
                var imageLayer1 = new ImageLayer
                {
                    Opacity = 50,
                    Position = new Point(1, 1),
                    Size = new Size(5, 5)
                };
                var imageLayer2 = new ImageLayer
                {
                    Opacity = 50,
                    Position = new Point(1, 1),
                    Size = new Size(5, 5)
                };

                // Act
                var result = imageLayer1.Equals(imageLayer2);

                // Assert
                Assert.That(result, Is.True);
            }
        }
    }
}