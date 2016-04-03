namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// Interface used to decode the value of a <see cref="TiffProperty"/> out of a tiff stream.
    /// </summary>
    internal interface ITiffPropertyDecoder
    {
        /// <summary>
        /// Decodes the <see cref="TiffProperty"/> from the tiff stream.
        /// </summary>
        /// <param name="reader">
        /// The current <see cref="TiffReader"/> from the tiff stream.
        /// <remarks>The tiff stream will be sitting at the value data position in the stream. When Docode is called.</remarks>
        /// </param>
        /// <param name="property">The property we are trying to get the value for.</param>
        /// <param name="count">The count of the type of value data.</param>
        /// <returns>True if the decoder decodes the property; False otherwise.</returns>
        bool Decode(TiffReader reader, TiffProperty property, int count);
    }
}
