using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessorCore.Formats.Tiff.ValueDecoders
{
    internal class TiffExifGPSDirectoryDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffTagValue value, int count)
        {
            if (value.Tag.TagId != 34853)
            {
                return false;
            }

            var offset = reader.ReadInt32();
            reader.Seek(offset, SeekOrigin.Begin);

            // The exif info is in another tiff directory... go get it
            value.Value = new IFD(reader);
            return true;
        }
    }
}
