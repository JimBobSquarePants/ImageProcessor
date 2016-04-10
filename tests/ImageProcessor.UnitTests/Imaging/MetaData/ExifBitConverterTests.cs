// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExifBitConverterTests.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The exif bit converter tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.UnitTests.Imaging.MetaData
{
    using ImageProcessor.Imaging;
    using ImageProcessor.Imaging.MetaData;

    using NUnit.Framework;

    /// <summary>
    /// The exif bit converter tests.
    /// </summary>
    public class ExifBitConverterTests
    {
        /// <summary>
        /// The when get bytes.
        /// </summary>
        [TestFixture]
        public class WhenGetBytes
        {
            /// <summary>
            /// The then should return bytes plus null given computer architecture is little endian and add null true.
            /// </summary>
            [Test]
            public void ThenShouldReturnBytesPlusNullGivenComputerArchitectureIsLittleEndianAndAddNullTrue()
            {
                // Arrange
                var converter = new ExifBitConverter(new LittleEndianComputerArchitectureInfoFake());

                // Act
                var bytes = converter.GetBytes("Hello", true);

                // Assert
                Assert.That(bytes, Is.EqualTo(new[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x0 }));
            }

            /// <summary>
            /// The then should return reversed bytes beginning with null given computer architecture is big endian and add null true.
            /// </summary>
            [Test]
            public void ThenShouldReturnReversedBytesBeginningWithNullGivenComputerArchitectureIsBigEndianAndAddNullTrue()
            {
                // Arrange
                var converter = new ExifBitConverter(new BigEndianComputerArchitectureInfoFake());

                // Act
                var bytes = converter.GetBytes("Hello", true);

                // Assert
                Assert.That(bytes, Is.EqualTo(new[] { 0x0, 0x6f, 0x6c, 0x6c, 0x65, 0x48 }));
            }

            /// <summary>
            /// The then should return bytes given computer architecture is little endian.
            /// </summary>
            [Test]
            public void ThenShouldReturnBytesGivenComputerArchitectureIsLittleEndian()
            {
                // Arrange
                var converter = new ExifBitConverter(new LittleEndianComputerArchitectureInfoFake());

                // Act
                var bytes = converter.GetBytes("Hello", false);

                // Assert
                Assert.That(bytes, Is.EqualTo(new[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f }));
            }

            /// <summary>
            /// The then should reverse byte array given computer architecture is big endian.
            /// </summary>
            [Test]
            public void ThenShouldReverseByteArrayGivenComputerArchitectureIsBigEndian()
            {
                // Arrange
                var converter = new ExifBitConverter(new BigEndianComputerArchitectureInfoFake());

                // Act
                var bytes = converter.GetBytes("Hello", false);

                // Assert
                Assert.That(bytes, Is.EqualTo(new[] { 0x6f, 0x6c, 0x6c, 0x65, 0x48 }));
            }

            /// <summary>
            /// The little endian computer architecture info fake.
            /// </summary>
            internal class LittleEndianComputerArchitectureInfoFake : IComputerArchitectureInfo
            {
                /// <summary>
                /// The is little endian.
                /// </summary>
                /// <returns>
                /// The false for fake little endian
                /// </returns>
                public bool IsLittleEndian()
                {
                    return true;
                }
            }

            /// <summary>
            /// The big endian computer architecture info fake.
            /// </summary>
            internal class BigEndianComputerArchitectureInfoFake : IComputerArchitectureInfo
            {
                /// <summary>
                /// The is little endian.
                /// </summary>
                /// <returns>
                /// The true for fake big endian
                /// </returns>
                public bool IsLittleEndian()
                {
                    return false;
                }
            }
        } 
    }
}