// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetaTests.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The meta tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.UnitTests.Processors
{
    using System.Management.Instrumentation;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Processors;

    using NUnit.Framework;

    /// <summary>
    /// The meta tests.
    /// </summary>
    public class MetaTests
    {
        /// <summary>
        /// The when constructing meta.
        /// </summary>
        [TestFixture]
        public class WhenConstructingMeta
        {
            /// <summary>
            /// The then_should_initialize_settings.
            /// </summary>
            [Test]
            public void ThenShouldInitializeSettings()
            {
                // Arrange
                var meta = new Meta();

                // Act //Assert
                Assert.That(meta.Settings, Is.Not.Null);
            }
        }

        /// <summary>
        /// The when_process_image.
        /// </summary>
        [TestFixture]
        public class WhenProcessImage
        {
            /// <summary>
            /// The then should throw image processing exception given non boolean dynamic parameter.
            /// </summary>
            [Test]
            public void ThenShouldThrowImageProcessingExceptionGivenNonBooleanDynamicParameter()
            {
                // Arrange
                var meta = new Meta { DynamicParameter = new { Something = "dynamic" } };

                // Act // Assert
                Assert.Throws<ImageProcessingException>(() => meta.ProcessImage(new ImageFactory()));
            }

        }
    }
}