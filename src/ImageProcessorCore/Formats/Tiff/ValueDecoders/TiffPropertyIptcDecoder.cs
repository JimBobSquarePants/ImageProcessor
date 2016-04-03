namespace ImageProcessorCore.Formats.Tiff.ValueDecoders
{
    /// <summary>
    /// The <see cref="TiffPropertyIptcDecoder"/> is responsible for processing the Tiff Tag <see cref="TiffTagRegistry.TiffIptcDirectory"/>
    /// This tag means that the value data is actually an IPTC blob. We must use the <see cref="IptcDecoder"/> to decode the blob.
    /// </summary>
    internal class TiffPropertyIptcDecoder : ITiffPropertyDecoder
    {
        /// <summary>
        /// Decodes the <see cref="TiffTagRegistry.TiffIptcDirectory"/> tag into a <see cref="IptcDirectory"/>
        /// </summary>
        /// <param name="reader">The current <see cref="TiffReader"/>.
        /// <remarks>
        /// The <see cref="TiffReader"/> is expected to be at the offset that contains the location
        /// of the IPTC blocb.</remarks>
        /// </param>
        /// <param name="property">The <see cref="TiffProperty"/> we are trying to decode.</param>
        /// <param name="count">The count of the type of value data.</param>
        /// <returns>True if the decoder decodes the <see cref="TiffDirectory"/>; False otherwise.</returns>
        public bool Decode(TiffReader reader, TiffProperty property, int count)
        {
            if( property.Tag.TagId != TiffTagRegistry.TiffIptcDirectory)
                return false;

            IptcDecoder decoder = IptcDecoder.Create(reader.BaseStream, count*4);
            if (null == decoder)
                return false;

            property.Value = decoder.Decode();
            return true;
        }

    }
}
