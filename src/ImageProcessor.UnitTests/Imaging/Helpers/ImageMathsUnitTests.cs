// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageMathsUnitTests.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Test harness for the image math unit tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.UnitTests.Imaging.Helpers
{
    using System;
    using System.Drawing;

    using FluentAssertions;
    using FluentAssertions.Equivalency;

    using ImageProcessor.Imaging.Helpers;

    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    /// <summary>
    /// Test harness for the image math unit tests
    /// </summary>
    public class ImageMathsUnitTests
    {
        /// <summary>
        /// The when getting rotated bounding rectangle is calculated.
        /// </summary>
        [TestFixture]
        public class WhenGettingRotatedBoundingRectangleIsCalculated
        {
            /// <summary>
            /// Tests that the bounding rectangle of a rotated image is calculated
            /// </summary>
            /// <param name="width">The width of the image.</param>
            /// <param name="height">The height of the image.</param>
            /// <param name="angle">The rotation angle.</param>
            /// <param name="expectedWidth">The expected width.</param>
            /// <param name="expectedHeight">The expected height.</param>
            [Test]
            [TestCase(100, 100, 45, 141, 141)]
            [TestCase(100, 100, 30, 137, 137)]
            [TestCase(100, 200, 50, 217, 205)]
            [TestCase(100, 200, -50, 217, 205)]
            public void BoundingRotatedRectangleIsCalculated(
                int width,
                int height,
                float angle,
                int expectedWidth,
                int expectedHeight)
            {
                Rectangle result = ImageMaths.GetBoundingRotatedRectangle(width, height, angle);

                result.Width.Should().Be(expectedWidth, "because the rotated width should have been calculated");
                result.Height.Should().Be(expectedHeight, "because the rotated height should have been calculated");
            }

            /// <summary>
            /// Tests that the zoom needed for an "inside" rotation is calculated
            /// </summary>
            /// <param name="imageWidth">Width of the image.</param>
            /// <param name="imageHeight">Height of the image.</param>
            /// <param name="angle">The rotation angle.</param>
            /// <param name="expected">The expected zoom.</param>
            [Test]
            [TestCase(100, 100, 45, 1.41f)]
            [TestCase(100, 100, 15, 1.22f)]
            [TestCase(100, 200, 45, 2.12f)]
            [TestCase(200, 100, 45, 2.12f)]
            [TestCase(600, 450, 20, 1.39f)]
            [TestCase(600, 450, 45, 1.64f)]
            [TestCase(100, 200, -45, 2.12f)]
            public void RotationZoomIsCalculated(int imageWidth, int imageHeight, float angle, float expected)
            {
                float result = ImageMaths.ZoomAfterRotation(imageWidth, imageHeight, angle);

                result.Should()
                    .BeApproximately(
                        expected,
                        0.01f,
                        "because the zoom level after rotation should have been calculated");

                result.Should()
                    .BePositive("because we're always zooming in so the zoom level should always be positive");

                result.Should()
                    .BeGreaterOrEqualTo(1, "because the zoom should always increase the size and not reduce it");
            }
        }

        /// <summary>
        /// The when rotate point.
        /// </summary>
        [TestFixture]
        public class WhenRotatePoint
        {
            /// <summary>
            /// The then should return point with x 2 and y 2 given null center point and point to rotate x and y.
            /// </summary>
            /// <param name="pointToRotateX">
            /// The point to rotate x.
            /// </param>
            /// <param name="pointToRotateY">
            /// The point to rotate y.
            /// </param>
            /// <param name="expectedX">
            /// The expected x.
            /// </param>
            /// <param name="expectedY">
            /// The expected y.
            /// </param>
            [Test]
            [TestCase(0, -25, 25, 0)]
            [TestCase(25, 0, 0, 25)]
            [TestCase(0, 25, -25, 0)]
            [TestCase(-25, 0, 0, -25)]
            public void ThenShouldReturnPointWithX2AndY2GivenNullCenterPointAndPointToRotateXAndY(
                int pointToRotateX,
                int pointToRotateY,
                int expectedX,
                int expectedY)
            {
                // Arrange
                var pointToRotate = new Point(pointToRotateX, pointToRotateY);

                // Act
                var rotatePoint = ImageMaths.RotatePoint(pointToRotate, 90);

                // Assert
                Assert.That(rotatePoint, Is.EqualTo(new Point(expectedX, expectedY)));
            }

            /// <summary>
            /// The then should return point with x 2 and y 2 given center point 25 and negative 25 and point to rotate x and y.
            /// </summary>
            /// <param name="pointToRotateX">
            /// The point to rotate x.
            /// </param>
            /// <param name="pointToRotateY">
            /// The point to rotate y.
            /// </param>
            /// <param name="expectedX">
            /// The expected x.
            /// </param>
            /// <param name="expectedY">
            /// The expected y.
            /// </param>
            [Test]
            [TestCase(25, -30, 30, -25)]
            [TestCase(30, -25, 25, -20)]
            [TestCase(25, -20, 20, -25)]
            [TestCase(20, -25, 25, -30)]
            public void ThenShouldReturnPointWithX2AndY2GivenCenterPoint25AndNegative25AndPointToRotateXAndY(
                int pointToRotateX,
                int pointToRotateY,
                int expectedX,
                int expectedY)
            {
                // Arrange
                var pointToRotate = new Point(pointToRotateX, pointToRotateY);

                // Act
                var rotatePoint = ImageMaths.RotatePoint(pointToRotate, 90, new Point(25, -25));

                // Assert
                Assert.That(rotatePoint, Is.EqualTo(new Point(expectedX, expectedY)));
            }
        }

        /// <summary>
        /// The when get bounding rotated rectangle.
        /// </summary>
        [TestFixture]
        public class WhenGetBoundingRotatedRectangle
        {
            /// <summary>
            /// The then should return rotated rectangle at 00 and 5 by 5 given 5 by 5 and degrees.
            /// </summary>
            /// <param name="degrees">
            /// The degrees.
            /// </param>
            [Test]
            [TestCase(0)]
            [TestCase(90)]
            [TestCase(180)]
            [TestCase(270)]
            [TestCase(360)]
            public void ThenShouldReturnRotatedRectangleAt00And5By5Given5By5AndDegrees(int degrees)
            {
                // Arrange // Act
                var rectangle = ImageMaths.GetBoundingRotatedRectangle(5, 5, degrees);

                // Assert
                Assert.That(rectangle, Is.EqualTo(new Rectangle(0, 0, 5, 5)));
            }

            /// <summary>
            /// The then should return rotated rectangle at 00 and 7 by 7 given 5 by 5 and degrees.
            /// </summary>
            /// <param name="degrees">
            /// The degrees.
            /// </param>
            [Test]
            [TestCase(45)]
            [TestCase(135)]
            [TestCase(225)]
            [TestCase(315)]
            public void ThenShouldReturnRotatedRectangleAt00And7By7Given5By5AndDegrees(int degrees)
            {
                // Arrange // Act
                var rectangle = ImageMaths.GetBoundingRotatedRectangle(5, 5, degrees);

                // Assert
                Assert.That(rectangle, Is.EqualTo(new Rectangle(0, 0, 7, 7)));
            }

            /// <summary>
            /// The then return rotated rectangle at 00 and 2 by 5 given 52 and 90 degrees.
            /// </summary>
            /// <param name="degrees">
            /// The degrees.
            /// </param>
            [Test]
            [TestCase(90)]
            [TestCase(270)]
            public void ThenReturnRotatedRectangleAt00And2By5Given52And90Degrees(int degrees)
            {
                // Arrange // Act
                var rectangle = ImageMaths.GetBoundingRotatedRectangle(5, 2, degrees);

                // Assert
                Assert.That(rectangle, Is.EqualTo(new Rectangle(0, 0, 2, 5)));
            }

            /// <summary>
            /// The then return rotated rectangle at 00 and 5 by 2 given 52 and 180 degrees.
            /// </summary>
            /// <param name="degrees">
            /// The degrees.
            /// </param>
            [Test]
            [TestCase(180)]
            [TestCase(360)]
            public void ThenReturnRotatedRectangleAt00And5By2Given52And180Degrees(int degrees)
            {
                // Arrange // Act
                var rectangle = ImageMaths.GetBoundingRotatedRectangle(5, 2, degrees);

                // Assert
                Assert.That(rectangle, Is.EqualTo(new Rectangle(0, 0, 5, 2)));
            }
        }

        /// <summary>
        /// The when get bounding rectangle.
        /// </summary>
        [TestFixture]
        public class WhenGetBoundingRectangle
        {
            /// <summary>
            /// The then should be at 00 and 5 by 5 given points 5 units apart.
            /// </summary>
            /// <param name="pointX1">
            /// The point x 1.
            /// </param>
            /// <param name="pointY1">
            /// The point y 1.
            /// </param>
            /// <param name="pointX2">
            /// The point x 2.
            /// </param>
            /// <param name="pointY2">
            /// The point y 2.
            /// </param>
            [Test]
            [TestCase(0, 0, 5, 5)]
            [TestCase(5, 5, 10, 10)]
            [TestCase(7, 7, 12, 12)]
            [TestCase(5, 10, 10, 15)]
            [TestCase(200, 200, 205, 205)]
            public void ThenShouldBeAt00And5By5GivenPoints5UnitsApart(
                int pointX1,
                int pointY1,
                int pointX2,
                int pointY2)
            {
                // Arrange // Act
                var rectangle = ImageMaths.GetBoundingRectangle(
                    new Point(pointX1, pointY1),
                    new Point(pointX2, pointY2));

                // Assert
                Assert.That(
                    rectangle,
                    Is.EqualTo(new Rectangle(pointX1, pointY1, pointX2 - pointX1, pointY2 - pointY1)));
            }

            /// <summary>
            /// The when to points.
            /// </summary>
            [TestFixture]
            public class WhenToPoints
            {
                /// <summary>
                /// The then should convert to 4 points given rectangle.
                /// </summary>
                /// <param name="x">
                /// The x.
                /// </param>
                /// <param name="y">
                /// The y.
                /// </param>
                /// <param name="width">
                /// The width.
                /// </param>
                /// <param name="height">
                /// The height.
                /// </param>
                [Test]
                [TestCase(0, 0, 10, 10)]
                [TestCase(5, 5, 10, 10)]
                [TestCase(1000, 1000, 100, 500)]
                [TestCase(1, 1, 1, 1)]
                public void ThenShouldConvertTo4PointsGivenRectangle(int x, int y, int width, int height)
                {
                    // Arrange
                    var rectangle = new Rectangle(x, y, width, height);

                    // Act
                    var points = ImageMaths.ToPoints(rectangle);

                    // Assert
                    var collectionEquivalentConstraint = Is.EquivalentTo(
                        new[]
                            {
                                new Point(x, y), new Point(x + width, y), new Point(x, y + height),
                                new Point(x + width, y + height),
                            });
                    Assert.That(
                        points,
                        collectionEquivalentConstraint);
                }
            }
        }

        /// <summary>
        /// The when clamp.
        /// </summary>
        [TestFixture]
        public class WhenClamp
        {
            /// <summary>
            /// The then should use min value given value less than min.
            /// </summary>
            /// <param name="minValue">
            /// The min value.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            [Test]
            [TestCase(0, -1)]
            [TestCase(0, -5)]
            [TestCase(0, -10)]
            [TestCase(0, -1000)]
            [TestCase(250, -1000)]
            [TestCase(500, 500)]
            public void ThenShouldUseMinValueGivenValueLessThanMin(int minValue, int value)
            {
                // Arrange //Act
                var result = ImageMaths.Clamp(value, minValue, int.MaxValue);

                // Assert
                Assert.That(result, Is.EqualTo(minValue));
            }

            /// <summary>
            /// The then should use max value given value greater than max.
            /// </summary>
            /// <param name="maxValue">
            /// The max value.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            [Test]
            [TestCase(2000, 10000)]
            [TestCase(1000, 20000)]
            [TestCase(5, 10)]
            [TestCase(0, 10)]
            [TestCase(-1000, 2000)]
            [TestCase(-500, 501)]
            [TestCase(-501, -500)]
            public void ThenShouldUseMaxValueGivenValueGreaterThanMax(int maxValue, int value)
            {
                // Arrange //Act
                var result = ImageMaths.Clamp(value, int.MinValue, maxValue);

                // Assert
                Assert.That(result, Is.EqualTo(maxValue));
            }

        }
    }
}