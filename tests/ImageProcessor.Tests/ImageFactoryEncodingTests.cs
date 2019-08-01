using System.Collections.Generic;
using ImageProcessor.Configuration;
using ImageProcessor.Formats;
using Xunit;

namespace ImageProcessor.Tests
{
    public class ImageFactoryEncodingTests
    {
        public ImageFactoryEncodingTests()
        {
            ImageProcessorBootstrapper.Instance.AddImageFormats(new WebPFormat());
        }

        private const string Category = "Encoding";

        public static IEnumerable<object[]> BitDepths = new[]
        {
            new object[]{ BitDepth.Bit1 },
            new object[]{ BitDepth.Bit4 },
            new object[]{ BitDepth.Bit8 },
            new object[]{ BitDepth.Bit16 },
            new object[]{ BitDepth.Bit24 },
            new object[]{ BitDepth.Bit32 }
        };

        [Theory]
        [MemberData(nameof(BitDepths))]
        public void CanEncodeJpegAllBitDepths(BitDepth bitDepth)
        {
            using (var factory = new ImageFactory())
            {
                TestFile file = TestFiles.Jpeg.Penguins;

                factory.Load(file.FullName).SaveAndCompare(file, Category, bitDepth);
            }
        }

        [Theory]
        [MemberData(nameof(BitDepths))]
        public void CanEncodePngAllBitDepths(BitDepth bitDepth)
        {
            using (var factory = new ImageFactory())
            {
                TestFile file = TestFiles.Png.Penguins;

                factory.Load(file.FullName).SaveAndCompare(file, Category, bitDepth);
            }
        }

        [Theory]
        [MemberData(nameof(BitDepths))]
        public void CanEncodeBmpAllBitDepths(BitDepth bitDepth)
        {
            using (var factory = new ImageFactory())
            {
                TestFile file = TestFiles.Bmp.Penguins;

                factory.Load(file.FullName).SaveAndCompare(file, Category, bitDepth);
            }
        }

        [Theory]
        [MemberData(nameof(BitDepths))]
        public void CanEncodeGifAllBitDepths(BitDepth bitDepth)
        {
            using (var factory = new ImageFactory())
            {
                TestFile file = TestFiles.Gif.Penguins;

                factory.Load(file.FullName).SaveAndCompare(file, Category, bitDepth);
            }
        }

        [Theory]
        [MemberData(nameof(BitDepths))]
        public void CanEncodeTiffAllBitDepths(BitDepth bitDepth)
        {
            using (var factory = new ImageFactory())
            {
                TestFile file = TestFiles.Tiff.Penguins;

                factory.Load(file.FullName).SaveAndCompare(file, Category, bitDepth);
            }
        }

        [Theory]
        [MemberData(nameof(BitDepths))]
        public void CanEncodeWebPAllBitDepths(BitDepth bitDepth)
        {
            using (var factory = new ImageFactory())
            {
                TestFile file = TestFiles.WebP.Penguins;

                factory.Load(file.FullName).SaveAndCompare(file, Category, bitDepth);
            }
        }
    }
}
