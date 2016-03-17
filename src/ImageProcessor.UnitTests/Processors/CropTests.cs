namespace ImageProcessor.UnitTests.Processors
   {
   using ImageProcessor.Common.Exceptions;
   using ImageProcessor.Processors;
   using ImageProcessor.Imaging;
   using System;
   using System.Drawing;
   using System.Drawing.Imaging;
   using System.IO;

   using NUnit.Framework;

   /// <summary>
   /// The crop tests.
   /// </summary>
   public class CropTests
      {
      /// <summary>
      /// The when processing image.
      /// </summary>
      [TestFixture]
      public class WhenProcessingImage
         {
         /// <summary>
         /// The then resulting image size should be like crop layer
         /// </summary>
         /// <param name="left">
         /// The left position.
         /// </param>
         /// <param name="top">
         /// The top position.
         /// </param>
         /// <param name="right">
         /// The right position.
         /// </param>
         /// <param name="bottom">
         /// The bottom position.
         /// </param>
         /// <param name="mode">
         /// The <see cref="CropMode"/>.
         /// </param>         [Test]
         [TestCase(10.5F, 11.2F, 15.6F, 90.9F, CropMode.Percentage)]
         [TestCase(10.5F, 11.2F, 15.6F, 108.9F, CropMode.Percentage)]
         [TestCase(15.1F, 20.7F, 65.8F, 156.7F, CropMode.Pixels)]
         [TestCase(15.1F, 20.7F, 65.8F, 256.7F, CropMode.Pixels)]
         public void ThenResultingImageSizeShouldBeLikeCropLayer(float left, float top, float right, float bottom, CropMode mode)
            {
            const int sizeX = 200;
            const int sizeY = 200;

            // Keep another version of crop layer because the Crop() method modifies the input parameter
            CropLayer originalCL = new CropLayer(left, top, right, bottom, mode);
            CropLayer cl = new CropLayer(left, top, right, bottom, mode);

            // Arrange
            var crop = new Crop();

            using (Bitmap bitmap = new Bitmap(sizeX, sizeY))
            using (MemoryStream memoryStream = new MemoryStream())
               {
               bitmap.Save(memoryStream, ImageFormat.Bmp);

               memoryStream.Position = 0;

               using (ImageFactory imageFactory = new ImageFactory())
               using (ImageFactory resultImage = imageFactory.Load(memoryStream).Crop(cl))
                  {
                  int expectedWidth;
                  int expectedHeight;

                  if (cl.CropMode == CropMode.Percentage)
                     {
                     float pixelLeft = Math.Max(originalCL.Left, 0);
                     float pixelRight = Math.Min(originalCL.Right, 100.0f);
                     float pixelTop = Math.Max(originalCL.Top, 0);
                     float pixelBottom = Math.Min(originalCL.Bottom, 100.0f);

                     expectedWidth = Convert.ToInt32(sizeX * (pixelRight - pixelLeft) / 100.0f);
                     expectedHeight = Convert.ToInt32(sizeY * (pixelBottom - pixelTop) / 100.0f);
                     }
                  else
                     {
                     float pixelLeft = Math.Max(originalCL.Left, 0);
                     float pixelRight = Math.Min(originalCL.Right, sizeX);
                     float pixelTop = Math.Max(originalCL.Top, 0);
                     float pixelBottom = Math.Min(originalCL.Bottom, sizeY);

                     expectedWidth = Convert.ToInt32(pixelRight - pixelLeft);
                     expectedHeight = Convert.ToInt32(pixelBottom - pixelTop);
                     }

                  // Act // Assert
                  Assert.AreEqual(expectedWidth, resultImage.Image.Width);
                  Assert.AreEqual(expectedHeight, resultImage.Image.Height);
                  }
               }
            }
         }
      }
   }
