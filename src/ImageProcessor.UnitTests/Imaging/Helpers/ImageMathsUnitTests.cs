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
    using System.Drawing;
    using FluentAssertions;
    using FluentAssertions.Equivalency;

    using ImageProcessor.Imaging.Helpers;
    using NUnit.Framework;

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
            public void BoundingRotatedRectangleIsCalculated(int width, int height, float angle, int expectedWidth, int expectedHeight)
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

                result.Should().BeApproximately(expected, 0.01f, "because the zoom level after rotation should have been calculated");

                result.Should().BePositive("because we're always zooming in so the zoom level should always be positive");

                result.Should().BeGreaterOrEqualTo(1, "because the zoom should always increase the size and not reduce it");
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
            public void ThenShouldReturnPointWithX2AndY2GivenNullCenterPointAndPointToRotateXAndY(int pointToRotateX, int pointToRotateY, int expectedX, int expectedY)
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
            public void ThenShouldReturnPointWithX2AndY2GivenCenterPoint25AndNegative25AndPointToRotateXAndY(int pointToRotateX, int pointToRotateY, int expectedX, int expectedY)
            {
                // Arrange
                var pointToRotate = new Point(pointToRotateX, pointToRotateY);

                // Act
                var rotatePoint = ImageMaths.RotatePoint(pointToRotate, 90, new Point(25, -25));

                // Assert
                Assert.That(rotatePoint, Is.EqualTo(new Point(expectedX, expectedY)));
            }
        }
    }
}