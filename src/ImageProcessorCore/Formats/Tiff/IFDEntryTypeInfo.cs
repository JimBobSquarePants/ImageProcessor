using System;
using System.Collections.Generic;

namespace ImageProcessorCore.Formats
{
    public class IFDEntryTypeInfo : Attribute
    {
        public int TypeSizeInBytes { get; set; }
       
    }
}
