namespace ImageProcessor.Web.UnitTests
{
    using System.Drawing;

    using ImageProcessor.Web.Helpers;

    using NUnit.Framework;

    [TestFixture]
    public class ExtendedColorTypeConverterTests
    {
        [TestCase("#0000ff")]
        [TestCase("#00f")]
        [TestCase("#ff0000ff")]
        [TestCase("blue")]
        public void ExtendedColorTypeConverterParsesColors(string input)
        {
            ExtendedColorTypeConverter converter = new ExtendedColorTypeConverter();
            Color color = (Color)converter.ConvertFrom(input);
            Assert.AreEqual(color.ToArgb(), Color.Blue.ToArgb());
        }
    }
}
