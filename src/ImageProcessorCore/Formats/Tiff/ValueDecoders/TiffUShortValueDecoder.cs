using System;

namespace ImageProcessorCore.Formats
{
    public class TiffUShortValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value)
        {
            if (entry.FieldType != IFDEntryType.Short)
                return false;

            // no need... to big
            if (entry.ValueCount > 1600)
                return false;

            // ushort takes 2 bytes
            var size = entry.ValueCount * 2;

            // Does the value fit into the offset directly
            if (size <= 4 )
            {
                value.Value = (ushort) reader.GetUShortFromBuffer(entry.OffsetBuffer);
            }
            else
            {
                // must be an array of shorts to get here, because a single short
                // size would be less the 4 ( or fit into the offset )
                reader.Seek(entry.ValueOffset);

                var array = new ushort[entry.ValueCount];
                for (var i = 0; i < entry.ValueCount; i++)
                    array[i] = reader.GetUInt16();

                value.Value = array;

            }

            return true;
        }

    }
}
