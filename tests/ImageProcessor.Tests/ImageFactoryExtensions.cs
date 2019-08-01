using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageProcessor.Formats;

namespace ImageProcessor.Tests
{
    public static class ImageFactoryExtensions
    {
        public static ImageFactory SaveAndCompare(
            this ImageFactory factory,
            TestFile testFile,
            string category,
            params object[] additionalData)
        {
            string filename = Path.GetFileNameWithoutExtension(testFile.Name);

            BitDepth bitDepth = default;
            bool hasBitDepth = false;
            if (additionalData.Length > 0)
            {
                if (additionalData[0] is BitDepth depth)
                {
                    hasBitDepth = true;
                    bitDepth = depth;
                }

                filename += "_" + string.Join("_", additionalData);
            }

            filename += testFile.Extension;

            string expectedPath = Path.GetFullPath(Path.Combine(testFile.ExpectedRoot, category, filename));
            string actualPath = Path.GetFullPath(Path.Combine(testFile.ActualRoot, category, filename));

            // TODO: Remove expected saving once we have our images.
            if (hasBitDepth)
            {
                factory.Save(expectedPath, bitDepth);
                factory.Save(actualPath, bitDepth);
            }
            else
            {
                factory.Save(expectedPath);
                factory.Save(actualPath);
            }

            Bitmap expectedClone = null;
            Bitmap actualClone = null;
            FastBitmap expectedFast = null;
            FastBitmap actualFast = null;
            try
            {
                using (var expectedFactory = new ImageFactory())
                using (var actualFactory = new ImageFactory())
                {
                    expectedFactory.Load(expectedPath);
                    actualFactory.Load(actualPath);

                    Image expectedImage = expectedFactory.Image;
                    Image actualImage = actualFactory.Image;

                    if (expectedImage.Size != actualImage.Size)
                    {
                        throw new ImagesSimilarityException("Images are not the same size!");
                    }

                    // Copy and dispose of originals to allow fast comparison.
                    // TODO: We only compare the first frame. Consider comparing each one.
                    expectedClone = FormatUtilities.DeepCloneImageFrame(expectedImage, PixelFormat.Format32bppArgb);
                    actualClone = FormatUtilities.DeepCloneImageFrame(actualImage, PixelFormat.Format32bppArgb);
                }

                expectedFast = new FastBitmap(expectedClone);
                actualFast = new FastBitmap(actualClone);

                for (int y = 0; y < expectedFast.Height; y++)
                {
                    for (int x = 0; x < expectedFast.Width; x++)
                    {
                        Color expected = expectedFast.GetPixel(x, y);
                        Color actual = actualFast.GetPixel(x, y);

                        if (expected.ToArgb() != actual.ToArgb())
                        {
                            throw new ImagesSimilarityException($"Images at {x}, {y} are different. {expected} : {actual}!");
                        }
                    }
                }
            }
            finally
            {
                expectedFast?.Dispose();
                actualFast?.Dispose();
                expectedClone?.Dispose();
                actualClone?.Dispose();
            }

            return factory;
        }
    }
}
