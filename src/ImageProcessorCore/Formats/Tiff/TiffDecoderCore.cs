using System.Collections.Generic;
using System.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffDecoderCore
    {
        private TiffReader _tiffReader;
        private TiffHeader _header;
        private List<IFD> _directories;
            
        public void Decode(Stream stream)
        {
            _tiffReader = new TiffReader(stream);
            _header = new TiffHeader(_tiffReader);
               
            _directories = new List<IFD>();
            int nextOffset = _header.Offset;
            do
            {
                _tiffReader.Seek(nextOffset);

                IFD directory = new IFD(_tiffReader);
                _directories.Add(directory);


                nextOffset = _tiffReader.GetInt32();

            } while (nextOffset != 0);
            
        }

    }
}
