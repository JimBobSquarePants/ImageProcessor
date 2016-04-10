// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtendedColorTypeConverterTests.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Contains tests for the ExtendedColorTypeConverter class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.UnitTests
{
    using System.Drawing;

    using ImageProcessor.Web.Helpers;

    using NUnit.Framework;

    /// <summary>
    /// The extended color type converter tests.
    /// </summary>
    [TestFixture]
    public class ExtendedColorTypeConverterTests
    {
        /// <summary>
        /// Tests the ExtendedColorTypeConverter returns the correct
        /// values for a range of different formats.
        /// </summary>
        /// <param name="input">
        /// The input color code.
        /// </param>
        [TestCase("#0000ff")]
        [TestCase("#00f")]
        [TestCase("#ff0000ff")]
        [TestCase("blue")]
        public void ExtendedColorTypeConverterParsesColors(string input)
        {
            ColorTypeConverter converter = new ColorTypeConverter();
            object convertFrom = converter.ConvertFrom(null, input, typeof(Color));
            if (convertFrom != null)
            {
                Color color = (Color)convertFrom;
                Assert.AreEqual(Color.Blue.ToArgb(), color.ToArgb());
            }
        }
    }
}
