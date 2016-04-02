using System;
using System.Collections.Generic;
using ImageProcessorCore.Formats.Tiff.ValueDecoders;
using ImageProcessorCore.IO;

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
                new TiffExifDirectoryDecoder(), // more specific should be first 
                new TiffGPSDirectoryDecoder(),
                new TiffSubDirectoryDecoder(),
                new TiffIptcValueDecoder(),
                new TiffStringValueDecoder(),
                new TiffShortValueDecoder(),
                new TiffLongDecoder(),
                new TiffRationalDecoder(),
                new TiffIgnoreDecoder() // needs to be last...
            };
        }

        public void DecodeValue(TiffReader reader, int valueCount,  TiffProperty value)
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
