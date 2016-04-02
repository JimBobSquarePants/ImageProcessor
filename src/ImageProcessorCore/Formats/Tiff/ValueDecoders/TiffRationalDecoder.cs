using System;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffRationalDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffProperty value, int count)
        {
            if (value.Format != TiffDataFormat.Rational)
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
