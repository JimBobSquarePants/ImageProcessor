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
    internal class IFD
    {
        public int Count { get; private set; }

        public List<TiffTagValue> Entries { get; private set; } 
        
        public IFD(TiffReader reader)
        {
            // first 2 bytes are the number of IDFEntry's in the directory.
            short entryCount = reader.ReadInt16();
            if (entryCount <= 0)
            {
                return;
            }


            Entries = new List<TiffTagValue>();

            // Go through the entries....
            for (int i = 0; i < entryCount; i++)
            {
                TiffReaderSnapshot entryStart = reader.Snapshot();
                var tagValue = TiffTagValue.Create(reader);
                if (null != tagValue)
                {
                    Entries.Add(tagValue);
                }

                reader.Remember(entryStart);
                reader.Seek( 12, SeekOrigin.Current);
            }

        }

        public override string ToString()
        {
            return string.Format("TIFF Directory: contains {0} entries.", Entries.Count);
        }
    }
}
