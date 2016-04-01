using System;
using System.Collections.Generic;
using ImageProcessorCore.Formats.Tiff.ValueDecoders;

namespace ImageProcessorCore.Formats
{
    internal class TiffValueDecoderRegistry
    {
        private readonly List<ITiffValueDecoder> _decoders;

        private static readonly Lazy<TiffValueDecoderRegistry> Lazy = new Lazy<TiffValueDecoderRegistry>(() => new TiffValueDecoderRegistry());

        public static TiffValueDecoderRegistry Instance => Lazy.Value;
        
        private TiffValueDecoderRegistry()
        {
            _decoders = new List<ITiffValueDecoder>
            {
                new TiffExifIFDDecoder(), // more specific should be first 
                new TiffExifGPSDirectoryDecoder(),
                new TiffStringValueDecoder(),
                new TiffShortValueDecoder(),
                new TiffLongValueDecoder(),
                new TiffRationalValueDecoder(),
                new TiffIgnoreValueDecoder() // needs to be last...
            };
        }

        public void DecodeValue(TiffReader reader, int valueCount,  TiffTagValue value)
        {
            foreach (var valueDecoder in _decoders)
            {
                if (valueDecoder.DecodeValue(reader, value, valueCount))
                {
                    break;
                }
            }
        }
    }
}
