using System;
using System.Drawing;
using System.Drawing.Imaging;
using FluentAssertions;
using ImageProcessor.Imaging.Filters.Photo;
using NUnit.Framework;

namespace ImageProcessor.UnitTests.Imaging.Filters.Photo
{
    [TestFixture]
    public class MatrixFilterBaseTests
    {
        internal class VariantFilterBase : MatrixFilterBase
        {
            public override ColorMatrix Matrix
            {
                get { return new ColorMatrix(); }
            }

            public override Bitmap TransformImage(Image image, Image newImage)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public static void MatrixFilterBaseImplementsEqualsBasedOnMatrixPropertyVariant()
        {
            VariantFilterBase first = new VariantFilterBase();
            VariantFilterBase second = new VariantFilterBase();

            first.Equals(second).Should().BeTrue();
        }

        internal static ColorMatrix InvariantColorMatrix = new ColorMatrix(new[]
        {
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, 0, 0}
        });


        internal class InvariantFilterBase : MatrixFilterBase
        {
            public override ColorMatrix Matrix
            {
                get
                {
                    return InvariantColorMatrix;
                }
            }

            public override Bitmap TransformImage(Image image, Image newImage)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public static void MatrixFilterBaseImplementsEqualsBasedOnMatrixPropertyInvariant()
        {
            InvariantFilterBase first = new InvariantFilterBase();
            InvariantFilterBase second = new InvariantFilterBase();

            first.Equals(second).Should().BeTrue();
        }
    }
}
