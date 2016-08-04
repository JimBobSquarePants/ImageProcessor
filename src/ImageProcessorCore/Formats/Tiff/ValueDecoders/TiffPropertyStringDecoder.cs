using System;
using System.Text;

namespace ImageProcessorCore.Formats
{
    internal class TiffPropertyStringDecoder : ITiffPropertyDecoder
    {
        public bool Decode(TiffReader reader, TiffProperty property, int count)
        {
            if (property.Format != TiffDataFormat.AsciiString)
            {
                return false;
            }

            //TODO: Is this the best we can do?
            byte[] data = reader.ReadBytes(count);
            property.Value = Encoding.UTF8.GetString(data, 0, count).TrimEnd((Char)0);

            return true;
        }
        
    }
}
