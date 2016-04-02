using System;
using System.Text;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffStringValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffProperty value, int count)
        {
            if (value.Format != TiffDataFormat.AsciiString)
            {
                return false;
            }

            //TODO: Is this the best we can do?
            byte[] data = reader.ReadBytes(count);
            value.Value = Encoding.ASCII.GetString(data).TrimEnd((Char)0);

            return true;
        }
        
    }
}
