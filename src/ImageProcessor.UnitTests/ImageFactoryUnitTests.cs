// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageFactoryUnitTests.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ImageProcessor.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    using FluentAssertions;

    using ImageProcessor.Imaging;
    using ImageProcessor.Imaging.Filters.EdgeDetection;
    using ImageProcessor.Imaging.Filters.Photo;
    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Imaging.MetaData;

    using NUnit.Framework;

    /// <summary>
    /// Test harness for the image factory
    /// </summary>
    [TestFixture]
    public class ImageFactoryUnitTests
    {
        /// <summary>
        /// The list of images. Designed to speed up the tests a little.
        /// </summary>
        private IEnumerable<FileInfo> imagesInfos;

        /// <summary>
        /// The list of ImageFactories. Designed to speed up the test a bit more.
        /// </summary>
        private List<ImageFactory> imagesFactories;

        /// <summary>
        /// Tests the loading of image from a file
        /// </summary>
        [Test]
        public void ImageIsLoadedFromFile()
        {
            foreach (FileInfo file in this.ListInputFiles())
            {
                using (ImageFactory imageFactory = new ImageFactory())
                {
                    imageFactory.Load(file.FullName);

                    imageFactory.ImagePath.Should().Be(file.FullName, "because the path should have been memorized");
                    imageFactory.Image.Should().NotBeNull("because the image should have been loaded");
                }
            }
        }

        /// <summary>
        /// Tests the loading of image from a memory stream
        /// </summary>
        [Test]
        public void ImageIsLoadedFromMemoryStream()
        {
            foreach (FileInfo file in this.ListInputFiles())
            {
                byte[] photoBytes = File.ReadAllBytes(file.FullName);

                using (MemoryStream inStream = new MemoryStream(photoBytes))
                {
                    using (ImageFactory imageFactory = new ImageFactory())
                    {
                        imageFactory.Load(inStream);

                        imageFactory.ImagePath.Should().BeNull("because an image loaded from stream should not have a file path");
                        imageFactory.Image.Should().NotBeNull("because the image should have been loaded");
                    }
                }
            }
        }

        [Test]
        public void ImageIsLoadedFromByteArray()
        {
            foreach (FileInfo file in this.ListInputFiles())
            {
                byte[] photoBytes = File.ReadAllBytes(file.FullName);

                using (ImageFactory imageFactory = new ImageFactory())
                {
                    imageFactory.Load(photoBytes);

                    imageFactory.ImagePath.Should().BeNull("because an image loaded from byte array should not have a file path");
                    imageFactory.Image.Should().NotBeNull("because the image should have been loaded");
                }
            }
        }

        /// <summary>
        /// Tests that the save method actually saves a file
        /// </summary>
        [Test]
        public void ImageIsSavedToDisk()
        {
            foreach (FileInfo file in this.ListInputFiles())
            {
                string outputFileName = string.Format("./output/{0}", file.Name);
                using (ImageFactory imageFactory = new ImageFactory())
                {
                    imageFactory.Load(file.FullName);
                    imageFactory.Save(outputFileName);

                    File.Exists(outputFileName).Should().BeTrue("because the file should have been saved on disk");

                    File.Delete(outputFileName);
                }
            }
        }

        /// <summary>
        /// Tests that the save method actually writes to memory
        /// </summary>
        [Test]
        public void ImageIsSavedToMemory()
        {
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                using (MemoryStream s = new MemoryStream())
                {
                    imageFactory.Save(s);
                    s.Seek(0, SeekOrigin.Begin);

                    s.Capacity.Should().BeGreaterThan(0, "because the stream should contain the image");
                }
            }
        }

        /// <summary>
        /// Tests that a filter is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void AlphaIsModified()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Alpha(50);

                if (imageFactory.CurrentImageFormat.GetType() == typeof(BitmapFormat))
                {
                    AssertionHelpers.AssertImagesAreIdentical(
                        original,
                        imageFactory.Image,
                        "because the alpha operation should not have been applied on {0}",
                        imageFactory.ImagePath);
                }
                else
                {
                    AssertionHelpers.AssertImagesAreDifferent(
                        original,
                        imageFactory.Image,
                        "because the alpha operation should have been applied on {0}",
                        imageFactory.ImagePath);
                }

                imageFactory.Format(new JpegFormat()).Save("./output/alpha-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that brightness changes is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void BrightnessIsModified()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Brightness(50);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the brightness operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/brightness-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that background color changes are really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void BackgroundColorIsChanged()
        {
            ImageFactory imageFactory = new ImageFactory();
            imageFactory.Load(@"Images\text.png");
            Image original = (Image)imageFactory.Image.Clone();
            imageFactory.BackgroundColor(Color.Yellow);
            AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the background color operation should have been applied on {0}", imageFactory.ImagePath);
        }

        /// <summary>
        /// Tests that a contrast change is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void ContrastIsModified()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Contrast(50);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the contrast operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/contrast-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that a saturation change is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void SaturationIsModified()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Saturation(50);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the saturation operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/saturation-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that a tint change is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void TintIsModified()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Tint(Color.FromKnownColor(KnownColor.AliceBlue));
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the tint operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/tint-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that a vignette change is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void VignetteEffectIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Vignette(Color.FromKnownColor(KnownColor.AliceBlue));
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the vignette operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/vignette-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that a filter is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void WatermarkIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Watermark(new TextLayer
                {
                    FontFamily = new FontFamily("Arial"),
                    FontSize = 10,
                    Position = new Point(10, 10),
                    Text = "Lorem ipsum dolor"
                });
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the watermark operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/watermark-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that a filter is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void BlurEffectIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.GaussianBlur(5);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the blur operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/blur-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that a filter is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void BlurWithLayerIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.GaussianBlur(new GaussianLayer { Sigma = 10, Size = 5, Threshold = 2 });
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the layered blur operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/blurlayer-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that a filter is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void SharpenEffectIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.GaussianSharpen(5);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the sharpen operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/sharpen-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that a filter is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void SharpenWithLayerIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.GaussianSharpen(new GaussianLayer { Sigma = 10, Size = 5, Threshold = 2 });
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the layered sharpen operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/sharpenlayer-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that all filters can be applied
        /// </summary>
        [Test]
        public void FilterIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();

                List<IMatrixFilter> filters = new List<IMatrixFilter>
                {
                    MatrixFilters.BlackWhite,
                    MatrixFilters.Comic,
                    MatrixFilters.Gotham,
                    MatrixFilters.GreyScale,
                    MatrixFilters.HiSatch,
                    MatrixFilters.Invert,
                    MatrixFilters.Lomograph,
                    MatrixFilters.LoSatch,
                    MatrixFilters.Polaroid,
                    MatrixFilters.Sepia
                };

                int j = 0;
                foreach (IMatrixFilter filter in filters)
                {
                    imageFactory.Filter(filter);
                    AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the filter operation should have been applied on {0}", imageFactory.ImagePath);
                    imageFactory.Reset();
                    AssertionHelpers.AssertImagesAreIdentical(original, imageFactory.Image, "because the image should be reset");

                    imageFactory.Format(new JpegFormat()).Save("./output/filter-" + j++ + "-image-" + i + ".jpg");
                }

                i++;
            }
        }

        /// <summary>
        /// Tests that a filter is really applied by checking that the image is modified
        /// </summary>
        [Test]
        public void RoundedCornersAreApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.RoundedCorners(new RoundedCornerLayer(5));
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the rounded corners operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/roundedcorners-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image is well resized using constraints
        /// </summary>
        [Test]
        public void ImageIsResizedWithinConstraints()
        {
            int i = 0;
            const int MaxSize = 200;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                imageFactory.Constrain(new Size(MaxSize, MaxSize));
                imageFactory.Image.Width.Should().BeLessOrEqualTo(MaxSize, "because the image size should have been reduced");
                imageFactory.Image.Height.Should().BeLessOrEqualTo(MaxSize, "because the image size should have been reduced");

                imageFactory.Format(new JpegFormat()).Save("./output/resizedcontraints-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image is well cropped
        /// </summary>
        [Test]
        public void ImageIsCropped()
        {
            int i = 0;
            const int MaxSize = 20;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Crop(new Rectangle(0, 0, MaxSize, MaxSize));
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the crop operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Image.Width.Should().Be(MaxSize, "because the cropped image should be {0}x{0}", MaxSize);
                imageFactory.Image.Height.Should().Be(MaxSize, "because the cropped image should be {0}x{0}", MaxSize);

                imageFactory.Format(new JpegFormat()).Save("./output/crop-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image is well cropped
        /// </summary>
        [Test]
        public void ImageIsCroppedWithLayer()
        {
            int i = 0;
            const int MaxSize = 20;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Crop(new CropLayer(0, 0, MaxSize, MaxSize, CropMode.Pixels));
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the layered crop operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Image.Width.Should().Be(MaxSize, "because the cropped image should be {0}x{0}", MaxSize);
                imageFactory.Image.Height.Should().Be(MaxSize, "because the cropped image should be {0}x{0}", MaxSize);

                imageFactory.Format(new JpegFormat()).Save("./output/croplayer-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image is flipped
        /// </summary>
        [Test]
        public void ImageIsFlipped()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Flip(true);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the vertical flip operation should have been applied on {0}", imageFactory.ImagePath);
                imageFactory.Image.Width.Should().Be(original.Width, "because the dimensions should not have changed");
                imageFactory.Image.Height.Should().Be(original.Height, "because the dimensions should not have changed");
                imageFactory.Reset();
                AssertionHelpers.AssertImagesAreIdentical(original, imageFactory.Image, "because the image should be reset");

                imageFactory.Flip();
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the horizontal flip operation should have been applied on {0}", imageFactory.ImagePath);
                imageFactory.Image.Width.Should().Be(original.Width, "because the dimensions should not have changed");
                imageFactory.Image.Height.Should().Be(original.Height, "because the dimensions should not have changed");

                imageFactory.Format(new JpegFormat()).Save("./output/flip-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image is resized
        /// </summary>
        [Test]
        public void ImageIsResized()
        {
            int i = 0;
            const int NewSize = 150;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                imageFactory.Resize(new Size(NewSize, NewSize));

                imageFactory.Image.Width.Should().Be(NewSize, "because the new image's size should be {0}x{0}", NewSize);
                imageFactory.Image.Height.Should().Be(NewSize, "because the new image's size should be {0}x{0}", NewSize);

                imageFactory.Format(new JpegFormat()).Save("./output/resized-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image is resized
        /// </summary>
        [Test]
        public void ImageIsResizedWithLayer()
        {
            int i = 0;
            const int NewSize = 150;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                imageFactory.Resize(new ResizeLayer(new Size(NewSize, NewSize), ResizeMode.Stretch, AnchorPosition.Left));

                imageFactory.Image.Width.Should().Be(NewSize, "because the new image's size should be {0}x{0}", NewSize);
                imageFactory.Image.Height.Should().Be(NewSize, "because the new image's size should be {0}x{0}", NewSize);

                imageFactory.Format(new JpegFormat()).Save("./output/resizedlayer-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image is resized
        /// </summary>
        [Test]
        public void ImageIsRotated()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Rotate(90);

                imageFactory.Image.Width.Should().Be(original.Height, "because the rotated image dimensions should have been switched");
                imageFactory.Image.Height.Should().Be(original.Width, "because the rotated image dimensions should have been switched");

                imageFactory.Format(new JpegFormat()).Save("./output/rotated-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image's inside is rotated
        /// </summary>
        [Test]
        public void ImageIsRotatedInside()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.RotateBounded(45, true);

                imageFactory.Image.Width.Should().Be(original.Width, "because the rotated image dimensions should not have changed");
                imageFactory.Image.Height.Should().Be(original.Height, "because the rotated image dimensions should not have changed");

                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the inside image should have been rotated on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/rotatebounded-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image's inside is rotated counter-clockwise
        /// </summary>
        [Test]
        public void ImageIsRotatedInsideCounterClockwise()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.RotateBounded(-45, true);

                imageFactory.Image.Width.Should().Be(original.Width, "because the rotated image dimensions should not have changed");
                imageFactory.Image.Height.Should().Be(original.Height, "because the rotated image dimensions should not have changed");

                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the inside image should have been rotated on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/rotateboundedccw-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image's inside is rotated and resized
        /// </summary>
        [Test]
        public void ImageIsRotatedInsideAndResized()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.RotateBounded(45);

                imageFactory.Image.Width.Should().NotBe(original.Width, "because the rotated image dimensions should have changed");
                imageFactory.Image.Height.Should().NotBe(original.Height, "because the rotated image dimensions should have changed");

                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the inside image should have been rotated on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/rotateboundedresized-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the images hue has been altered.
        /// </summary>
        [Test]
        public void HueIsModified()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Hue(90);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the hue operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Reset();
                AssertionHelpers.AssertImagesAreIdentical(original, imageFactory.Image, "because the image should be reset");

                imageFactory.Hue(116, true);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the hue+rotate operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/hue-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image has had its resolution changed.
        /// </summary>
        [Test]
        public void ResolutionIsApplied()
        {
            int i = 0;
            byte[] bytes = new ExifBitConverter(new ComputerArchitectureInfo()).IsLittleEndian()
                ? new byte[] { 144, 1, 0, 0, 1, 0, 0, 0 }
                : new byte[] { 0, 0, 0, 1, 0, 0, 1, 144 };

            int horizontalKey = (int)ExifPropertyTag.XResolution;
            int verticalKey = (int)ExifPropertyTag.YResolution;

            foreach (ImageFactory imageFactory in this.ListInputImagesWithMetadata())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Resolution(400, 400);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the resolution operation should have been applied on {0}", imageFactory.ImagePath);

                Assert.AreEqual(400, imageFactory.Image.HorizontalResolution);
                Assert.AreEqual(400, imageFactory.Image.VerticalResolution);

                if (imageFactory.PreserveExifData && imageFactory.ExifPropertyItems.Any())
                {
                    if (imageFactory.ExifPropertyItems.ContainsKey(horizontalKey)
                        && imageFactory.ExifPropertyItems.ContainsKey(verticalKey))
                    {
                        PropertyItem horizontal = imageFactory.ExifPropertyItems[horizontalKey];
                        PropertyItem vertical = imageFactory.ExifPropertyItems[verticalKey];
                        Assert.AreEqual(bytes, horizontal.Value);
                        Assert.AreEqual(bytes, vertical.Value);
                    }
                }

                imageFactory.Format(new JpegFormat()).Save("./output/resolution-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the image has been pixelated.
        /// </summary>
        [Test]
        public void PixelateEffectIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.Pixelate(8);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the pixelate operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/pixelate-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the images quality has been set.
        /// </summary>
        [Test]
        public void ImageQualityIsModified()
        {
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                int original = imageFactory.CurrentImageFormat.Quality;
                imageFactory.Quality(69);
                int updated = imageFactory.CurrentImageFormat.Quality;

                updated.Should().NotBe(original, "because the quality should have been changed");
            }
        }

        /// <summary>
        /// Tests that the image has had a color replaced.
        /// </summary>
        [Test]
        public void ColorIsReplaced()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();
                imageFactory.ReplaceColor(Color.White, Color.Black, 90);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the color replace operation should have been applied on {0}", imageFactory.ImagePath);

                imageFactory.Format(new JpegFormat()).Save("./output/colorreplace-" + i++ + ".jpg");
            }
        }

        /// <summary>
        /// Tests that the various edge detection algorithms are applied.
        /// </summary>
        [Test]
        public void EdgeDetectionEffectIsApplied()
        {
            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();

                List<IEdgeFilter> filters = new List<IEdgeFilter>
                {
                    new KayyaliEdgeFilter(),
                    new KirschEdgeFilter(),
                    new Laplacian3X3EdgeFilter(),
                    new Laplacian5X5EdgeFilter(),
                    new LaplacianOfGaussianEdgeFilter(),
                    new PrewittEdgeFilter(),
                    new RobertsCrossEdgeFilter(),
                    new ScharrEdgeFilter(),
                    new SobelEdgeFilter()
                };

                int j = 0;
                foreach (IEdgeFilter filter in filters)
                {
                    imageFactory.DetectEdges(filter);
                    AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the edge operation should have been applied on {0}", imageFactory.ImagePath);
                    imageFactory.Reset();
                    AssertionHelpers.AssertImagesAreIdentical(original, imageFactory.Image, "because the image should be reset");

                    imageFactory.Format(new JpegFormat()).Save("./output/edgefilter-" + j++ + "-image-" + i + ".jpg");
                }

                i++;
            }
        }

        /// <summary>
        /// Tests that the resize is applied.
        /// </summary>
        [Test]
        public void ResizeIsApplied()
        {
            Size stretchedSize = new Size(400, 400);
            ResizeLayer stretchLayer = new ResizeLayer(stretchedSize, ResizeMode.Stretch);

            Size paddedSize = new Size(700, 700);
            // ReSharper disable once RedundantArgumentDefaultValue
            ResizeLayer paddedLayer = new ResizeLayer(paddedSize, ResizeMode.Pad);

            Size cropSize = new Size(600, 450);
            ResizeLayer cropLayer = new ResizeLayer(cropSize, ResizeMode.Crop);

            Size minSize = new Size(300, 300);
            ResizeLayer minLayer = new ResizeLayer(minSize, ResizeMode.Min);

            Size padSingleDimensionWidthSize = new Size(400, 0);
            // ReSharper disable once RedundantArgumentDefaultValue
            ResizeLayer paddedSingleDimensionWidthLayer = new ResizeLayer(padSingleDimensionWidthSize, ResizeMode.Pad);

            Size padSingleDimensionHeightSize = new Size(0, 300);
            // ReSharper disable once RedundantArgumentDefaultValue
            ResizeLayer paddedSingleDimensionHeightLayer = new ResizeLayer(padSingleDimensionHeightSize, ResizeMode.Pad);

            Size boxPadSingleDimensionWidthSize = new Size(400, 0);
            ResizeLayer boxPadSingleDimensionWidthLayer = new ResizeLayer(boxPadSingleDimensionWidthSize, ResizeMode.BoxPad);

            Size boxPadSingleDimensionHeightSize = new Size(0, 300);
            ResizeLayer boxPadSingleDimensionHeightLayer = new ResizeLayer(boxPadSingleDimensionHeightSize, ResizeMode.BoxPad);

            int i = 0;
            foreach (ImageFactory imageFactory in this.ListInputImages())
            {
                Image original = (Image)imageFactory.Image.Clone();

                // First stretch
                imageFactory.Format(new JpegFormat()).Resize(stretchLayer);
                AssertionHelpers.AssertImagesAreDifferent(original, imageFactory.Image, "because the resize operation should have been applied on {0}", imageFactory.ImagePath);

                Assert.AreEqual(imageFactory.Image.Size, stretchedSize);
                imageFactory.Save("./output/resize-stretch-" + i + ".jpg");

                // Check we padd correctly.
                imageFactory.Resize(paddedLayer);
                Assert.AreEqual(imageFactory.Image.Size, paddedSize);
                imageFactory.Save("./output/resize-padd-" + i + ".jpg");

                // Check we crop correctly.
                imageFactory.Resize(cropLayer);
                Assert.AreEqual(imageFactory.Image.Size, cropSize);
                imageFactory.Save("./output/resize-crop-" + i + ".jpg");

                // Check we min correctly using the shortest size.
                imageFactory.Resize(minLayer);
                Assert.AreEqual(imageFactory.Image.Size, new Size(400, 300));
                imageFactory.Save("./output/resize-crop-" + i + ".jpg");

                // Check padding with only a single dimension specified (width)
                imageFactory.Resize(paddedSingleDimensionWidthLayer);
                Assert.AreEqual(imageFactory.Image.Size, new Size(400, 300));
                imageFactory.Save("./output/resize-padsingledimension-width-" + i + ".jpg");

                // Check padding with only a single dimension specified (height)
                imageFactory.Resize(paddedSingleDimensionHeightLayer);
                Assert.AreEqual(imageFactory.Image.Size, new Size(400, 300));
                imageFactory.Save("./output/resize-padsingledimension-height-" + i + ".jpg");

                // Check box padding with only a single dimension specified (width)
                imageFactory.Resize(boxPadSingleDimensionWidthLayer);
                Assert.AreEqual(imageFactory.Image.Size, new Size(400, 300));
                imageFactory.Save("./output/resize-boxpadsingledimension-width-" + i + ".jpg");

                // Check box padding with only a single dimension specified (height)
                imageFactory.Resize(boxPadSingleDimensionHeightLayer);
                Assert.AreEqual(imageFactory.Image.Size, new Size(400, 300));
                imageFactory.Save("./output/resize-boxpadsingledimension-height-" + i + ".jpg");

                imageFactory.Reset();
                AssertionHelpers.AssertImagesAreIdentical(original, imageFactory.Image, "because the image should be reset");

                imageFactory.Format(new JpegFormat()).Save("./output/resize-" + i + ".jpg");
            }
        }

        /// <summary>
        /// Gets the files matching the given extensions.
        /// </summary>
        /// <param name="dir">
        /// The <see cref="System.IO.DirectoryInfo"/>.
        /// </param>
        /// <param name="extensions">
        /// The extensions.
        /// </param>
        /// <returns>
        /// A collection of <see cref="System.IO.FileInfo"/>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// The extensions variable is null.
        /// </exception>
        private static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions");
            }

            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Lists the input files in the Images folder
        /// </summary>
        /// <returns>The list of files.</returns>
        private IEnumerable<FileInfo> ListInputFiles()
        {
            if (this.imagesInfos != null)
            {
                return this.imagesInfos;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo("./Images");

            this.imagesInfos = GetFilesByExtensions(directoryInfo, ".jpg", ".jpeg", ".png", ".gif", ".tiff", ".bmp", ".webp");

            return this.imagesInfos;
        }

        /// <summary>
        /// Lists the input images to use from the Images folder
        /// </summary>
        /// <returns>The list of images</returns>
        private IEnumerable<ImageFactory> ListInputImages()
        {
            if (this.imagesFactories == null || !this.imagesFactories.Any())
            {
                this.imagesFactories = new List<ImageFactory>();
                foreach (FileInfo fi in this.ListInputFiles())
                {
                    this.imagesFactories.Add((new ImageFactory()).Load(fi.FullName));
                }
            }

            // reset all the images whenever we call this
            foreach (ImageFactory image in this.imagesFactories)
            {
                image.Reset();
            }

            return this.imagesFactories;
        }

        /// <summary>
        /// Lists the input images to use from the Images folder
        /// </summary>
        /// <returns>The list of images</returns>
        private IEnumerable<ImageFactory> ListInputImagesWithMetadata()
        {
            if (this.imagesFactories == null || !this.imagesFactories.Any())
            {
                this.imagesFactories = new List<ImageFactory>();
                foreach (FileInfo fi in this.ListInputFiles())
                {
                    this.imagesFactories.Add((new ImageFactory(true)).Load(fi.FullName));
                }
            }

            // reset all the images whenever we call this
            foreach (ImageFactory image in this.imagesFactories)
            {
                image.Reset();
            }

            return this.imagesFactories;
        }
    }
}