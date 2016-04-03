namespace ImageProcessorCore.Formats
{
    internal class TiffPropertyShortDecoder : ITiffPropertyDecoder
    {
        public bool Decode(TiffReader reader, TiffProperty property, int count)
        {
            if (property.Format != TiffDataFormat.Short)
                return false;

            if (count <= 1)
            {
                property.Value = reader.ReadInt16();
                return true;
            }
            
            // the property must be an array of short's
            short[] array = new short[count];
            property.Value = array;
            for (var i = 0; i < count; i++)
            {
                array[i] = reader.ReadInt16();
            }
            
            return true;
        }

    }
}
