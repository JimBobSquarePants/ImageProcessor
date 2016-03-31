using System;
using System.IO;

namespace ImageProcessorCore.Formats.Tiff
{
    public class IFDEntry
    {
        public  byte[] OffsetBuffer { get; set; }

        public ushort TagId { get; set; }

        public IFDEntryType FieldType { get; set; }

        public int ValueCount { get; set; }

        public int ValueOffset { get; set; }

    }
}
