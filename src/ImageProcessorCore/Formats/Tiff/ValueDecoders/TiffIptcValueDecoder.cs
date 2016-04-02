using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats.Tiff.ValueDecoders
{
    internal class TiffIptcValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffProperty value, int count)
        {
            if( value.Tag.TagId != 33723)
                return false;

            IptcDecoder decoder = IptcDecoder.Create(reader.BaseStream, count*4);
            if (null == decoder)
                return false;

            value.Value = decoder.Decode();
            return true;
        }

    }
}
