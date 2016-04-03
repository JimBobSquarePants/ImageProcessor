using System;
using System.Collections.Generic;
using ImageProcessorCore.Formats.Tiff.ValueDecoders;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// The <see cref="TiffPropertyValueDecoderRegistry"/> maintains a pipeline of value decoders. Each value decoder
    /// is given the chance to decode the value until one of them actually does decode the value.
    /// </summary>
    internal class TiffPropertyValueDecoderRegistry
    {
        private readonly List<ITiffPropertyDecoder> _decoders;

        private static readonly Lazy<TiffPropertyValueDecoderRegistry> Lazy = new Lazy<TiffPropertyValueDecoderRegistry>(() => new TiffPropertyValueDecoderRegistry());

        public static TiffPropertyValueDecoderRegistry Instance => Lazy.Value;
        
        private TiffPropertyValueDecoderRegistry()
        {
            _decoders = new List<ITiffPropertyDecoder>
            {
                new TiffPropertyExifDecoder(), // more specific should be first 
                new TiffPropertyGpsDecoder(),
                new TiffPropertySubDirectoryDecoder(),
                new TiffPropertyIptcDecoder(),
                new TiffPropertyStringDecoder(),
                new TiffPropertyShortDecoder(),
                new TiffPropertyLongDecoder(),
                new TiffPropertyRationalDecoder(),
                new TiffPropertyIgnoreDecoder() // needs to be last...
            };
        }

        public void DecodeValue(TiffReader reader, int valueCount,  TiffProperty value)
        {
            foreach (var valueDecoder in _decoders)
            {
                if (valueDecoder.Decode(reader, value, valueCount))
                {
                    break;
                }
            }
        }
    }
}
