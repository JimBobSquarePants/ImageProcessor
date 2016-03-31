using System;
using System.Collections.Generic;

namespace ImageProcessorCore.Formats.Tiff.ValueDecoders
{
    public class TiffValueDecoderRegistry
    {
        private List<ITiffValueDecoder> _decoders;

        private static readonly Lazy<TiffValueDecoderRegistry> Lazy = new Lazy<TiffValueDecoderRegistry>(() => new TiffValueDecoderRegistry());

        public static TiffValueDecoderRegistry Instance => Lazy.Value;
        
        private TiffValueDecoderRegistry()
        {
            _decoders = new List<ITiffValueDecoder>
            {
                new TiffExifIFDDecoder(), // more specific should be first 
                new TiffStringValueDecoder(),
                new TiffUShortValueDecoder(),
                new TiffSShortValueDecoder(),
                new TiffULongValueDecoder(),
                new TiffURationalValueDecoder(),
                new TiffIgnoreValueDecoder() // needs to be last...
            };
        }

        public void DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value)
        {
            foreach (var valueDecoder in _decoders)
            {
                if (valueDecoder.DecodeValue(reader, entry, value))
                    break;
            }
        }
    }
}
