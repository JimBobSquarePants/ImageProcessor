using System;

namespace ImageProcessorCore.Formats.Tiff.ValueDecoders
{
    public class TiffURationalValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value)
        {
            if (entry.FieldType != IFDEntryType.URational)
                return false;
            
            // URATIONAL = 2 (4-byte) unsinged integers
            var size = entry.ValueCount * 8;
           
            // first 4 bytes are the numerator
            // second 4 bytes are the denominator
            // must be an array of shorts to get here, because a single short
            // size would be less the 4 ( or fit into the offset )
            
            reader.Seek(entry.ValueOffset);

            var numerator = reader.GetInt32();
            var denominator = reader.GetInt32();

            // TODO: strongly type this... for now just use an anonymouse type
            value.Value = new {Numerator = numerator, Denominator = denominator};

            return true;
        }

    }
}
