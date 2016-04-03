using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffPropertyLongDecoder : ITiffPropertyDecoder
    {
        public bool Decode(TiffReader reader, TiffProperty property, int count)
        {
            if (property.Format != TiffDataFormat.Long)
                return false;

            if (count <= 1)
            {
                property.Value = reader.ReadInt32();
                return true;
            }

            // the property must be an array of ints's
            int[] array = new int[count];
            property.Value = array;
            for (var i = 0; i < count; i++)
            {
                array[i] = reader.ReadInt32();
            }

            return true;
        }

    }
}
