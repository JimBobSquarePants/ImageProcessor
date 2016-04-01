using System;
using System.Text;

namespace ImageProcessorCore.Formats
{
    internal class TiffStringValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffTagValue value, int count)
        {
            if (value.ValueType != IFDEntryType.AsciiString)
            {
                return false;
            }

            byte[] data = reader.ReadBytes(count);
            value.Value = Encoding.ASCII.GetString(data).TrimEnd((Char)0);

            return true;
        }
        
    }
}
