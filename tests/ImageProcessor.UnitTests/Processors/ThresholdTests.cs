namespace ImageProcessor.UnitTests.Processors
{
    using System.Drawing;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Imaging;

    using NUnit.Framework;

    [TestFixture]
    public class ThresholdTests
    {
        [Test]
        public void ThresholdPenguins()
        {
            var file = ImageSources.GetFilePathByName("format-Penguins.jpg");

            using (var bmp = new Bitmap(file))
            {
                using (var factory = new ImageFactory())
                {
                    factory.Load(bmp);

                    factory.Threshold(160);

                    using (var fast = new FastBitmap(factory.Image))
                    {

                        var top_left_pixel_color = fast.GetPixel(0, 0);
                        var bottom_left_pixel_color = fast.GetPixel(0, fast.Height - 1);

                        Assert.AreEqual(Color.FromArgb(255, 255, 255, 255), top_left_pixel_color);
                        Assert.AreEqual(Color.FromArgb(255, 0, 0, 0), bottom_left_pixel_color);
                    }

                    // These should not throw, they are within the acceptable range of values
                    factory.Threshold(1);
                    factory.Threshold(255);
                }
            }
        }

        [Test]
        public void InvalidThresholdParameter0()
        {
            Assert.Throws<ImageProcessingException>(() => InvalidThresh(0));
        }

        [Test]
        public void InvalidThresholdParameter256()
        {
            Assert.Throws<ImageProcessingException>(() => InvalidThresh(256));
        }

        [Test]
        public void InvalidThresholdParameterm1()
        {
            Assert.Throws<ImageProcessingException>(() => InvalidThresh(-1));
        }

        [Test]
        public void InvalidThresholdParameterMax()
        {
            Assert.Throws<ImageProcessingException>(() => InvalidThresh(int.MaxValue));
        }

        [Test]
        public void InvalidThresholdParameterMin()
        {
            Assert.Throws<ImageProcessingException>(() => InvalidThresh(int.MinValue));
        }

        private void InvalidThresh(int t)
        {
            var file = ImageSources.GetFilePathByName("format-Penguins.jpg");

            using (var bmp = new Bitmap(file))
            {
                using (var factory = new ImageFactory())
                {
                    factory.Load(bmp);

                    factory.Threshold(t);
                }
            }
        }
    }
}
