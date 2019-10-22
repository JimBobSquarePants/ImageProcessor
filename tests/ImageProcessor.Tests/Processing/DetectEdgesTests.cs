using System;
using System.Collections.Generic;
using ImageProcessor.Processing;
using Xunit;

namespace ImageProcessor.Tests.Processing
{
    public class DetectEdgesTests
    {
        private const string category = "DetectEdges";

        public static IEnumerable<object[]> EdgeDetectionOperatorsData()
        {
            foreach (object value in Enum.GetValues(typeof(EdgeDetectionOperators)))
            {
                yield return new object[] { value };
            }
        }

        [Theory]
        [MemberData(nameof(EdgeDetectionOperatorsData))]
        public void FactoryCanDetectEdges(EdgeDetectionOperators mode)
        {
            TestFile file = TestFiles.Png.Penguins;
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .DetectEdges(mode)
                       .SaveAndCompare(file, category, mode);
            }
        }

        [Theory]
        [MemberData(nameof(EdgeDetectionOperatorsData))]
        public void FactoryCanDetectEdgesWithColor(EdgeDetectionOperators mode)
        {
            TestFile file = TestFiles.Png.Penguins;
            using (var factory = new ImageFactory())
            {
                factory.Load(file.FullName)
                       .DetectEdges(mode, false)
                       .SaveAndCompare(file, category, mode, "color");
            }
        }
    }
}
