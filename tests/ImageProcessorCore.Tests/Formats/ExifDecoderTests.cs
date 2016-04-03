using System.Collections.Generic;
using System.IO;
using Xunit;
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

                // Make sure we got 2 directory's from this file.
                Assert.Equal(2, decoder.Directories.Count);

                // Make sure we have 12 properties in the first directory
                Assert.Equal(12, decoder.Directories[0].Entries.Count);

                // Make sure we have 12 properties in the first directory
                Assert.Equal(13, decoder.Directories[1].Entries.Count);

                // Now make sure we have an exif sub directory
                var exifProperty = decoder.Directories[0].Entries.Single(i => i.Tag.TagId == TiffTagRegistry.TiffExifDirectory);

                // should only be 1 exif directory for this file
                IEnumerable<TiffDirectory> exifDirectoreis = exifProperty.Value as IEnumerable<TiffDirectory>;
                Assert.Equal(1, exifDirectoreis.Count());

                // Should be 37 exif properties
                Assert.Equal(37, exifDirectoreis.First().Entries.Count);

            }
        }

        [Fact]
        public void CheckIMG_5058App1Jpeg()
        {
            using (TiffDecoderCore decoder = OpenJpegApp1File("TestImages/Formats/Jpg/IMG_5085.JPG.app1"))
            {
                decoder.Decode();

                // Make sure we got 2 directory's from this file.
                Assert.Equal(2, decoder.Directories.Count);

                // Make sure we have 13properties in the first directory
                Assert.Equal(13, decoder.Directories[0].Entries.Count);

                // Make sure we have 6 properties in the first directory
                Assert.Equal(6, decoder.Directories[1].Entries.Count);

                // Now make sure we have an exif sub directory
                var exifProperty = decoder.Directories[0].Entries.Single(i => i.Tag.TagId == TiffTagRegistry.TiffExifDirectory);

                // should only be 1 exif directory for this file
                IEnumerable<TiffDirectory> exifDirectoreis = exifProperty.Value as IEnumerable<TiffDirectory>;
                Assert.Equal(1, exifDirectoreis.Count());

                // Should be 34 exif properties
                Assert.Equal(34, exifDirectoreis.First().Entries.Count);

            }
        }

    }
}
