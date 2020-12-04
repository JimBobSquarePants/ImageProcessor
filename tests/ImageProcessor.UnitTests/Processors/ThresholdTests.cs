namespace ImageProcessor.UnitTests.Processors
{
    using System.Drawing;

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
                }
            }
        }
    }
}
