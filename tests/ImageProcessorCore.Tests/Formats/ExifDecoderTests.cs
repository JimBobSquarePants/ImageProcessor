using System.Collections.Generic;
using System.IO;
using Xunit;
using ImageProcessorCore;
using ImageProcessorCore.Formats;
using System.Linq;

namespace ImageProcessor.Tests.Formats
{
    public class ExifDecoderTests
    {

        private TiffDecoderCore OpenJpegApp1File(string file)
        {
            FileStream stream = File.OpenRead(file);

            byte[] buffer = new byte[6];
            stream.Read(buffer, 0, 6);
            var exif = buffer[0] == 'E' && buffer[1] == 'x' && buffer[2] == 'i' && buffer[3] == 'f' && buffer[4] == '\0' && buffer[5] == '\0';

            return TiffDecoderCore.Create(stream);
        }

        [Fact]
        public void CheckDSC03718App1Jpeg()
        {
            using (TiffDecoderCore decoder = OpenJpegApp1File("TestImages/Formats/Jpg/DSC03718.JPG.app1"))
            {
                decoder.Decode();
                
                List<ImageProperty> exifProperties = new List<ImageProperty>();
                decoder.FillExifProperties(exifProperties);
                Assert.Equal(28, exifProperties.Count);

                // check the x resolution
                // probably need to put the tag in the property
                // so we can query the property list by tag to find the one we are looking for
                // it also has other goodies like a description of the tag...
                Rational<int> xResolution = (Rational<int>) exifProperties[0].Value; 
                Assert.Equal(1, xResolution.Denominator);
                Assert.Equal(350, xResolution.Numerator);
                
            }
        }

        [Fact]
        public void CheckIMG_5058App1Jpeg()
        {
            using (TiffDecoderCore decoder = OpenJpegApp1File("TestImages/Formats/Jpg/IMG_5085.JPG.app1"))
            {
                decoder.Decode();

                List<ImageProperty> exifProperties = new List<ImageProperty>();
                decoder.FillExifProperties(exifProperties);
                Assert.Equal(45, exifProperties.Count);


            }
        }

    }
}
