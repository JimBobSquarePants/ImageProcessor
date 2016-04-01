using System;

namespace ImageProcessorCore.Formats
{
    public class TiffRationalValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value)
        {
            if (entry.FieldType != IFDEntryType.Rational)
            {
                return false;
            }
            
            // URATIONAL = 2 (4-byte) unsinged integers
            int size = entry.ValueCount * 8;
           
            // first 4 bytes are the numerator
            // second 4 bytes are the denominator
            // must be an array of shorts to get here, because a single short
            // size would be less the 4 ( or fit into the offset )
            
            reader.Seek(entry.ValueOffset);

            int numerator = reader.GetInt32();
            int denominator = reader.GetInt32();

            value.Value = new Rational<int>(numerator, denominator);
           
            return true;
        }

    }
}
