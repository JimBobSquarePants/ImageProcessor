namespace ImageProcessorCore.Formats
{
    internal class TiffPropertyRationalDecoder : ITiffPropertyDecoder
    {
        public bool Decode(TiffReader reader, TiffProperty property, int count)
        {
            if (property.Format != TiffDataFormat.Rational)
            {
                return false;
            }
           
            // first 4 bytes are the numerator
            // second 4 bytes are the denominator
           
            int numerator = reader.ReadInt32();
            int denominator = reader.ReadInt32();

            property.Value = numerator;// new Rational<int>(numerator, denominator);
           
            return true;
        }

    }
}
