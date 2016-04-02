using System;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffShortValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffProperty value, int count)
        {
            if (value.Format != TiffDataFormat.Short)
                return false;

            if (count <= 1)
            {
                value.Value = reader.ReadInt16();
                return true;
            }
            
            // the value must be an array of short's
            short[] array = new short[count];
            value.Value = array;
            for (var i = 0; i < count; i++)
            {
                array[i] = reader.ReadInt16();
            }
            
            return true;
        }

    }
}
