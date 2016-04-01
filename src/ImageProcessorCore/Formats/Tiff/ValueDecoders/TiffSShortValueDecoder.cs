using System;

namespace ImageProcessorCore.Formats
{
    public class TiffSShortValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value)
        {
            if (entry.FieldType != IFDEntryType.SShort)
            {
                return false;
            }


            // ushort takes 2 bytes
            int size = entry.ValueCount * 2;

            // Does the value fit into the offset directly
            if (size <= 4)
            {
                value.Value = (short)reader.GetShortFromBuffer(entry.OffsetBuffer);
            }
            else
            {
                // must be an array of shorts to get here, because a single short
                // size would be less the 4 ( or fit into the offset )
                reader.Seek(entry.ValueOffset);

                short[] array = new short[entry.ValueCount];
                for (var i = 0; i < entry.ValueCount; i++)
                {
                    array[i] = reader.GetInt16();
                }

                value.Value = array;

            }

            return true;
        }

    }
}
