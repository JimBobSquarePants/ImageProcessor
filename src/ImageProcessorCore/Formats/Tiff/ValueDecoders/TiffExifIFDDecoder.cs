﻿using System;
using System.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffExifIFDDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffTagValue value, int count)
        {
            if (value.Tag.TagId != 34665)
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
