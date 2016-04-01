using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ImageProcessorCore.Formats
{
    internal class TiffTagValue
    {
        public TiffTag Tag { get; set; }

        public object Value { get; set; }

        public IFDEntryType ValueType { get; set; }

        private static IFDEntryTypeInfo GetTypeInfo(IFDEntryType fieldType)
        {
            var enumName = Enum.GetName(typeof (IFDEntryType), fieldType);
            if (enumName == null)
            {
                // this is an unknown type
                return null;
            }

            MemberInfo[] members = typeof(IFDEntryType).GetMember(enumName);
            return members.First().GetCustomAttribute<IFDEntryTypeInfo>();
        }

        internal static TiffTagValue Create(TiffReader reader)
        {
            // Each entry is 12 bytes in length. The first 2 bytes are the tag
            ushort tagId = reader.ReadUInt16();

            // The next 2 bytes are the field type
            IFDEntryType fieldType = (IFDEntryType) reader.ReadInt16();
            if (fieldType == IFDEntryType.Invalid)
            {
                return null;
            }

            IFDEntryTypeInfo typeInfo = GetTypeInfo(fieldType);
            if (null == typeInfo)
            {
                return null;
            }

            // The next 4 bytes are the value count; which is the number of items
            // in the value array. Could be just one, in which case it's not an array.
            // This is important when determining the size of the value data.
            int valueCount = reader.ReadInt32();

            // Set the reader to the location of the value data
            if (valueCount * typeInfo.TypeSizeInBytes > 4)
            {
                // the value data is somewhere else in the file. Go
                // to that location so TiffReader is ready for the ValueDecoders.
                int positionOfData = reader.ReadInt32();
                reader.Seek(positionOfData, SeekOrigin.Begin);
            }
            

            TiffTag tag = TiffTagRegistry.Instance.Tags.SingleOrDefault(t => t.TagId == tagId) 
                ?? new TiffTag
                {
                    TagId = tagId,
                    TagGroup = "General",
                    Name = "Unknown",
                    Description = "This tag is not in the registry"
                };

            TiffTagValue value = new TiffTagValue{Tag = tag, ValueType = fieldType};

            TiffValueDecoderRegistry.Instance.DecodeValue(reader, valueCount, value);
          
            return value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
