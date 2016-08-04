using System;
using System.Collections.Generic;

namespace ImageProcessorCore.Formats
{
    public class TiffDataFormatInfo : Attribute
    {
        public int TypeSizeInBytes { get; set; }

        public TiffDataFormatInfo()
        {
            
        }

        public TiffDataFormatInfo(int sizeInBytes)
        {
            TypeSizeInBytes = sizeInBytes;
        }
       
    }
}
