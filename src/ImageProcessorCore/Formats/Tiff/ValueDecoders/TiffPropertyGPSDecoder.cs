using System.Collections.Generic;
using System.IO;

namespace ImageProcessorCore.Formats.Tiff.ValueDecoders
{
    /// <summary>
    /// The <see cref="TiffPropertyGpsDecoder"/> is responsible for processing the Tiff Tag <see cref="TiffTagRegistry.TiffGpsDirectory"/>
    /// This tag means that the value data is actually another <see cref="TiffDirectory"/> with gps <see cref="TiffProperty"/> in it.
    /// </summary>
    internal class TiffPropertyGpsDecoder : ITiffPropertyDecoder
    {
        /// <summary>
        /// Decodes the <see cref="TiffTagRegistry.TiffGpsDirectory"/> tag into a <see cref="TiffDirectory"/>
        /// </summary>
        /// <param name="reader">The current <see cref="TiffReader"/>.
        /// <remarks>
        /// The <see cref="TiffReader"/> is expected to be at the offset that contains the location
        /// of the Tiff Directory chain.</remarks>
        /// </param>
        /// <param name="property">The <see cref="TiffProperty"/> we are trying to decode.</param>
        /// <param name="count">The count of the type of value data.</param>
        /// <returns>True if the decoder decodes the <see cref="TiffDirectory"/>; False otherwise.</returns>
        public bool Decode(TiffReader reader, TiffProperty property, int count)
        {
            // If its not the right tag id then do nothing and let another
            // decoder have a shot at it.
            if (property.Tag.TagId != TiffTagRegistry.TiffGpsDirectory)
            {
                return false;
            }

            // This property is actually a Tiff Directory. Technically; according
            // to the TIFF spec, this directory could be chained to other
            // directories. However, I don't think the GPS Directory ever does
            // this. We will go ahead and process them if they are there.
            List<TiffDirectory> directories = new List<TiffDirectory>();
            var offset = reader.ReadInt32();
            do
            {
                // Get the offset in the tiff stream where the first Tiff Directory is located
                reader.Seek(offset, SeekOrigin.Begin);

                // Process the tiff directory
                TiffDirectory dir = new TiffDirectory(reader);
                dir.Name = "GPS Directory";
                directories.Add(dir);

                // Get the location of the next chained directory. If no other
                // directories are chained to this one. We should read 0 from
                // the tiff stream.
                offset = reader.ReadInt32();

            } while (offset != 0);

            // Set the value of the property.
            property.Value = directories;

            // We have processed this property. Prevent others from
            // trying to process this property by returning true.
            return true;
        }
    }
}
