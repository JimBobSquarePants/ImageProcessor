using System.Collections.Generic;
using System.IO;

namespace ImageProcessorCore.Formats.Tiff.ValueDecoders
{
    /// <summary>
    /// This decoder handles the <see cref="TiffTagRegistry.TiffSubDirectory"></see> tag. 
    /// It means this entry is a sub directory.
    /// </summary>
    internal class TiffPropertySubDirectoryDecoder : ITiffPropertyDecoder
    {
        public bool Decode(TiffReader reader, TiffProperty property, int count)
        {
            // 0x014A(330)
            if (property.Tag.TagId != TiffTagRegistry.TiffSubDirectory)
            {
                return false;
            }

            List<TiffDirectory> directories = new List<TiffDirectory>();
            var offset = reader.ReadInt32();
            do
            {
                reader.Seek(offset, SeekOrigin.Begin);

                // The exif info is in another tiff directory... go get it
                TiffDirectory dir = new TiffDirectory(reader);
                directories.Add(dir);

                offset = reader.ReadInt32();

            } while (offset != 0);

            property.Value = directories;

            return true;
        }
    }
}
