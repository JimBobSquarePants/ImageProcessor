using System;
using System.Collections.Generic;
using System.IO;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffExifDirectoryDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffProperty value, int count)
        {
            if (value.Tag.TagId != 34665)
            {
                return false;
            }

            List<TiffDirectory> directories = new List<TiffDirectory>();
            var offset = reader.ReadInt32();
            do
            {
                reader.Seek(offset, SeekOrigin.Begin);

                // The exif info is in another tiff directory... go get it
                TiffDirectory dir = new TiffDirectory(reader);
                directories.Add(dir);

                offset = reader.ReadInt32();

            } while (offset != 0);

            value.Value = directories;
          
            return true;
        }
    }

}
