// <copyright file="JpegDecoderCore.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>
namespace ImageProcessorCore.Formats
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    internal class JpegDecoderCore
    {
        private class errMissingFF00 : Exception
        {
        }

        private class errShortHuffmanData : Exception
        {
        }

        private class errEOF : Exception
        {
        }

        // maxCodeLength is the maximum (inclusive) number of bits in a Huffman code.
        private const int maxCodeLength = 16;

        // maxNCodes is the maximum (inclusive) number of codes in a Huffman tree.
        private const int maxNCodes = 256;

        // lutSize is the log-2 size of the Huffman decoder's look-up table.
        private const int lutSize = 8;

        private const int maxComponents = 4;

        private const int maxTc = 1;

        private const int maxTh = 3;

        private const int maxTq = 3;

        private const int dcTable = 0;

        private const int acTable = 1;

        // private const int sof0Marker = 0xc0; // Start Of Frame (Baseline).
        // private const int sof1Marker = 0xc1; // Start Of Frame (Extended Sequential).
        private const int sof2Marker = 0xc2; // Start Of Frame (Progressive).

        private const int dhtMarker = 0xc4; // Define Huffman Table.

        private const int rst0Marker = 0xd0; // ReSTart (0).

        private const int rst7Marker = 0xd7; // ReSTart (7).

        private const int soiMarker = 0xd8; // Start Of Image.

        private const int eoiMarker = 0xd9; // End Of Image.

        private const int sosMarker = 0xda; // Start Of Scan.

        private const int dqtMarker = 0xdb; // Define Quantization Table.

        private const int driMarker = 0xdd; // Define Restart Interval.

        private const int comMarker = 0xfe; // COMment.

        // "APPlication specific" markers aren't part of the JPEG spec per se,
        // but in practice, their use is described at
        // http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html
        private const int app0Marker = 0xe0;

        private const int app14Marker = 0xee;

        private const int app15Marker = 0xef;

        // See http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
        private const int adobeTransformUnknown = 0;

        private const int adobeTransformYCbCr = 1;

        private const int adobeTransformYCbCrK = 2;

        // unzig maps from the zig-zag ordering to the natural ordering. For example,
        // unzig[3] is the column and row of the fourth element in zig-zag order. The
        // value is 16, which means first column (16%8 == 0) and third row (16/8 == 2).
        private static readonly int[] unzig = new[]
                                                  {
                                                      0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5, 12, 19, 26, 
                                                      33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28, 35, 42, 49, 56, 57, 
                                                      50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51, 58, 59, 52, 45, 38, 31, 
                                                      39, 46, 53, 60, 61, 54, 47, 55, 62, 63, 
                                                  };

        private class Component
        {
            public int h; // Horizontal sampling factor.

            public int v; // Vertical sampling factor.

            public byte c; // Component identifier.

            public byte tq; // Quantization table destination selector.
        }

        public JpegDecoderCore()
        {
            this.huff = new huffman_class[maxTc + 1, maxTh + 1];
            this.quant = new Block[maxTq + 1];
            this.tmp = new byte[2 * Block.BlockSize];
            this.comp = new Component[maxComponents];
            this.progCoeffs = new Block[maxComponents][];
            this.bits = new bits_class();
            this.bytes = new bytes_class();

            for (int i = 0; i < maxTc + 1; i++) for (int j = 0; j < maxTh + 1; j++) this.huff[i, j] = new huffman_class();

            for (int i = 0; i < this.quant.Length; i++) this.quant[i] = new Block();

            for (int i = 0; i < this.comp.Length; i++) this.comp[i] = new Component();
        }

        private class img_ycbcr
        {
            public enum YCbCrSubsampleRatio
            {
                YCbCrSubsampleRatio444, 

                YCbCrSubsampleRatio422, 

                YCbCrSubsampleRatio420, 

                YCbCrSubsampleRatio440, 

                YCbCrSubsampleRatio411, 

                YCbCrSubsampleRatio410, 
            }

            private static void yCbCrSize(int w, int h, YCbCrSubsampleRatio ratio, out int cw, out int ch)
            {
                switch (ratio)
                {
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio422:
                        cw = (w + 1) / 2;
                        ch = h;
                        break;
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio420:
                        cw = (w + 1) / 2;
                        ch = (h + 1) / 2;
                        break;
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio440:
                        cw = w;
                        ch = (h + 1) / 2;
                        break;
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio411:
                        cw = (w + 3) / 4;
                        ch = h;
                        break;
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio410:
                        cw = (w + 3) / 4;
                        ch = (h + 1) / 2;
                        break;
                    default:

                        // Default to 4:4:4 subsampling.
                        cw = w;
                        ch = h;
                        break;
                }
            }

            private img_ycbcr()
            {
            }

            public byte[] pix_y;

            public byte[] pix_cb;

            public byte[] pix_cr;

            public int y_stride;

            public int c_stride;

            public int y_offset;

            public int c_offset;

            public int x;

            public int y;

            public int w;

            public int h;

            public YCbCrSubsampleRatio ratio;

            public img_ycbcr(int w, int h, YCbCrSubsampleRatio ratio)
            {
                int cw, ch;
                yCbCrSize(w, h, ratio, out cw, out ch);
                this.pix_y = new byte[w * h];
                this.pix_cb = new byte[cw * ch];
                this.pix_cr = new byte[cw * ch];
                this.ratio = ratio;
                this.y_stride = w;
                this.c_stride = cw;
                this.x = 0;
                this.y = 0;
                this.w = w;
                this.h = h;
            }

            public img_ycbcr subimage(int x, int y, int w, int h)
            {
                var ret = new img_ycbcr
                              {
                                  w = w, 
                                  h = h, 
                                  pix_y = this.pix_y, 
                                  pix_cb = this.pix_cb, 
                                  pix_cr = this.pix_cr, 
                                  ratio = this.ratio, 
                                  y_stride = this.y_stride, 
                                  c_stride = this.c_stride, 
                                  y_offset = y * this.y_stride + x, 
                                  c_offset = y * this.c_stride + x
                              };
                return ret;
            }

            public int get_row_y_offset(int y)
            {
                return y * this.y_stride;
            }

            public int get_row_c_offset(int y)
            {
                switch (this.ratio)
                {
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio422:
                        return y * this.c_stride;
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio420:
                        return (y / 2) * this.c_stride;
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio440:
                        return (y / 2) * this.c_stride;
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio411:
                        return y * this.c_stride;
                    case YCbCrSubsampleRatio.YCbCrSubsampleRatio410:
                        return (y / 2) * this.c_stride;
                    default:
                        return y * this.c_stride;
                }
            }
        }

        public class img_gray
        {
            public byte[] pixels;

            public int stride;

            public int x;

            public int y;

            public int w;

            public int h;

            public int offset;

            private img_gray()
            {
            }

            public img_gray(int w, int h)
            {
                this.w = w;
                this.h = h;
                this.pixels = new byte[w * h];
                this.stride = w;
                this.offset = 0;
            }

            public img_gray subimage(int x, int y, int w, int h)
            {
                var ret = new img_gray
                              {
                                  w = w, 
                                  h = h, 
                                  pixels = this.pixels, 
                                  stride = this.stride, 
                                  offset = y * this.stride + x
                              };
                return ret;
            }

            public int get_row_offset(int y)
            {
                return this.offset + y * this.stride;
            }
        }

        private class huffman_class
        {
            public huffman_class()
            {
                this.lut = new ushort[1 << lutSize];
                this.vals = new byte[maxNCodes];
                this.minCodes = new int[maxCodeLength];
                this.maxCodes = new int[maxCodeLength];
                this.valsIndices = new int[maxCodeLength];
                this.nCodes = 0;
            }

            // length is the number of codes in the tree.
            public int nCodes;

            // lut is the look-up table for the next lutSize bits in the bit-stream.
            // The high 8 bits of the uint16 are the encoded value. The low 8 bits
            // are 1 plus the code length, or 0 if the value is too large to fit in
            // lutSize bits.
            public ushort[] lut;

            // vals are the decoded values, sorted by their encoding.
            public byte[] vals;

            // minCodes[i] is the minimum code of length i, or -1 if there are no
            // codes of that length.
            public int[] minCodes;

            // maxCodes[i] is the maximum code of length i, or -1 if there are no
            // codes of that length.
            public int[] maxCodes;

            // valsIndices[i] is the index into vals of minCodes[i].
            public int[] valsIndices;
        }

        // bytes is a byte buffer, similar to a bufio.Reader, except that it
        // has to be able to unread more than 1 byte, due to byte stuffing.
        // Byte stuffing is specified in section F.1.2.3.
        private class bytes_class
        {
            public bytes_class()
            {
                this.buf = new byte[4096];
                this.i = 0;
                this.j = 0;
                this.nUnreadable = 0;
            }

            // buf[i:j] are the buffered bytes read from the underlying
            // io.Reader that haven't yet been passed further on.
            public byte[] buf;

            public int i;

            public int j;

            // nUnreadable is the number of bytes to back up i after
            // overshooting. It can be 0, 1 or 2.
            public int nUnreadable;
        }

        // bits holds the unprocessed bits that have been taken from the byte-stream.
        // The n least significant bits of a form the unread bits, to be read in MSB to
        // LSB order.
        private class bits_class
        {
            public uint a; // accumulator.

            public uint m; // mask. m==1<<(n-1) when n>0, with m==0 when n==0.

            public int n; // the number of unread bits in a.
        }

        public int width;

        public int height;

        public int nComp;

        public img_gray img1; // grayscale

        // public img_rgb imgrgb; //RGB
        private img_ycbcr img3; // YCrCb

        private Stream inputStream;

        private bits_class bits;

        private bytes_class bytes;

        private byte[] blackPix;

        private int blackStride;

        private int ri; // Restart Interval.

        private bool progressive;

        private bool jfif;

        private bool adobeTransformValid;

        private byte adobeTransform;

        private ushort eobRun; // End-of-Band run, specified in section G.1.2.2.

        private Component[] comp;

        private Block[][] progCoeffs; // Saved state between progressive-mode scans.

        private huffman_class[,] huff;

        private Block[] quant; // Quantization tables, in zig-zag order.

        private byte[] tmp;

        // ensureNBits reads bytes from the byte buffer to ensure that bits.n is at
        // least n. For best performance (avoiding function calls inside hot loops),
        // the caller is the one responsible for first checking that bits.n < n.
        private void ensureNBits(int n)
        {
            while (true)
            {
                var c = this.readByteStuffedByte();
                this.bits.a = (this.bits.a << 8) | (uint)c;
                this.bits.n += 8;
                if (this.bits.m == 0) this.bits.m = 1 << 7;
                else this.bits.m <<= 8;
                if (this.bits.n >= n) break;
            }
        }

        // receiveExtend is the composition of RECEIVE and EXTEND, specified in section
        // F.2.2.1.
        private int receiveExtend(byte t)
        {
            if (this.bits.n < t) this.ensureNBits(t);

            this.bits.n -= t;
            this.bits.m >>= t;
            int s = 1 << t;
            int x = (int)((this.bits.a >> this.bits.n) & (s - 1));
            if (x < (s >> 1)) x += ((-1) << t) + 1;
            return x;
        }

        // processDHT processes a Define Huffman Table marker, and initializes a huffman
        // struct from its contents. Specified in section B.2.4.2.
        private void processDHT(int n)
        {
            while (n > 0)
            {
                if (n < 17) throw new ImageFormatException("DHT has wrong length");

                this.readFull(this.tmp, 0, 17);

                int tc = this.tmp[0] >> 4;
                if (tc > maxTc) throw new ImageFormatException("bad Tc value");

                int th = this.tmp[0] & 0x0f;
                if (th > maxTh || !this.progressive && th > 1) throw new ImageFormatException("bad Th value");

                huffman_class h = this.huff[tc, th];

                // Read nCodes and h.vals (and derive h.nCodes).
                // nCodes[i] is the number of codes with code length i.
                // h.nCodes is the total number of codes.
                h.nCodes = 0;

                int[] ncodes = new int[maxCodeLength];
                for (int i = 0; i < ncodes.Length; i++)
                {
                    ncodes[i] = this.tmp[i + 1];
                    h.nCodes += ncodes[i];
                }

                if (h.nCodes == 0) throw new ImageFormatException("Huffman table has zero length");
                if (h.nCodes > maxNCodes) throw new ImageFormatException("Huffman table has excessive length");

                n -= h.nCodes + 17;
                if (n < 0) throw new ImageFormatException("DHT has wrong length");

                this.readFull(h.vals, 0, h.nCodes);

                // Derive the look-up table.
                for (int i = 0; i < h.lut.Length; i++) h.lut[i] = 0;

                uint x = 0, code = 0;

                for (int i = 0; i < lutSize; i++)
                {
                    code <<= 1;

                    for (int j = 0; j < ncodes[i]; j++)
                    {
                        // The codeLength is 1+i, so shift code by 8-(1+i) to
                        // calculate the high bits for every 8-bit sequence
                        // whose codeLength's high bits matches code.
                        // The high 8 bits of lutValue are the encoded value.
                        // The low 8 bits are 1 plus the codeLength.
                        byte base2 = (byte)(code << (7 - i));
                        ushort lutValue = (ushort)(((ushort)h.vals[x] << 8) | (2 + i));
                        for (int k = 0; k < 1 << (7 - i); k++) h.lut[base2 | k] = lutValue;
                        code++;
                        x++;
                    }
                }

                // Derive minCodes, maxCodes, and valsIndices.
                int c = 0, index = 0;
                for (int i = 0; i < ncodes.Length; i++)
                {
                    int nc = ncodes[i];
                    if (nc == 0)
                    {
                        h.minCodes[i] = -1;
                        h.maxCodes[i] = -1;
                        h.valsIndices[i] = -1;
                    }
                    else
                    {
                        h.minCodes[i] = c;
                        h.maxCodes[i] = c + nc - 1;
                        h.valsIndices[i] = index;
                        c += nc;
                        index += nc;
                    }

                    c <<= 1;
                }
            }
        }

        // decodeHuffman returns the next Huffman-coded value from the bit-stream,
        // decoded according to h.
        private byte decodeHuffman(huffman_class h)
        {
            if (h.nCodes == 0) throw new ImageFormatException("uninitialized Huffman table");

            if (this.bits.n < 8)
            {
                try
                {
                    this.ensureNBits(8);
                }
                catch (errMissingFF00)
                {
                    if (this.bytes.nUnreadable != 0) this.unreadByteStuffedByte();
                    goto slowPath;
                }
                catch (errShortHuffmanData)
                {
                    if (this.bytes.nUnreadable != 0) this.unreadByteStuffedByte();
                    goto slowPath;
                }
            }

            ushort v = h.lut[(this.bits.a >> (this.bits.n - lutSize)) & 0xff];
            if (v != 0)
            {
                byte n = (byte)((v & 0xff) - 1);
                this.bits.n -= n;
                this.bits.m >>= n;
                return (byte)(v >> 8);
            }

            slowPath:
            int code = 0;
            for (int i = 0; i < maxCodeLength; i++)
            {
                if (this.bits.n == 0) this.ensureNBits(1);
                if ((this.bits.a & this.bits.m) != 0) code |= 1;
                this.bits.n--;
                this.bits.m >>= 1;
                if (code <= h.maxCodes[i]) return h.vals[h.valsIndices[i] + code - h.minCodes[i]];
                code <<= 1;
            }

            throw new ImageFormatException("bad Huffman code");
        }

        private bool decodeBit()
        {
            if (this.bits.n == 0) this.ensureNBits(1);

            bool ret = (this.bits.a & this.bits.m) != 0;
            this.bits.n--;
            this.bits.m >>= 1;
            return ret;
        }

        private uint decodeBits(int n)
        {
            if (this.bits.n < n) this.ensureNBits(n);

            uint ret = this.bits.a >> (this.bits.n - n);
            ret = (uint)(ret & ((1 << n) - 1));
            this.bits.n -= n;
            this.bits.m >>= n;
            return ret;
        }

        // fill fills up the bytes.buf buffer from the underlying io.Reader. It
        // should only be called when there are no unread bytes in bytes.
        private void fill()
        {
            if (this.bytes.i != this.bytes.j) throw new ImageFormatException("jpeg: fill called when unread bytes exist");

            // Move the last 2 bytes to the start of the buffer, in case we need
            // to call unreadByteStuffedByte.
            if (this.bytes.j > 2)
            {
                this.bytes.buf[0] = this.bytes.buf[this.bytes.j - 2];
                this.bytes.buf[1] = this.bytes.buf[this.bytes.j - 1];
                this.bytes.i = 2;
                this.bytes.j = 2;
            }

            // Fill in the rest of the buffer.
            int n = this.inputStream.Read(this.bytes.buf, this.bytes.j, this.bytes.buf.Length - this.bytes.j);
            if (n == 0) throw new errEOF();
            this.bytes.j += n;
        }

        // unreadByteStuffedByte undoes the most recent readByteStuffedByte call,
        // giving a byte of data back from bits to bytes. The Huffman look-up table
        // requires at least 8 bits for look-up, which means that Huffman decoding can
        // sometimes overshoot and read one or two too many bytes. Two-byte overshoot
        // can happen when expecting to read a 0xff 0x00 byte-stuffed byte.
        private void unreadByteStuffedByte()
        {
            this.bytes.i -= this.bytes.nUnreadable;
            this.bytes.nUnreadable = 0;
            if (this.bits.n >= 8)
            {
                this.bits.a >>= 8;
                this.bits.n -= 8;
                this.bits.m >>= 8;
            }
        }

        // readByte returns the next byte, whether buffered or not buffere It does
        // not care about byte stuffing.
        private byte readByte()
        {
            while (this.bytes.i == this.bytes.j) this.fill();
            byte x = this.bytes.buf[this.bytes.i];
            this.bytes.i++;
            this.bytes.nUnreadable = 0;
            return x;
        }

        // readByteStuffedByte is like readByte but is for byte-stuffed Huffman data.
        private byte readByteStuffedByte()
        {
            byte x;

            // Take the fast path if bytes.buf contains at least two bytes.
            if (this.bytes.i + 2 <= this.bytes.j)
            {
                x = this.bytes.buf[this.bytes.i];
                this.bytes.i++;
                this.bytes.nUnreadable = 1;
                if (x != 0xff) return x;
                if (this.bytes.buf[this.bytes.i] != 0x00) throw new errMissingFF00();
                this.bytes.i++;
                this.bytes.nUnreadable = 2;
                return 0xff;
            }

            this.bytes.nUnreadable = 0;

            x = this.readByte();
            this.bytes.nUnreadable = 1;
            if (x != 0xff) return x;
            x = this.readByte();
            this.bytes.nUnreadable = 2;
            if (x != 0x00) throw new errMissingFF00();
            return 0xff;
        }

        // readFull reads exactly len(p) bytes into p. It does not care about byte
        // stuffing.
        void readFull(byte[] data, int offset, int len)
        {
            // Unread the overshot bytes, if any.
            if (this.bytes.nUnreadable != 0)
            {
                if (this.bits.n >= 8) this.unreadByteStuffedByte();
                this.bytes.nUnreadable = 0;
            }

            while (len > 0)
            {
                if (this.bytes.j - this.bytes.i >= len)
                {
                    Array.Copy(this.bytes.buf, this.bytes.i, data, offset, len);
                    this.bytes.i += len;
                    len -= len;
                }
                else
                {
                    Array.Copy(this.bytes.buf, this.bytes.i, data, offset, this.bytes.j - this.bytes.i);
                    offset += this.bytes.j - this.bytes.i;
                    len -= this.bytes.j - this.bytes.i;
                    this.bytes.i += this.bytes.j - this.bytes.i;

                    this.fill();
                }
            }
        }

        // ignore ignores the next n bytes.
        private void ignore(int n)
        {
            // Unread the overshot bytes, if any.
            if (this.bytes.nUnreadable != 0)
            {
                if (this.bits.n >= 8) this.unreadByteStuffedByte();
                this.bytes.nUnreadable = 0;
            }

            while (true)
            {
                int m = this.bytes.j - this.bytes.i;
                if (m > n) m = n;
                this.bytes.i += m;
                n -= m;
                if (n == 0) break;
                else this.fill();
            }
        }

        // Specified in section B.2.2.
        private void processSOF(int n)
        {
            if (this.nComp != 0) throw new ImageFormatException("multiple SOF markers");

            switch (n)
            {
                case 6 + 3 * 1: // Grayscale image.
                    this.nComp = 1;
                    break;
                case 6 + 3 * 3: // YCbCr or RGB image.
                    this.nComp = 3;
                    break;
                case 6 + 3 * 4: // YCbCrK or CMYK image.
                    this.nComp = 4;
                    break;
                default:
                    throw new ImageFormatException("number of components");
            }

            this.readFull(this.tmp, 0, n);

            // We only support 8-bit precision.
            if (this.tmp[0] != 8) throw new ImageFormatException("precision");

            this.height = (this.tmp[1] << 8) + this.tmp[2];
            this.width = (this.tmp[3] << 8) + this.tmp[4];
            if (this.tmp[5] != this.nComp) throw new ImageFormatException("SOF has wrong length");

            for (int i = 0; i < this.nComp; i++)
            {
                this.comp[i].c = this.tmp[6 + 3 * i];

                // Section B.2.2 states that "the value of C_i shall be different from
                // the values of C_1 through C_(i-1)".
                for (int j = 0; j < i; j++)
                {
                    if (this.comp[i].c == this.comp[j].c) throw new ImageFormatException("Repeated component identifier");
                }

                this.comp[i].tq = this.tmp[8 + 3 * i];
                if (this.comp[i].tq > maxTq) throw new ImageFormatException("Bad Tq value");

                byte hv = this.tmp[7 + 3 * i];
                int h = hv >> 4;
                int v = hv & 0x0f;
                if (h < 1 || 4 < h || v < 1 || 4 < v) throw new ImageFormatException("Unsupported Luma/chroma subsampling ratio");
                if (h == 3 || v == 3) throw new ImageFormatException("Lnsupported subsampling ratio");

                switch (this.nComp)
                {
                    case 1:

                        // If a JPEG image has only one component, section A.2 says "this data
                        // is non-interleaved by definition" and section A.2.2 says "[in this
                        // case...] the order of data units within a scan shall be left-to-right
                        // and top-to-bottom... regardless of the values of H_1 and V_1". Section
                        // 4.8.2 also says "[for non-interleaved data], the MCU is defined to be
                        // one data unit". Similarly, section A.1.1 explains that it is the ratio
                        // of H_i to max_j(H_j) that matters, and similarly for V. For grayscale
                        // images, H_1 is the maximum H_j for all components j, so that ratio is
                        // always 1. The component's (h, v) is effectively always (1, 1): even if
                        // the nominal (h, v) is (2, 1), a 20x5 image is encoded in three 8x8
                        // MCUs, not two 16x8 MCUs.
                        h = 1;
                        v = 1;
                        break;

                    case 3:

                        // For YCbCr images, we only support 4:4:4, 4:4:0, 4:2:2, 4:2:0,
                        // 4:1:1 or 4:1:0 chroma subsampling ratios. This implies that the
                        // (h, v) values for the Y component are either (1, 1), (1, 2),
                        // (2, 1), (2, 2), (4, 1) or (4, 2), and the Y component's values
                        // must be a multiple of the Cb and Cr component's values. We also
                        // assume that the two chroma components have the same subsampling
                        // ratio.
                        switch (i)
                        {
                            case 0:
                                {
                                    // Y.
                                    // We have already verified, above, that h and v are both
                                    // either 1, 2 or 4, so invalid (h, v) combinations are those
                                    // with v == 4.
                                    if (v == 4) throw new ImageFormatException("unsupported subsampling ratio");
                                    break;
                                }

                            case 1:
                                {
                                    // Cb.
                                    if (this.comp[0].h % h != 0 || this.comp[0].v % v != 0) throw new ImageFormatException("unsupported subsampling ratio");
                                    break;
                                }

                            case 2:
                                {
                                    // Cr.
                                    if (this.comp[1].h != h || this.comp[1].v != v) throw new ImageFormatException("unsupported subsampling ratio");
                                    break;
                                }
                        }

                        break;

                    case 4:

                        // For 4-component images (either CMYK or YCbCrK), we only support two
                        // hv vectors: [0x11 0x11 0x11 0x11] and [0x22 0x11 0x11 0x22].
                        // Theoretically, 4-component JPEG images could mix and match hv values
                        // but in practice, those two combinations are the only ones in use,
                        // and it simplifies the applyBlack code below if we can assume that:
                        // - for CMYK, the C and K channels have full samples, and if the M
                        // and Y channels subsample, they subsample both horizontally and
                        // vertically.
                        // - for YCbCrK, the Y and K channels have full samples.
                        switch (i)
                        {
                            case 0:
                                if (hv != 0x11 && hv != 0x22) throw new ImageFormatException("unsupported subsampling ratio");
                                break;
                            case 1:
                            case 2:
                                if (hv != 0x11) throw new ImageFormatException("unsupported subsampling ratio");
                                break;
                            case 3:
                                if (this.comp[0].h != h || this.comp[0].v != v) throw new ImageFormatException("unsupported subsampling ratio");
                                break;
                        }

                        break;
                }

                this.comp[i].h = h;
                this.comp[i].v = v;
            }
        }

        // Specified in section B.2.4.1.
        private void processDQT(int n)
        {
            while (n > 0)
            {
                bool done = false;

                n--;
                byte x = this.readByte();
                byte tq = (byte)(x & 0x0f);
                if (tq > maxTq) throw new ImageFormatException("bad Tq value");

                switch (x >> 4)
                {
                    case 0:
                        if (n < Block.BlockSize)
                        {
                            done = true;
                            break;
                        }

                        n -= Block.BlockSize;
                        this.readFull(this.tmp, 0, Block.BlockSize);

                        for (int i = 0; i < Block.BlockSize; i++) this.quant[tq][i] = this.tmp[i];
                        break;
                    case 1:
                        if (n < 2 * Block.BlockSize)
                        {
                            done = true;
                            break;
                        }

                        n -= 2 * Block.BlockSize;
                        this.readFull(this.tmp, 0, 2 * Block.BlockSize);

                        for (int i = 0; i < Block.BlockSize; i++) this.quant[tq][i] = ((int)this.tmp[2 * i] << 8) | (int)this.tmp[2 * i + 1];
                        break;
                    default:
                        throw new ImageFormatException("bad Pq value");
                }

                if (done) break;
            }

            if (n != 0) throw new ImageFormatException("DQT has wrong length");
        }

        // Specified in section B.2.4.4.
        private void processDRI(int n)
        {
            if (n != 2) throw new ImageFormatException("DRI has wrong length");

            this.readFull(this.tmp, 0, 2);
            this.ri = ((int)this.tmp[0] << 8) + (int)this.tmp[1];
        }

        private void processApp0Marker(int n)
        {
            if (n < 5)
            {
                this.ignore(n);
                return;
            }

            this.readFull(this.tmp, 0, 5);
            n -= 5;

            this.jfif = this.tmp[0] == 'J' && this.tmp[1] == 'F' && this.tmp[2] == 'I' && this.tmp[3] == 'F'
                        && this.tmp[4] == '\x00';
            if (n > 0) this.ignore(n);
        }

        private void processApp14Marker(int n)
        {
            if (n < 12)
            {
                this.ignore(n);
                return;
            }

            this.readFull(this.tmp, 0, 12);
            n -= 12;

            if (this.tmp[0] == 'A' && this.tmp[1] == 'd' && this.tmp[2] == 'o' && this.tmp[3] == 'b'
                && this.tmp[4] == 'e')
            {
                this.adobeTransformValid = true;
                this.adobeTransform = this.tmp[11];
            }

            if (n > 0) this.ignore(n);
        }

        // decode reads a JPEG image from r and returns it as an image.Image.
        public void Decode(Stream stream, ImageBase image, bool configOnly)
        {
            this.inputStream = stream;

            // Check for the Start Of Image marker.
            this.readFull(this.tmp, 0, 2);
            if (this.tmp[0] != 0xff || this.tmp[1] != soiMarker)
            {
                throw new ImageFormatException("Missing SOI marker.");
            }

            // Process the remaining segments until the End Of Image marker.
            while (true)
            {
                this.readFull(this.tmp, 0, 2);
                while (this.tmp[0] != 0xff)
                {
                    // Strictly speaking, this is a format error. However, libjpeg is
                    // liberal in what it accepts. As of version 9, next_marker in
                    // jdmarker.c treats this as a warning (JWRN_EXTRANEOUS_DATA) and
                    // continues to decode the stream. Even before next_marker sees
                    // extraneous data, jpeg_fill_bit_buffer in jdhuff.c reads as many
                    // bytes as it can, possibly past the end of a scan's data. It
                    // effectively puts back any markers that it overscanned (e.g. an
                    // "\xff\xd9" EOI marker), but it does not put back non-marker data,
                    // and thus it can silently ignore a small number of extraneous
                    // non-marker bytes before next_marker has a chance to see them (and
                    // print a warning).
                    // We are therefore also liberal in what we accept. Extraneous data
                    // is silently ignore
                    // This is similar to, but not exactly the same as, the restart
                    // mechanism within a scan (the RST[0-7] markers).
                    // Note that extraneous 0xff bytes in e.g. SOS data are escaped as
                    // "\xff\x00", and so are detected a little further down below.
                    this.tmp[0] = this.tmp[1];
                    this.tmp[1] = this.readByte();
                }

                byte marker = this.tmp[1];
                if (marker == 0)
                {
                    // Treat "\xff\x00" as extraneous data.
                    continue;
                }

                while (marker == 0xff)
                {
                    // Section B.1.1.2 says, "Any marker may optionally be preceded by any
                    // number of fill bytes, which are bytes assigned code X'FF'".
                    marker = this.readByte();
                }

                // End Of Image.
                if (marker == eoiMarker)
                {
                    break;
                }

                if (rst0Marker <= marker && marker <= rst7Marker)
                {
                    // Figures B.2 and B.16 of the specification suggest that restart markers should
                    // only occur between Entropy Coded Segments and not after the final ECS.
                    // However, some encoders may generate incorrect JPEGs with a final restart
                    // marker. That restart marker will be seen here instead of inside the processSOS
                    // method, and is ignored as a harmless error. Restart markers have no extra data,
                    // so we check for this before we read the 16-bit length of the segment.
                    continue;
                }

                // Read the 16-bit length of the segment. The value includes the 2 bytes for the
                // length itself, so we subtract 2 to get the number of remaining bytes.
                this.readFull(this.tmp, 0, 2);
                int n = ((int)this.tmp[0] << 8) + (int)this.tmp[1] - 2;
                if (n < 0)
                {
                    throw new ImageFormatException("short segment length");
                }

                switch (marker)
                {
                    case JpegConstants.Markers.SOF0:
                    case JpegConstants.Markers.SOF1:
                    case JpegConstants.Markers.SOF2:
                        this.progressive = marker == JpegConstants.Markers.SOF2;
                        this.processSOF(n);
                        if (configOnly && this.jfif)
                        {
                            return;
                        }

                        break;
                    case dhtMarker:
                        if (configOnly) this.ignore(n);
                        else this.processDHT(n);
                        break;
                    case dqtMarker:
                        if (configOnly) this.ignore(n);
                        else this.processDQT(n);
                        break;
                    case sosMarker:
                        if (configOnly)
                        {
                            return;
                        }

                        this.ProcessSOS(n);
                        break;
                    case driMarker:
                        if (configOnly) this.ignore(n);
                        else this.processDRI(n);
                        break;
                    case app0Marker:
                        this.processApp0Marker(n);
                        break;
                    case app14Marker:
                        this.processApp14Marker(n);
                        break;
                    default:
                        if (app0Marker <= marker && marker <= app15Marker || marker == comMarker)
                        {
                            this.ignore(n);
                        }
                        else if (marker < 0xc0) // See Table B.1 "Marker code assignments".
                            throw new ImageFormatException("Unknown marker");
                        else throw new ImageFormatException("Unknown marker");
                        break;
                }
            }

            if (this.img1 != null)
            {
                return;
            }

            if (this.img3 != null)
            {
                if (this.IsRGB())
                {
                    this.ConvertDirectToRGB(this.width, this.height, image);
                }
                else
                {
                    this.ConvertToRGB(this.width, this.height, image);
                }
            }
            else
            {
                throw new ImageFormatException("Missing SOS marker.");
            }
        }

        private void ConvertToRGB(int width, int height, ImageBase image)
        {
            int cScale = this.comp[0].h / this.comp[1].h;

            float[] pixels = new float[width * height * 4];

            Parallel.For(
                0, 
                height, 
                y =>
                    {
                        int yo = this.img3.get_row_y_offset(y);
                        int co = this.img3.get_row_c_offset(y);

                        for (int x = 0; x < width; x++)
                        {
                            byte yy = this.img3.pix_y[yo + x];
                            byte cb = this.img3.pix_cb[co + (x / cScale)];
                            byte cr = this.img3.pix_cr[co + (x / cScale)];

                            int index = ((y * width) + x) * 4;

                            // Implicit casting FTW
                            Color color = new YCbCr(yy, cb, cr);
                            pixels[index] = color.R;
                            pixels[index + 1] = color.G;
                            pixels[index + 2] = color.B;
                            pixels[index + 3] = color.A;
                        }
                    });

            image.SetPixels(width, height, pixels);
        }

        private void ConvertDirectToRGB(int width, int height, ImageBase image)
        {
            int cScale = this.comp[0].h / this.comp[1].h;
            float[] pixels = new float[width * height * 4];

            Parallel.For(
                0, 
                height, 
                y =>
                    {
                        int yo = this.img3.get_row_y_offset(y);
                        int co = this.img3.get_row_c_offset(y);

                        for (int x = 0; x < width; x++)
                        {
                            byte red = this.img3.pix_y[yo + x];
                            byte green = this.img3.pix_cb[co + x / cScale];
                            byte blue = this.img3.pix_cr[co + x / cScale];

                            int index = ((y * width) + x) * 4;
                            Color color = new Bgra32(red, green, blue);
                            pixels[index] = color.R;
                            pixels[index + 1] = color.G;
                            pixels[index + 2] = color.B;
                            pixels[index + 3] = color.A;
                        }
                    });

            image.SetPixels(width, height, pixels);
        }

        private struct Scan
        {
            public byte compIndex;

            public byte td;

            public byte ta;
        }

        /// <summary>
        /// Processes the SOS (Start of scan marker).
        /// </summary>
        /// <remarks>
        /// TODO: This is throwing an error when decoding progressive images taken from 
        /// Facebook. The second segment is returning 1 component (first is 3) this is 
        /// throwing an error on line 1208.
        /// </remarks>
        /// <remarks>
        /// TODO: This also needs some significant refactoring to follow a more OO format.
        /// </remarks>
        /// <param name="n">
        /// The first byte of the current image marker.
        /// </param>
        /// <exception cref="ImageFormatException">
        /// Missing SOF Marker
        /// SOS has wrong length
        /// </exception>
        private void ProcessSOS(int n)
        {
            if (this.nComp == 0)
            {
                throw new ImageFormatException("missing SOF marker");
            }

            if (n < 6 || 4 + (2 * this.nComp) < n || n % 2 != 0)
            {
                throw new ImageFormatException("SOS has wrong length");
            }

            this.readFull(this.tmp, 0, n);
            byte lnComp = this.tmp[0];

            if (n != 4 + 2 * lnComp)
            {
                throw new ImageFormatException("SOS length inconsistent with number of components");
            }

            Scan[] scan = new Scan[maxComponents];
            int totalHV = 0;

            for (int i = 0; i < lnComp; i++)
            {
                // Component selector.
                int cs = this.tmp[1 + (2 * i)];
                int compIndex = -1;
                for (int j = 0; j < this.nComp; j++)
                {
                    Component compv = this.comp[j];
                    if (cs == compv.c)
                    {
                        compIndex = j;
                    }
                }

                if (compIndex < 0)
                {
                    throw new ImageFormatException("Unknown component selector");
                }

                scan[i].compIndex = (byte)compIndex;

                // Section B.2.3 states that "the value of Cs_j shall be different from
                // the values of Cs_1 through Cs_(j-1)". Since we have previously
                // verified that a frame's component identifiers (C_i values in section
                // B.2.2) are unique, it suffices to check that the implicit indexes
                // into comp are unique.
                for (int j = 0; j < i; j++)
                {
                    if (scan[i].compIndex == scan[j].compIndex)
                    {
                        throw new ImageFormatException("Repeated component selector");
                    }
                }

                totalHV += this.comp[compIndex].h * this.comp[compIndex].v;

                scan[i].td = (byte)(this.tmp[2 + (2 * i)] >> 4);
                if (scan[i].td > maxTh)
                {
                    throw new ImageFormatException("bad Td value");
                }

                scan[i].ta = (byte)(this.tmp[2 + (2 * i)] & 0x0f);
                if (scan[i].ta > maxTh)
                {
                    throw new ImageFormatException("bad Ta value");
                }
            }

            // Section B.2.3 states that if there is more than one component then the
            // total H*V values in a scan must be <= 10.
            if (this.nComp > 1 && totalHV > 10)
            {
                throw new ImageFormatException("Total sampling factors too large.");
            }

            // zigStart and zigEnd are the spectral selection bounds.
            // ah and al are the successive approximation high and low values.
            // The spec calls these values Ss, Se, Ah and Al.
            // For progressive JPEGs, these are the two more-or-less independent
            // aspects of progression. Spectral selection progression is when not
            // all of a block's 64 DCT coefficients are transmitted in one pass.
            // For example, three passes could transmit coefficient 0 (the DC
            // component), coefficients 1-5, and coefficients 6-63, in zig-zag
            // order. Successive approximation is when not all of the bits of a
            // band of coefficients are transmitted in one pass. For example,
            // three passes could transmit the 6 most significant bits, followed
            // by the second-least significant bit, followed by the least
            // significant bit.
            // For baseline JPEGs, these parameters are hard-coded to 0/63/0/0.
            int zigStart = 0;
            int zigEnd = Block.BlockSize - 1;
            int ah = 0;
            int al = 0;

            if (this.progressive)
            {
                zigStart = (int)this.tmp[1 + (2 * lnComp)];
                zigEnd = (int)this.tmp[2 + (2 * lnComp)];
                ah = (int)(this.tmp[3 + (2 * lnComp)] >> 4);
                al = (int)(this.tmp[3 + (2 * lnComp)] & 0x0f);

                if ((zigStart == 0 && zigEnd != 0) || zigStart > zigEnd || Block.BlockSize <= zigEnd)
                {
                    throw new ImageFormatException("bad spectral selection bounds");
                }

                if (zigStart != 0 && lnComp != 1)
                {
                    throw new ImageFormatException("progressive AC coefficients for more than one component");
                }

                if (ah != 0 && ah != al + 1)
                {
                    throw new ImageFormatException("bad successive approximation values");
                }
            }

            // mxx and myy are the number of MCUs (Minimum Coded Units) in the image.
            int h0 = this.comp[0].h;
            int v0 = this.comp[0].v;
            int mxx = (this.width + (8 * h0) - 1) / (8 * h0);
            int myy = (this.height + (8 * v0) - 1) / (8 * v0);

            if (this.img1 == null && this.img3 == null)
            {
                this.makeImg(mxx, myy);
            }

            if (this.progressive)
            {
                for (int i = 0; i < lnComp; i++)
                {
                    int compIndex = scan[i].compIndex;
                    if (this.progCoeffs[compIndex] == null)
                    {
                        this.progCoeffs[compIndex] =
                            new Block[mxx * myy * this.comp[compIndex].h * this.comp[compIndex].v];

                        for (int j = 0; j < this.progCoeffs[compIndex].Length; j++)
                        {
                            this.progCoeffs[compIndex][j] = new Block();
                        }
                    }
                }
            }

            this.bits = new bits_class();

            int mcu = 0;
            byte expectedRST = rst0Marker;

            // b is the decoded coefficients, in natural (not zig-zag) order.
            Block b = new Block();
            int[] dc = new int[maxComponents];

            // bx and by are the location of the current block, in units of 8x8
            // blocks: the third block in the first row has (bx, by) = (2, 0).
            int bx, by, blockCount = 0;

            for (int my = 0; my < myy; my++)
            {
                for (int mx = 0; mx < mxx; mx++)
                {
                    for (int i = 0; i < lnComp; i++)
                    {
                        int compIndex = scan[i].compIndex;
                        int hi = this.comp[compIndex].h;
                        int vi = this.comp[compIndex].v;
                        Block qt = this.quant[this.comp[compIndex].tq];

                        for (int j = 0; j < hi * vi; j++)
                        {
                            // The blocks are traversed one MCU at a time. For 4:2:0 chroma
                            // subsampling, there are four Y 8x8 blocks in every 16x16 MCU.
                            // For a baseline 32x16 pixel image, the Y blocks visiting order is:
                            // 0 1 4 5
                            // 2 3 6 7
                            // For progressive images, the interleaved scans (those with nComp > 1)
                            // are traversed as above, but non-interleaved scans are traversed left
                            // to right, top to bottom:
                            // 0 1 2 3
                            // 4 5 6 7
                            // Only DC scans (zigStart == 0) can be interleave AC scans must have
                            // only one component.
                            // To further complicate matters, for non-interleaved scans, there is no
                            // data for any blocks that are inside the image at the MCU level but
                            // outside the image at the pixel level. For example, a 24x16 pixel 4:2:0
                            // progressive image consists of two 16x16 MCUs. The interleaved scans
                            // will process 8 Y blocks:
                            // 0 1 4 5
                            // 2 3 6 7
                            // The non-interleaved scans will process only 6 Y blocks:
                            // 0 1 2
                            // 3 4 5
                            if (lnComp != 1)
                            {
                                bx = hi * mx + j % hi;
                                by = vi * my + j / hi;
                            }
                            else
                            {
                                int q = mxx * hi;
                                bx = blockCount % q;
                                by = blockCount / q;
                                blockCount++;
                                if (bx * 8 >= this.width || by * 8 >= this.height) continue;
                            }

                            // Load the previous partially decoded coefficients, if applicable.
                            if (this.progressive) b = this.progCoeffs[compIndex][by * mxx * hi + bx];
                            else b = new Block();

                            if (ah != 0)
                            {
                                this.refine(b, this.huff[acTable, scan[i].ta], zigStart, zigEnd, 1 << al);
                            }
                            else
                            {
                                int zig = zigStart;
                                if (zig == 0)
                                {
                                    zig++;

                                    // Decode the DC coefficient, as specified in section F.2.2.1.
                                    byte value = this.decodeHuffman(this.huff[dcTable, scan[i].td]);
                                    if (value > 16)
                                    {
                                        throw new ImageFormatException("Excessive DC component");
                                    }

                                    int dcDelta = this.receiveExtend(value);
                                    dc[compIndex] += dcDelta;
                                    b[0] = dc[compIndex] << al;
                                }

                                if (zig <= zigEnd && this.eobRun > 0)
                                {
                                    this.eobRun--;
                                }
                                else
                                {
                                    // Decode the AC coefficients, as specified in section F.2.2.2.
                                    var huffv = this.huff[acTable, scan[i].ta];
                                    for (; zig <= zigEnd; zig++)
                                    {
                                        byte value = this.decodeHuffman(huffv);
                                        byte val0 = (byte)(value >> 4);
                                        byte val1 = (byte)(value & 0x0f);
                                        if (val1 != 0)
                                        {
                                            zig += val0;
                                            if (zig > zigEnd) break;

                                            var ac = this.receiveExtend(val1);
                                            b[unzig[zig]] = ac << al;
                                        }
                                        else
                                        {
                                            if (val0 != 0x0f)
                                            {
                                                this.eobRun = (ushort)(1 << val0);
                                                if (val0 != 0)
                                                {
                                                    this.eobRun |= (ushort)this.decodeBits(val0);
                                                }

                                                this.eobRun--;
                                                break;
                                            }

                                            zig += 0x0f;
                                        }
                                    }
                                }
                            }

                            if (this.progressive)
                            {
                                if (zigEnd != Block.BlockSize - 1 || al != 0)
                                {
                                    // We haven't completely decoded this 8x8 block. Save the coefficients.
                                    this.progCoeffs[compIndex][by * mxx * hi + bx] = b;

                                    // At this point, we could execute the rest of the loop body to dequantize and
                                    // perform the inverse DCT, to save early stages of a progressive image to the
                                    // *image.YCbCr buffers (the whole point of progressive encoding), but in Go,
                                    // the jpeg.Decode function does not return until the entire image is decoded,
                                    // so we "continue" here to avoid wasted computation.
                                    continue;
                                }
                            }

                            // Dequantize, perform the inverse DCT and store the block to the image.
                            for (int zig = 0; zig < Block.BlockSize; zig++) b[unzig[zig]] *= qt[zig];

                            IDCT.Transform(b);

                            byte[] dst = null;
                            int offset = 0;
                            int stride = 0;

                            if (this.nComp == 1)
                            {
                                dst = this.img1.pixels;
                                stride = this.img1.stride;
                                offset = this.img1.offset + 8 * (by * this.img1.stride + bx);
                            }
                            else
                            {
                                switch (compIndex)
                                {
                                    case 0:
                                        dst = this.img3.pix_y;
                                        stride = this.img3.y_stride;
                                        offset = this.img3.y_offset + 8 * (by * this.img3.y_stride + bx);
                                        break;

                                    case 1:
                                        dst = this.img3.pix_cb;
                                        stride = this.img3.c_stride;
                                        offset = this.img3.c_offset + 8 * (by * this.img3.c_stride + bx);
                                        break;

                                    case 2:
                                        dst = this.img3.pix_cr;
                                        stride = this.img3.c_stride;
                                        offset = this.img3.c_offset + 8 * (by * this.img3.c_stride + bx);
                                        break;

                                    case 3:

                                        // dst, stride = blackPix[8*(by*blackStride+bx):], blackStride
                                        // break;
                                        // TODO: Check this.
                                        throw new ImageFormatException("Too many components");

                                    default:
                                        throw new ImageFormatException("Too many components");
                                }
                            }

                            // Level shift by +128, clip to [0, 255], and write to dst.
                            for (int y = 0; y < 8; y++)
                            {
                                int y8 = y * 8;
                                int yStride = y * stride;

                                for (int x = 0; x < 8; x++)
                                {
                                    int c = b[y8 + x];
                                    if (c < -128)
                                    {
                                        c = 0;
                                    }
                                    else if (c > 127)
                                    {
                                        c = 255;
                                    }
                                    else
                                    {
                                        c += 128;
                                    }

                                    // TODO: I think we can cut out all the double looping by
                                    // assigning the color here.
                                    dst[yStride + x + offset] = (byte)c;
                                }
                            }
                        }
 // for j
                    }
 // for i

                    mcu++;

                    if (this.ri > 0 && mcu % this.ri == 0 && mcu < mxx * myy)
                    {
                        // A more sophisticated decoder could use RST[0-7] markers to resynchronize from corrupt input,
                        // but this one assumes well-formed input, and hence the restart marker follows immediately.
                        this.readFull(this.tmp, 0, 2);
                        if (this.tmp[0] != 0xff || this.tmp[1] != expectedRST)
                        {
                            throw new ImageFormatException("Bad RST marker");
                        }

                        expectedRST++;
                        if (expectedRST == rst7Marker + 1)
                        {
                            expectedRST = rst0Marker;
                        }

                        // Reset the Huffman decoder.
                        this.bits = new bits_class();

                        // Reset the DC components, as per section F.2.1.3.1.
                        dc = new int[maxComponents];

                        // Reset the progressive decoder state, as per section G.1.2.2.
                        this.eobRun = 0;
                    }
                }
 // for mx
            }
 // for my
        }

        // refine decodes a successive approximation refinement block, as specified in
        // section G.1.2.
        private void refine(Block b, huffman_class h, int zigStart, int zigEnd, int delta)
        {
            // Refining a DC component is trivial.
            if (zigStart == 0)
            {
                if (zigEnd != 0) throw new ImageFormatException("Invalid state for zig DC component");

                bool bit = this.decodeBit();
                if (bit) b[0] |= delta;

                return;
            }

            // Refining AC components is more complicated; see sections G.1.2.2 and G.1.2.3.
            int zig = zigStart;
            if (this.eobRun == 0)
            {
                for (; zig <= zigEnd; zig++)
                {
                    bool done = false;
                    int z = 0;
                    var val = this.decodeHuffman(h);
                    int val0 = val >> 4;
                    int val1 = val & 0x0f;

                    switch (val1)
                    {
                        case 0:
                            if (val0 != 0x0f)
                            {
                                this.eobRun = (ushort)(1 << val0);
                                if (val0 != 0)
                                {
                                    uint bits = this.decodeBits(val0);
                                    this.eobRun |= (ushort)bits;
                                }

                                done = true;
                            }

                            break;
                        case 1:
                            z = delta;
                            bool bit = this.decodeBit();
                            if (!bit) z = -z;
                            break;
                        default:
                            throw new ImageFormatException("unexpected Huffman code");
                    }

                    if (done) break;

                    zig = this.refineNonZeroes(b, zig, zigEnd, val0, delta);
                    if (zig > zigEnd) throw new ImageFormatException(string.Format("too many coefficients {0} > {1}", zig, zigEnd));

                    if (z != 0) b[unzig[zig]] = z;
                }
            }

            if (this.eobRun > 0)
            {
                this.eobRun--;
                this.refineNonZeroes(b, zig, zigEnd, -1, delta);
            }
        }

        // refineNonZeroes refines non-zero entries of b in zig-zag order. If nz >= 0,
        // the first nz zero entries are skipped over.
        public int refineNonZeroes(Block b, int zig, int zigEnd, int nz, int delta)
        {
            for (; zig <= zigEnd; zig++)
            {
                int u = unzig[zig];
                if (b[u] == 0)
                {
                    if (nz == 0) break;
                    nz--;
                    continue;
                }

                bool bit = this.decodeBit();
                if (!bit) continue;

                if (b[u] >= 0) b[u] += delta;
                else b[u] -= delta;
            }

            return zig;
        }

        private void makeImg(int mxx, int myy)
        {
            if (this.nComp == 1)
            {
                var m = new img_gray(8 * mxx, 8 * myy);
                this.img1 = m.subimage(0, 0, this.width, this.height);
            }
            else
            {
                var h0 = this.comp[0].h;
                var v0 = this.comp[0].v;
                var hRatio = h0 / this.comp[1].h;
                var vRatio = v0 / this.comp[1].v;

                var ratio = img_ycbcr.YCbCrSubsampleRatio.YCbCrSubsampleRatio444;
                switch ((hRatio << 4) | vRatio)
                {
                    case 0x11:
                        ratio = img_ycbcr.YCbCrSubsampleRatio.YCbCrSubsampleRatio444;
                        break;
                    case 0x12:
                        ratio = img_ycbcr.YCbCrSubsampleRatio.YCbCrSubsampleRatio440;
                        break;
                    case 0x21:
                        ratio = img_ycbcr.YCbCrSubsampleRatio.YCbCrSubsampleRatio422;
                        break;
                    case 0x22:
                        ratio = img_ycbcr.YCbCrSubsampleRatio.YCbCrSubsampleRatio420;
                        break;
                    case 0x41:
                        ratio = img_ycbcr.YCbCrSubsampleRatio.YCbCrSubsampleRatio411;
                        break;
                    case 0x42:
                        ratio = img_ycbcr.YCbCrSubsampleRatio.YCbCrSubsampleRatio410;
                        break;
                }

                var m = new img_ycbcr(8 * h0 * mxx, 8 * v0 * myy, ratio);
                this.img3 = m.subimage(0, 0, this.width, this.height);

                /*if d.nComp == 4 {
                    h3, v3 := d.comp[3].h, d.comp[3].v
                    d.blackPix = make([]byte, 8*h3*mxx*8*v3*myy)
                    d.blackStride = 8 * h3 * mxx
                }*/
            }
        }

        private bool IsRGB()
        {
            if (this.jfif)
            {
                return false;
            }

            if (this.adobeTransformValid && this.adobeTransform == adobeTransformUnknown)
            {
                // http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
                // says that 0 means Unknown (and in practice RGB) and 1 means YCbCr.
                return true;
            }

            return this.comp[0].c == 'R' && this.comp[1].c == 'G' && this.comp[2].c == 'B';
        }
    }
}
