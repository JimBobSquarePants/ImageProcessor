using System;
using System.IO;
using System.Text;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// Encapsulates a seekable stream of bytes.
    /// </summary>
    /// <remarks>
    /// This class has several helper or conversion methods that take raw bytes and converts them
    /// to well known .net types. The <see cref="BitOrder"/> can be changed to support Little Endian (intel) 
    /// or Big Endian (motorolla)
    /// </remarks>
    public class TiffReader
    {
        private readonly Stream _stream;
        private TiffReaderSnapshot _begin;

        /// <summary>
        /// The bit order used when converting the stream to .net types.
        /// <remarks>
        /// We will default to Little Endian (intel)
        /// </remarks>
        /// </summary>
        public BitOrder BitOrder { get; set; }


        public TiffReader(Stream stream, BitOrder bitOrder = BitOrder.LittleEndian)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must be capable of seeking.", nameof(stream));
            }

            BitOrder = bitOrder;
            _stream = stream;

            // because we support parsing a tiff image from in the middle of a jpeg image
            // we need to know where the start of the tiff image is in the underlining stream
            _begin = Snapshot();
        }
        

        public bool IsBigEndian => BitOrder == BitOrder.BigEndian;

        public TiffReaderSnapshot Snapshot()
        {
            return new TiffReaderSnapshot {Position = _stream.Position};
        }

        public void Remember(TiffReaderSnapshot snapshot)
        {
            _stream.Seek(snapshot.Position, SeekOrigin.Begin);
        }

        /// <summary>
        /// Gets the next byte in the stream.
        /// </summary>
        /// <returns>The next byte in the stream.</returns>
        /// <exception cref="System.IO.IOException">Could not read the next byte. Probably because we reached to the end of the stream..</exception>
        public byte GetByte()
        {
            int value = _stream.ReadByte();
            if (value == -1)
            {
                throw new IOException("End of data reached.");
            }

            return unchecked((byte)value);
        }


        public void Seek(int offset)
        {
            long start = _begin.Position;

            _stream.Seek(start + offset, SeekOrigin.Begin);
        }

        public sbyte GetSByte(int index)
        {
            byte[] buffer = new byte[1];
            if (1 != _stream.Read(buffer, 0, 1))
            {
                throw new IOException("Failed to read from stream.");
            }
            
            return unchecked((sbyte)buffer[0]);
        }

        public byte[] GetBytes(int count)
        {
            byte[] bytes = new byte[count];
            int totalBytesRead = 0;
            while (totalBytesRead != count)
            {
                var bytesRead = _stream.Read(bytes, totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    throw new IOException("End of data reached.");
                }
                totalBytesRead += bytesRead;
            }
            return bytes;
        }

        public short GetInt16()
        {
            if(IsBigEndian)
            {
                return (short) (GetByte() << 8 | GetByte());
            }

            return (short) (GetByte() | GetByte() << 8);
        }

        public ushort GetUInt16()
        {
            if (IsBigEndian)
            {
                return (ushort)((short)(GetByte() << 8 | GetByte()));
            }

            return (ushort)((short)(GetByte() | GetByte() << 8));
        }

        public uint GetUInt32()
        {
            if (IsBigEndian)
            {
                return (uint) (GetByte() << 24 | GetByte() << 16 | GetByte() << 8 | GetByte());
            }
            return (uint) (GetByte() | GetByte() << 8 | GetByte() << 16 | GetByte() << 24);
        }

        public int GetInt32()
        {
            if (IsBigEndian)
            {
                return (GetByte() << 24 | GetByte() << 16 | GetByte() << 8 | GetByte());
            }
            return (GetByte() | GetByte() << 8 | GetByte() << 16 | GetByte() << 24);
        }

        public long GetInt64()
        {
            if (IsBigEndian)
            {
                return (long)GetByte() << 56 | (long)GetByte() << 48 | (long)GetByte() << 40 | (long)GetByte() << 32 | 
                    (long)GetByte() << 24 | (long)GetByte() << 16 | (long)GetByte() << 8 | GetByte();
            }
            return GetByte() | (long)GetByte() << 8 | (long)GetByte() << 16 | (long)GetByte() << 24 | (long)GetByte() << 32 |
                (long)GetByte() << 40 | (long)GetByte() << 48 | (long)GetByte() << 56;
        }

        public float GetS15Fixed16()
        {
            if (IsBigEndian)
            {
                float res = GetByte() << 8 | GetByte();
                var d = GetByte() << 8 | GetByte();
                return (float)(res + d / 65536.0);
            }
            else
            {
                var d = GetByte() | GetByte() << 8;
                float res = GetByte() | GetByte() << 8;
                return (float)(res + d / 65536.0);
            }
        }

        public float GetFloat32() => BitConverter.ToSingle(BitConverter.GetBytes(GetInt32()), 0);

        public double GetDouble64() => BitConverter.Int64BitsToDouble(GetInt64());

        public string GetString(int bytesRequested) => GetString(bytesRequested, Encoding.UTF8);

        public string GetString(int bytesRequested, Encoding encoding)
        {
            byte[] bytes = GetBytes(bytesRequested);
            return encoding.GetString(bytes, 0, bytes.Length);
        }

        public string GetNullTerminatedString(int maxLengthBytes)
        {
            var bytes = new byte[maxLengthBytes];
            var length = 0;
            while (length < bytes.Length && (bytes[length] = GetByte()) != 0)
            {
                length++;
            }
            return Encoding.ASCII.GetString(bytes, 0, length);
        }

        public int GetInt32FromBuffer(byte[] buffer)
        {
            if (IsBigEndian)
            {
                return buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
            }
            return (buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
        }


        public ushort GetUShortFromBuffer(byte[] buffer)
        {
            if (IsBigEndian)
            {
                return (ushort)((short)(buffer[0] << 8 | buffer[1]));
            }
            return (ushort)((short)(buffer[0] | buffer[1] << 8));
        }

        public uint GetUIntFromBuffer(byte[] buffer)
        {
            if (IsBigEndian)
            {
                return (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);
            }
            return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
        }

        public short GetShortFromBuffer(byte[] buffer)
        {
            if (IsBigEndian)
            {
                return (short)(buffer[0] << 8 | buffer[1]);
            }
            return (short)(buffer[0] | buffer[1] << 8);
        }

        public string GetNullTerminatedStringFromBuffer(byte[] buffer, int maxLengthBytes)
        {
            return Encoding.ASCII.GetString(buffer, 0, maxLengthBytes -1);
        }

    }
}
