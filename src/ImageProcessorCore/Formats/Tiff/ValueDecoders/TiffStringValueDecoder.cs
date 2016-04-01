using System.IO;

namespace ImageProcessorCore.Formats
{
    public class TiffStringValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value)
        {
            if (entry.FieldType != IFDEntryType.AsciiString)
                return false;

            // since the ascii string contains an array of single byte characters
            // the size of the data is just the number of characters.
            var size = entry.ValueCount;

            if (size <= 4)
            {
                value.Value = reader.GetNullTerminatedStringFromBuffer(entry.OffsetBuffer, size);
                value.ValueType = entry.FieldType;
            }
            else
            {
                reader.Seek(entry.ValueOffset);
                value.Value = reader.GetNullTerminatedString(size);
                value.ValueType = entry.FieldType;
            }   
            

            return true;
        }
        
    }
}
