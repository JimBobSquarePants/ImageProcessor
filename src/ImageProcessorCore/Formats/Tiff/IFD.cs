using System.Collections.Generic;
using System.IO;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// The IFD is short for 'Image File Directory'. It contains a bunch <see cref="IFDEntry"/> that describe the
    /// image contained in the directory.  Please see spec at http://partners.adobe.com/public/developer/en/tiff/TIFF6.pdf
    /// Section 2. The directory structure is as follows.
    /// 
    /// First 2 bytes are the number of <see cref="IFDEntry"/> in the directory. Followed by each <see cref="IFDEntry"/>
    /// which are 12 bytes each. This is followed by an offset to the next <see cref="IFD"/> record in the tiff file.
    /// if the next record contains 4 bytes of 0's. There are no more <see cref="IFD"/> entries in the file.
    /// </summary>
    /// <exception cref="IOException"></exception>
    public class IFD
    {
        public int Count { get; private set; }

        public List<TiffTagValue> Entries { get; private set; } 
        
        public IFD(TiffReader reader)
        {
            // first 2 bytes are the number of IDFEntry's in the directory.
            var entryCount = reader.GetInt16();
            if( entryCount <= 0 )
                throw new IOException("Invalid tiff format. No IDFEntrys found in the directory.");

            Entries = new List<TiffTagValue>();

            // Go through the entries....
            for (int i = 0; i < entryCount; i++)
            {
                // Each entry is 12 bytes in length. The first 2 bytes are the tag
                var tagId = reader.GetUInt16();

                // The next 2 bytes are the field type
                var fieldType = reader.GetInt16();

                // The next 4 bytes are the value count.
                var valueCount = reader.GetInt32();

                var offsetBuffer = reader.GetBytes(4);

                // The last 4 bytes are the value offset, or the actual value if
                // it fits in the 4 bytes. Spec says the value offset must
                // be on a word boundry. Which means it must be an even number.
                // Todo: add a check for this condition...
                //var valueOffsetOrValue = reader.GetInt32();
                
                var entry = new IFDEntry
                {
                    TagId = tagId,
                    FieldType =  (IFDEntryType)fieldType,
                    ValueCount = valueCount,
                    OffsetBuffer = offsetBuffer,
                    ValueOffset = reader.GetInt32FromBuffer(offsetBuffer)
                };

                var snapshot = reader.Snapshot();
                Entries.Add( TiffTagValue.Create(reader, entry) );
                reader.Remember(snapshot);
            }
           
        }


        private int GetFieldTypeSizeInBytes(IFDEntryType entryType)
        {
            // size in bytes
            var typeSize = 1;
            switch (entryType)
            {
                case IFDEntryType.Double:
                    typeSize = 8;
                    break;
                case IFDEntryType.Float:
                    typeSize = 4;
                    break;
                case IFDEntryType.SLong:
                    typeSize = 4;
                    break;
                case IFDEntryType.SRational:
                    typeSize = 8;
                    break;
                case IFDEntryType.SShort:
                    typeSize = 2;
                    break;
                case IFDEntryType.Short:
                    typeSize = 2;
                    break;
                case IFDEntryType.Long:
                    typeSize = 4;
                    break;
                case IFDEntryType.Rational:
                    typeSize = 8;
                    break;
                default:
                    typeSize = 1;
                    break;
            }

            return typeSize;
            
        }

        public override string ToString()
        {
            return string.Format("TIFF Directory: contains {0} entries.", Entries.Count);
        }
    }
}
