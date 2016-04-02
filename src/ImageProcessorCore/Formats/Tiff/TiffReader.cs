using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffReader : IDisposable
    {
        /// <summary>
        /// Remembers the start of the Tiff image in the Tiff stream.
        /// Whenver the tiff image gives us an offset to navigate to.
        /// We have to add the offset to the starting position in the
        /// stream to get to the correct place.
        /// </summary>
        private readonly int _startingPositionInStream;
        
        private readonly EndianBinaryReader _reader;

        private List<int> _directoriesVisited;
        
        internal TiffReader(EndianBinaryReader reader, int startingPosistion)
        {
            _reader = reader;
            _startingPositionInStream = startingPosistion;
            _directoriesVisited = new List<int>();    
        }

        public EndianBitConverter Converter => _reader.BitConverter;
        public Stream BaseStream => _reader.BaseStream;

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

        public int GetPosition()
        {
            return (int) _reader.BaseStream.Position;
        }

        public void Seek(int offset, SeekOrigin origin, bool adjustStart = true )
        {
            if (origin == SeekOrigin.Begin && adjustStart)
            {
                offset = _startingPositionInStream + offset;
            }

            _reader.Seek(offset, origin);
        }

        public TiffDataFormatInfo GetTypeInfo(TiffDataFormat fieldType)
        {
            var enumName = Enum.GetName(typeof(TiffDataFormat), fieldType);
            if (enumName == null)
            {
                // this is an unknown type
                return null;
            }

            MemberInfo[] members = typeof(TiffDataFormat).GetMember(enumName);
            return members.First().GetCustomAttribute<TiffDataFormatInfo>();
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
