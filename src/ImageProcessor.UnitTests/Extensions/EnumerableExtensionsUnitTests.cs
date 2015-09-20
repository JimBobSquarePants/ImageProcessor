// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensionsUnitTests.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Defines the EnumerableExtensionsUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;

    using ImageProcessor.Common.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// The enumerable extensions unit tests.
    /// </summary>
    public class EnumerableExtensionsUnitTests
    {
        /// <summary>
        /// The when stepped range.
        /// </summary>
        [TestFixture]
        public class WhenSteppedRange
        {
            /// <summary>
            /// The then should return 1 through 9 numbers given 1 then 10 then 1.
            /// </summary>
            [Test]
            public void ThenShouldReturn1Through9NumbersGiven1Then10Then1()
            {
                // Arrange // Act
                var enumerable = EnumerableExtensions.SteppedRange(1, 10, 1);

                // Assert
                Assert.That(enumerable, Is.EquivalentTo(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            }

            /// <summary>
            /// The then should return negative 10 through negative 1 given between range negative 10 then 1 then 1.
            /// </summary>
            [Test]
            public void ThenShouldReturnNegative10Through0GivenBetweenRangeNegative10Then1Then1()
            {
                // Arrange // Act
                var enumerable = EnumerableExtensions.SteppedRange(-10, 1, 1);

                // Assert
                Assert.That(enumerable, Is.EquivalentTo(new List<int> { 0, -1, -2, -3, -4, -5, -6, -7, -8, -9, -10 }));
            }

            /// <summary>
            /// The then should return 1 then 3 then 5 then 7 then 9 given between range 1 then 10 then 2.
            /// </summary>
            [Test]
            public void ThenShouldReturn1Then3Then5Then7Then9GivenBetweenRange1Then10Then2()
            {
                // Arrange // Act
                var enumerable = EnumerableExtensions.SteppedRange(1, 10, 2);

                // Assert
                Assert.That(enumerable, Is.EquivalentTo(new List<int> { 1, 3, 5, 7, 9 }));
            }

            /// <summary>
            /// The then should throw argument out of range exception given to exclusive less than 0.
            /// </summary>
            [Test]
            public void ThenShouldThrowArgumentOutOfRangeExceptionGivenToExclusiveLessThan0()
            {
                // Arrange // Act // Assert
                Assert.Throws<ArgumentOutOfRangeException>(() => EnumerableExtensions.SteppedRange(0, -9, -3));
            }
        }

        /// <summary>
        /// The when stepped range with function.
        /// </summary>
        [TestFixture]
        public class WhenSteppedRangeWithFunction
        {
            /// <summary>
            /// The then return 0 to 9 given function 0 then i less than 10 then 1.
            /// </summary>
            [Test]
            public void ThenReturn0To9GivenFunction0ThenILessThan10Then1()
            {
                // Arrange // Act
                var enumerable = EnumerableExtensions.SteppedRange(0, i => i < 10, 1);

                // Assert
                Assert.That(enumerable, Is.EquivalentTo(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            }
        }
    }
}