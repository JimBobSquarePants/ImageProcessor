using System;

namespace ImageProcessorCore.Formats
{
    internal class TiffRationalValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffTagValue value, int count)
        {
            if (value.ValueType != IFDEntryType.Rational)
            {
                return false;
            }
           
            // first 4 bytes are the numerator
            // second 4 bytes are the denominator
           
            int numerator = reader.ReadInt32();
            int denominator = reader.ReadInt32();

            value.Value = new Rational<int>(numerator, denominator);
           
            return true;
        }

    }
}
