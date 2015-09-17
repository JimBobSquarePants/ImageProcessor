// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestBigEndianBitConverter.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Tests the big endian bit converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.UnitTests.Metadata
{
    using ImageProcessor.Imaging.Helpers;

    using NUnit.Framework;

    /// <summary>
    /// Tests the big endian bit converter.
    /// </summary>
    [TestFixture]
    public class TestBigEndianBitConverter
    {
        [Test]
        public void GetBytesShort()
        {
            this.CheckBytes(new byte[] { 0, 0 }, EndianBitConverter.Big.GetBytes((short)0));
            this.CheckBytes(new byte[] { 0, 1 }, EndianBitConverter.Big.GetBytes((short)1));
            this.CheckBytes(new byte[] { 1, 0 }, EndianBitConverter.Big.GetBytes((short)256));
            this.CheckBytes(new byte[] { 255, 255 }, EndianBitConverter.Big.GetBytes((short)-1));
            this.CheckBytes(new byte[] { 1, 1 }, EndianBitConverter.Big.GetBytes((short)257));
        }

        [Test]
        public void GetBytesUShort()
        {
            this.CheckBytes(new byte[] { 0, 0 }, EndianBitConverter.Big.GetBytes((ushort)0));
            this.CheckBytes(new byte[] { 0, 1 }, EndianBitConverter.Big.GetBytes((ushort)1));
            this.CheckBytes(new byte[] { 1, 0 }, EndianBitConverter.Big.GetBytes((ushort)256));
            this.CheckBytes(new byte[] { 255, 255 }, EndianBitConverter.Big.GetBytes((ushort)ushort.MaxValue));
            this.CheckBytes(new byte[] { 1, 1 }, EndianBitConverter.Big.GetBytes((ushort)257));
        }

        [Test]
        public void GetBytesInt()
        {
            this.CheckBytes(new byte[] { 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes((int)0));
            this.CheckBytes(new byte[] { 0, 0, 0, 1 }, EndianBitConverter.Big.GetBytes((int)1));
            this.CheckBytes(new byte[] { 0, 0, 1, 0 }, EndianBitConverter.Big.GetBytes((int)256));
            this.CheckBytes(new byte[] { 0, 1, 0, 0 }, EndianBitConverter.Big.GetBytes((int)65536));
            this.CheckBytes(new byte[] { 1, 0, 0, 0 }, EndianBitConverter.Big.GetBytes((int)16777216));
            this.CheckBytes(new byte[] { 255, 255, 255, 255 }, EndianBitConverter.Big.GetBytes((int)-1));
            this.CheckBytes(new byte[] { 0, 0, 1, 1 }, EndianBitConverter.Big.GetBytes((int)257));
        }

        [Test]
        public void GetBytesUInt()
        {
            this.CheckBytes(new byte[] { 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes((uint)0));
            this.CheckBytes(new byte[] { 0, 0, 0, 1 }, EndianBitConverter.Big.GetBytes((uint)1));
            this.CheckBytes(new byte[] { 0, 0, 1, 0 }, EndianBitConverter.Big.GetBytes((uint)256));
            this.CheckBytes(new byte[] { 0, 1, 0, 0 }, EndianBitConverter.Big.GetBytes((uint)65536));
            this.CheckBytes(new byte[] { 1, 0, 0, 0 }, EndianBitConverter.Big.GetBytes((uint)16777216));
            this.CheckBytes(new byte[] { 255, 255, 255, 255 }, EndianBitConverter.Big.GetBytes((uint)uint.MaxValue));
            this.CheckBytes(new byte[] { 0, 0, 1, 1 }, EndianBitConverter.Big.GetBytes((uint)257));
        }

        [Test]
        public void GetBytesLong()
        {
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(0L));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, EndianBitConverter.Big.GetBytes(1L));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, EndianBitConverter.Big.GetBytes(256L));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, EndianBitConverter.Big.GetBytes(65536L));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(16777216L));
            this.CheckBytes(new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(4294967296L));
            this.CheckBytes(new byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(1099511627776L));
            this.CheckBytes(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(1099511627776L * 256));
            this.CheckBytes(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(1099511627776L * 256 * 256));
            this.CheckBytes(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, EndianBitConverter.Big.GetBytes(-1L));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 0, 1, 1 }, EndianBitConverter.Big.GetBytes(257L));
        }

        [Test]
        public void GetBytesULong()
        {
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(0UL));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, EndianBitConverter.Big.GetBytes(1UL));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, EndianBitConverter.Big.GetBytes(256UL));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, EndianBitConverter.Big.GetBytes(65536UL));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(16777216UL));
            this.CheckBytes(new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(4294967296UL));
            this.CheckBytes(new byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(1099511627776UL));
            this.CheckBytes(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(1099511627776UL * 256));
            this.CheckBytes(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, EndianBitConverter.Big.GetBytes(1099511627776UL * 256 * 256));
            this.CheckBytes(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, EndianBitConverter.Big.GetBytes(ulong.MaxValue));
            this.CheckBytes(new byte[] { 0, 0, 0, 0, 0, 0, 1, 1 }, EndianBitConverter.Big.GetBytes(257UL));
        }

        private void CheckBytes(byte[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length, "Lengths should match");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
