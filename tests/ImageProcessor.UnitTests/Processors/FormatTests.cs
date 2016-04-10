// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormatTests.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Defines the FormatTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.UnitTests.Processors
{
    using System.Collections.Generic;

    using ImageProcessor.Processors;

    using NUnit.Framework;

    /// <summary>
    /// The format tests.
    /// </summary>
    public class FormatTests
    {
        /// <summary>
        /// The when initialize format.
        /// </summary>
        [TestFixture]
        public class WhenInitializeFormat
        {
            /// <summary>
            /// The then should set settings to dictionary.
            /// </summary>
            [Test]
            public void ThenShouldSetSettingsToDictionary()
            {
                // Arrange // Act
                var format = new Format();

                // Assert
                Assert.That(format.Settings, Is.TypeOf<Dictionary<string, string>>().And.Not.Null);
            }
        } 
    }
}