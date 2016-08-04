﻿using System.Collections.Generic;
using System.IO;
using ImageProcessorCore.Formats.Tiff;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// The TiffDirectory contains a bunch <see cref="TiffProperty"/> that describe the
    /// image contained in the directory.  Please see spec at http://partners.adobe.com/public/developer/en/tiff/TIFF6.pdf
    /// Section 2. The directory structure is as follows.
    /// </summary>
    /// <exception cref="IOException"></exception>
    internal class TiffDirectory : ITiffAcceptor
    {
        public List<TiffProperty> Entries { get; private set; }

        public string Name { get; set; }

        public TiffDirectory()
        {
            Name = "Tiff Directory";
        }

        public TiffDirectory(TiffReader reader)
        {
            if (null == reader)
                return;

            // first 2 bytes are the number of items TiffDirectory.
            short entryCount = reader.ReadInt16();
            if (entryCount <= 0)
            {
                throw new IOException("Empty Tiff Directory is invalid.");
            }

            Entries = new List<TiffProperty>();

            // Go through the entries each entry is 12 bytes in length
            for (int i = 0; i < entryCount; i++)
            {
                int entryStart = reader.GetPosition();
                var tagValue = TiffProperty.Create(reader);
                if (null != tagValue)
                {
                    Entries.Add(tagValue);
                }

                // go to the next entry in the directory
                entryStart += 12;
                reader.Seek(entryStart, SeekOrigin.Begin, false);
            }

        }

        public void Accept(ITiffVisitor visitor)
        {
            visitor.Visit(this);
            foreach (TiffProperty property in Entries)
            {
                visitor.Visit(property);

                var acceptor = (ITiffAcceptor) property;
                acceptor.Accept(visitor);
            }
        }

        public override string ToString()
        {
            return $"{Name}: contains {Entries.Count} entries.";
        }
    }


}