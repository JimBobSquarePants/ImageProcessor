using System.IO;
using System.Linq;
using System.Collections.Generic;
using ImageProcessorCore.Formats.Tiff;

namespace ImageProcessorCore.Formats
{
    internal class TiffProperty : ITiffAcceptor
    {
        public TiffTag Tag { get; set; }

        public object Value { get; set; }

        public TiffDataFormat Format { get; set; }

        internal static TiffProperty CreateFromIptcTag(int tag, string name, object value)
        {
            return new TiffProperty
            {
                Tag = new TiffTag
                {
                    Name = name,
                    TagGroup = "IPTC",
                    TagId = (ushort) tag
                },
                Value = value
            };
        }

        internal static TiffProperty Create(TiffReader reader)
        {
            ushort tagId = reader.ReadUInt16();

            // The next 2 bytes are the field type
            TiffDataFormat fieldType = (TiffDataFormat) reader.ReadInt16();
            if (fieldType == TiffDataFormat.Invalid)
            {
                // TODO: Handle custom formats.
                return null;
            }

            TiffDataFormatInfo typeInfo = reader.GetTypeInfo(fieldType);
            if (null == typeInfo)
            {
                // TODO: Handle custom formats.
                return null;
            }

            // The next 4 bytes are the component count; which is the number of items
            // in the value array. Could be just one, in which case it's not an array.
            // This is important when determining the size of the value data.
            int componentCount = reader.ReadInt32();

            // Set the reader to the location of the value data
            if (componentCount * typeInfo.TypeSizeInBytes > 4)
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

            TiffProperty value = new TiffProperty{ Tag = tag, Format = fieldType };

            TiffPropertyValueDecoderRegistry.Instance.DecodeValue(reader, componentCount, value);
          
            return value;
        }

        public override string ToString()
        {
            return string.Format( "Tag: {0}, Value: {1}", Tag.Name, Value.ToString()) ;
        }


        public void Accept(ITiffVisitor visitor)
        {
            // Can this value be visited... aka acceptor
            ITiffAcceptor acceptor = Value as ITiffAcceptor;
            if (null != acceptor)
            {
                // yes it can
                acceptor.Accept(visitor);
            }
            else
            {
                // check to see if its enumerable
                IEnumerable<ITiffAcceptor> acceptorList = Value as IEnumerable<ITiffAcceptor>;
                if (null != acceptorList)
                {
                    foreach( ITiffAcceptor acc in acceptorList )
                        acc.Accept(visitor);
                }
            }

        }
    }
}
