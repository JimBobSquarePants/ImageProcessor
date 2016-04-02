using System.IO;
using System.Text;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class IptcReader
    {
        readonly EndianBinaryReader _reader;
        private readonly int _beginOfIptcData;

        public IptcReader(Stream stream)
        {
            _beginOfIptcData = (int)stream.Position;
            _reader = new EndianBinaryReader( EndianBitConverter.Big, stream );    
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public short ReadInt16()
        {
            return _reader.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            return _reader.ReadUInt16();
        }

        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }
        
        public int GetPosition()
        {
            return (int)_reader.BaseStream.Position;
        }

        public void Seek(int offset, SeekOrigin origin, bool adjustStart = true)
        {
            if (origin == SeekOrigin.Begin && adjustStart)
            {
                offset = _beginOfIptcData + offset;
            }

            _reader.Seek(offset, origin);
        }

     
    }
}
