using System;

namespace ImageProcessorCore.Formats
{
    public class TiffExifIFDDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value)
        {
            if (entry.TagId != 34665)
            {
                return false;
            }

            int size = entry.ValueCount*4;

            if (size <= 4)
            {
                int offset = reader.GetInt32FromBuffer(entry.OffsetBuffer);

                reader.Seek(offset);
                value.Value = new IFD(reader);
            }

            return true;
        }
    }

}
