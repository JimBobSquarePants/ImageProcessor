using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffLongDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffProperty value, int count)
        {
            if (value.Format != TiffDataFormat.Long)
                return false;

            if (count <= 1)
            {
                value.Value = reader.ReadInt32();
                return true;
            }

            // the value must be an array of ints's
            int[] array = new int[count];
            value.Value = array;
            for (var i = 0; i < count; i++)
            {
                array[i] = reader.ReadInt32();
            }

            return true;
        }

    }
}
