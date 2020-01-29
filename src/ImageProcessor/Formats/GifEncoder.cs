// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using ImageProcessor.Quantizers;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Encodes multiple images as an animated gif to a stream.
    /// <remarks>
    /// Uses default .NET GIF encoding and adds animation headers.
    /// Adapted from <see href="http://github.com/DataDink/Bumpkit/blob/master/BumpKit/BumpKit/GifEncoder.cs"/>.
    /// </remarks>
    /// </summary>
    public class GifEncoder
    {
        /// <summary>
        /// The application block size.
        /// </summary>
        private const byte ApplicationBlockSize = 0x0b;

        /// <summary>
        /// The application extension block identifier.
        /// </summary>
        private const int ApplicationExtensionBlockIdentifier = 0xFF21;

        /// <summary>
        /// The file trailer.
        /// </summary>
        private const byte FileTrailer = 0x3b;

        /// <summary>
        /// The graphic control extension block identifier.
        /// </summary>
        private const int GraphicControlExtensionBlockIdentifier = 0xF921;

        /// <summary>
        /// The graphic control extension block size.
        /// </summary>
        private const byte GraphicControlExtensionBlockSize = 0x04;

        /// <summary>
        /// The source color block length.
        /// </summary>
        private const int SourceColorBlockLength = 768;

        /// <summary>
        /// The source color block position.
        /// </summary>
        private const int SourceColorBlockPosition = 13;

        /// <summary>
        /// The source global color info position.
        /// </summary>
        private const int SourceGlobalColorInfoPosition = 10;

        /// <summary>
        /// The source graphic control extension length.
        /// </summary>
        private const int SourceGraphicControlExtensionLength = 8;

        /// <summary>
        /// The source graphic control extension position.
        /// </summary>
        private const int SourceGraphicControlExtensionPosition = 781;

        /// <summary>
        /// The source image block header length.
        /// </summary>
        private const int SourceImageBlockHeaderLength = 11;

        /// <summary>
        /// The source image block position.
        /// </summary>
        private const int SourceImageBlockPosition = 789;

        /// <summary>
        /// The application identification.
        /// </summary>
        private static readonly byte[] ApplicationIdentification = Encoding.ASCII.GetBytes("NETSCAPE2.0");

        /// <summary>
        /// The file type and version.
        /// </summary>
        private static readonly byte[] FileType = Encoding.ASCII.GetBytes("GIF89a");

        /// <summary>
        /// The stream.
        /// </summary>
        private readonly MemoryStream imageStream;

        /// <summary>
        /// The repeat count.
        /// </summary>
        private readonly int repeatCount;

        /// <summary>
        /// Whether the gif has has the last terminated byte written.
        /// </summary>
        private bool terminated;

        /// <summary>
        /// Whether this is first image frame.
        /// </summary>
        private bool isFirstImageFrame = true;

        /// <summary>
        /// The quantizer for reducing the palette.
        /// </summary>
        private readonly IQuantizer quantizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GifEncoder"/> class.
        /// </summary>
        /// <param name="quantizer">The quantizer used to reduce and index the images pixels.</param>
        /// <param name="repeatCount">
        /// The number of times to repeat the animation.
        /// </param>
        public GifEncoder(IQuantizer quantizer, int repeatCount)
        {
            this.quantizer = quantizer;
            this.imageStream = new MemoryStream();
            this.repeatCount = repeatCount;
        }

        /// <summary>
        /// Encodes the image frame to the output gif.
        /// </summary>
        /// <param name="frame">The <see cref="GifFrame"/> containing the image.</param>
        public void EncodeFrame(GifFrame frame)
        {
            // We need to quantize the image.
            Image image = this.quantizer.Quantize(frame.Image);
            using (var gifStream = new MemoryStream())
            {
                image.Save(gifStream, ImageFormat.Gif);
                if (this.isFirstImageFrame)
                {
                    // Steal the global color table info
                    this.WriteHeaderBlock(gifStream, image.Width, image.Height);
                }

                this.WriteGraphicControlBlock(gifStream, Convert.ToInt32(frame.Delay.TotalMilliseconds / 10F));
                this.WriteImageBlock(gifStream, !this.isFirstImageFrame, frame.X, frame.Y, image.Width, image.Height);
            }

            this.isFirstImageFrame = false;
        }

        /// <summary>
        /// Encodes the completed gif to an <see cref="Image"/>.
        /// </summary>
        /// <returns>The completed animated gif.</returns>
        public Image Encode()
        {
            this.Terminate();

            // Push the data
            this.imageStream.Position = 0;
            return Image.FromStream(this.imageStream);
        }

        /// <summary>
        /// Encodes the completed gif to an <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void EncodeToStream(Stream stream)
        {
            this.Terminate();

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            // Push the data
            this.imageStream.Position = 0;
            this.imageStream.CopyTo(stream);
        }

        /// <summary>
        /// Writes the termination marker to the image stream.
        /// </summary>
        private void Terminate()
        {
            if (!this.terminated)
            {
                // Complete File
                this.WriteByte(FileTrailer);
                this.terminated = true;
            }
        }

        /// <summary>
        /// Writes the header block of the animated gif to the stream.
        /// </summary>
        /// <param name="sourceGif">The source gif.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        private void WriteHeaderBlock(Stream sourceGif, int width, int height)
        {
            // File Header signature and version.
            this.imageStream.Write(FileType, 0, FileType.Length);

            // Write the logical screen descriptor.
            this.WriteShort(width); // Initial Logical Width
            this.WriteShort(height); // Initial Logical Height

            // Read the global color table info.
            sourceGif.Position = SourceGlobalColorInfoPosition;
            this.WriteByte(sourceGif.ReadByte());

            this.WriteByte(255); // Background Color Index
            this.WriteByte(0); // Pixel aspect ratio
            this.WriteColorTable(sourceGif);

            // Application Extension Header
            int count = this.repeatCount;
            if (count != 1)
            {
                // 0 means loop indefinitely. count is set as play n + 1 times.
                count = Math.Max(0, count - 1);
                this.WriteShort(ApplicationExtensionBlockIdentifier);
                this.WriteByte(ApplicationBlockSize);

                this.imageStream.Write(ApplicationIdentification, 0, ApplicationIdentification.Length);
                this.WriteByte(3); // Application block length
                this.WriteByte(1);
                this.WriteShort(count); // Repeat count for images.

                this.WriteByte(0); // Terminator
            }
        }

        /// <summary>
        /// Writes the given integer as a byte to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        private void WriteByte(int value) => this.imageStream.WriteByte(Convert.ToByte(value));

        /// <summary>
        /// Writes the color table.
        /// </summary>
        /// <param name="sourceGif">The source gif.</param>
        private void WriteColorTable(Stream sourceGif)
        {
            sourceGif.Position = SourceColorBlockPosition; // Locating the image color table

            byte[] colorTable = ArrayPool<byte>.Shared.Rent(SourceColorBlockLength);

            try
            {
                sourceGif.Read(colorTable, 0, SourceColorBlockLength);
                this.imageStream.Write(colorTable, 0, SourceColorBlockLength);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(colorTable);
            }
        }

        /// <summary>
        /// Writes graphic control block.
        /// </summary>
        /// <param name="gifStream">The source gif.</param>
        /// <param name="frameDelay">The frame delay.</param>
        private void WriteGraphicControlBlock(Stream gifStream, int frameDelay)
        {
            gifStream.Position = SourceGraphicControlExtensionPosition; // Locating the source GCE
            Span<byte> blockhead = stackalloc byte[SourceGraphicControlExtensionLength];
            gifStream.Read(blockhead); // Reading source GCE

            this.WriteShort(GraphicControlExtensionBlockIdentifier); // Identifier
            this.WriteByte(GraphicControlExtensionBlockSize); // Block Size
            this.WriteByte((blockhead[3] & 0xF7) | 0x08); // Setting disposal flag
            this.WriteShort(frameDelay); // Setting frame delay
            this.WriteByte(255); // Transparent color index
            this.WriteByte(0); // Terminator
        }

        /// <summary>
        /// Writes the image block data.
        /// </summary>
        /// <param name="gifStream">The source gif.</param>
        /// <param name="includeColorTable">The include color table.</param>
        /// <param name="x">The x position to write the image block.</param>
        /// <param name="y">The y position to write the image block.</param>
        /// <param name="h">The height of the image block.</param>
        /// <param name="w">The width of the image block.</param>
        private void WriteImageBlock(Stream gifStream, bool includeColorTable, int x, int y, int h, int w)
        {
            // Local Image Descriptor
            gifStream.Position = SourceImageBlockPosition; // Locating the image block
            Span<byte> header = stackalloc byte[SourceImageBlockHeaderLength];
            gifStream.Read(header);
            this.WriteByte(header[0]); // Separator
            this.WriteShort(x); // Position X
            this.WriteShort(y); // Position Y
            this.WriteShort(h); // Height
            this.WriteShort(w); // Width

            if (includeColorTable)
            {
                // If first frame, use global color table - else use local
                gifStream.Position = SourceGlobalColorInfoPosition;
                this.WriteByte((gifStream.ReadByte() & 0x3F) | 0x80); // Enabling local color table
                this.WriteColorTable(gifStream);
            }
            else
            {
                this.WriteByte((header[9] & 0x07) | 0x07); // Disabling local color table
            }

            this.WriteByte(header[10]); // LZW Min Code Size

            // Read/Write image data
            gifStream.Position = SourceImageBlockPosition + SourceImageBlockHeaderLength;

            int dataLength = gifStream.ReadByte();
            while (dataLength > 0)
            {
                byte[] imgData = ArrayPool<byte>.Shared.Rent(dataLength);

                try
                {
                    gifStream.Read(imgData, 0, dataLength);

                    this.imageStream.WriteByte(Convert.ToByte(dataLength));
                    this.imageStream.Write(imgData, 0, dataLength);
                    dataLength = gifStream.ReadByte();
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(imgData);
                }
            }

            this.imageStream.WriteByte(0); // Terminator
        }

        /// <summary>
        /// Writes a short to the image stream.
        /// </summary>
        /// <param name="value">The value.</param>
        private void WriteShort(int value)
        {
            // Leave only one significant byte.
            this.imageStream.WriteByte(Convert.ToByte(value & 0xFF));
            this.imageStream.WriteByte(Convert.ToByte((value >> 8) & 0xFF));
        }
    }
}
