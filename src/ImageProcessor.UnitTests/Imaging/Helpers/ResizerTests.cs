namespace ImageProcessor.UnitTests.Imaging.Helpers
{
    using NUnit.Framework;
    using System.Drawing;
    using FluentAssertions;
    using ImageProcessor.Imaging;

    internal class ResizerTests
    {
        /// <summary>
        /// A Stub class to capture parameters passed to <see cref="Resizer.ResizeComposite"/> and <see cref="Resizer.ResizeLinear"/>
        /// </summary>
        private class StubbedResizer : Resizer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StubbedResizer"/> class.
            /// </summary>
            /// <param name="resizeLayer">
            /// The <see cref="ResizeLayer"/>.
            /// </param>
            public StubbedResizer(ResizeLayer resizeLayer) : base(resizeLayer)
            {
            }

            /// <summary>
            /// Gets an image resized using the composite color space without any gamma correction adjustments.
            /// </summary>
            /// <remarks>
            /// Captures <paramref name="width"/> to <see cref="ResizeWidth"/>, <paramref name="height"/> to 
            /// <see cref="ResizeHeight"/>, and <paramref name="destination"/> to <see cref="ResizeDestination"/>.
            /// </remarks>
            /// <param name="source">The source image.</param>
            /// <param name="width">The width to resize to.</param>
            /// <param name="height">The height to resize to.</param>
            /// <param name="destination">The destination rectangle.</param>
            /// <returns>
            /// The <see cref="Bitmap"/>.
            /// </returns>
            protected override Bitmap ResizeComposite(Image source, int width, int height, Rectangle destination)
            {
                this.ResizeWidth = width;
                this.ResizeHeight = height;
                this.ResizeDestination = destination;

                return base.ResizeComposite(source, width, height, destination);
            }

            /// <summary>
            /// Gets an image resized using the linear color space with gamma correction adjustments.
            /// </summary>
            /// <remarks>
            /// Captures <paramref name="width"/> to <see cref="ResizeWidth"/>, <paramref name="height"/> to 
            /// <see cref="ResizeHeight"/>, and <paramref name="destination"/> to <see cref="ResizeDestination"/>.
            /// </remarks>
            /// <param name="source">The source image.</param>
            /// <param name="width">The width to resize to.</param>
            /// <param name="height">The height to resize to.</param>
            /// <param name="destination">The destination rectangle.</param>
            /// <returns>
            /// The <see cref="Bitmap"/>.
            /// </returns>
            protected override Bitmap ResizeLinear(Image source, int width, int height, Rectangle destination)
            {
                this.ResizeWidth = width;
                this.ResizeHeight = height;
                this.ResizeDestination = destination;

                return base.ResizeLinear(source, width, height, destination);
            }

            /// <summary>
            /// The <see cref="Rectangle"/> used to position the resized image
            /// </summary>
            public Rectangle ResizeDestination { get; private set; }

            /// <summary>
            /// The height of the resized image
            /// </summary>
            private int? ResizeHeight { get; set; }

            /// <summary>
            /// The width of the resized image
            /// </summary>
            private int? ResizeWidth { get; set; }
        }

        /// <summary>
        /// A class to test what happens when using the resizer to resize images
        /// </summary>
        [TestFixture]
        class WhenResizing
        {
            /// <summary>
            /// Test resizing when usinging <see cref="ResizeMode.Pad"/>
            /// </summary>
            /// <param name="width">The width of the original image</param>
            /// <param name="height">The height of the original image</param>
            /// <param name="destinationLeft">The left position where the resized image is placed</param>
            /// <param name="destinationTop">The top position where the resized image is placed</param>
            [Test]
            [TestCase(50, 75, 17, 0, Description = "Upscale")]
            [TestCase(50, 150, 33, 0, Description = "Downscaling height dimension")]
            [TestCase(150, 150, 0, 0, Description = "Downscaling both dimensions")]
            public void CorrectlyPlacesImageWhenPadding(
                int width,
                int height,
                int destinationLeft,
                int destinationTop)

            {
                const int NewWidth = 100;
                const int NewHeight = 100;
                StubbedResizer resizer = new StubbedResizer(new ResizeLayer(new Size(NewWidth, NewHeight), ResizeMode.Pad));
                resizer
                    .ResizeImage(new Bitmap(width, height), false);

                resizer.ResizeDestination.Top.Should().Be(destinationTop);
                resizer.ResizeDestination.Left.Should().Be(destinationLeft);
            }

            /// <summary>
            /// Test resizing when usinging <see cref="ResizeMode.BoxPad"/>
            /// </summary>
            /// <param name="width">The width of the original image</param>
            /// <param name="height">The height of the original image</param>
            /// <param name="destinationLeft">The left position where the resized image is placed</param>
            /// <param name="destinationTop">The top position where the resized image is placed</param>
            [Test]
            [TestCase(50, 75, 25, 12, Description = "Upscale")]
            [TestCase(50, 150, 33, 0, Description = "Downscaling height dimension")]
            [TestCase(150, 150, 0, 0, Description = "Downscaling both dimensions")]
            public void CorrectlyPlacesImageWhenBoxPadding(
                int width,
                int height,
                int destinationLeft,
                int destinationTop)

            {
                const int NewWidth = 100;
                const int NewHeight = 100;
                StubbedResizer resizer = new StubbedResizer(new ResizeLayer(new Size(NewWidth, NewHeight), ResizeMode.BoxPad));
                resizer.ResizeImage(new Bitmap(width, height), false);

                resizer.ResizeDestination.Top.Should().Be(destinationTop);
                resizer.ResizeDestination.Left.Should().Be(destinationLeft);
            }
        }
    }

}