using ImageProcessor.Web.Caching;

namespace ImageProcessor.Web.UnitTests.Helpers
{
    using NUnit.Framework;

    /// <summary>
    /// Cached image helper tests
    /// </summary>
    [TestFixture]
    public class CachedImageHelperTests
    {
        [Test]
        [TestCase("/media/image.gif?", "", "13c47ee938933b64f863cc374f9fa042fad93672.gif")]
        [TestCase("/media/image.gif?width=100&height=100", "width=100&height=100", "f6401f01522b35bf50e37550abdf75d3fb7c4f79.gif")]
        [TestCase("/media/image.gif?width=100&height=100", "", "f6401f01522b35bf50e37550abdf75d3fb7c4f79.gif")]
        [TestCase("/handler/123456?width=100&height=100", "width=100&height=100", "09b68b2b8b78f425706fe73bb58b1dc211759473.jpg")]
        [TestCase("/handler/123456", "", "d45c279def9be4585a9e01925f4f7607ffe5114e.jpg")]
        [TestCase("/some+path+with+plus/q_w-e+rty.jpeg?width=100&height=100&format=png", "width=100&height=100&format=png", "8df90f8b3bd2547bf1cb8a8828afb35b59434fea.png")]
        [TestCase("/remote.axd/maps.googleapis.com/maps/api/staticmap?center=Albany,+NY&zoom=13&scale=false&size=800x500&maptype=roadmap&sensor=false&format=png&visual_refresh=true?width=401&format=bmp", "width=401&format=bmp", "7785eac6e1607f0222977f2845f7fd1ccfaebb94.bmp")]
        [TestCase("/remote.axd/maps.googleapis.com/maps/api/staticmap?center=Albany,+NY&zoom=13&scale=false&size=800x500&maptype=roadmap&sensor=false&format=png&visual_refresh=true?width=401&format=bmp", "", "7785eac6e1607f0222977f2845f7fd1ccfaebb94.bmp")]
        [TestCase("/remote.axd?http://maps.googleapis.com/maps/api/staticmap?center=Albany,+NY&zoom=13&scale=false&size=800x500&maptype=roadmap&sensor=false&format=png&visual_refresh=true?width=401&format=bmp", "width=401&format=bmp", "97d47d5d73ba4814f57434567c4141d37547ca11.bmp")]
        [TestCase("/remote.axd?http://maps.googleapis.com/maps/api/staticmap?center=Albany,+NY&zoom=13&scale=false&size=800x500&maptype=roadmap&sensor=false&format=png&visual_refresh=true?width=401&format=bmp", "", "97d47d5d73ba4814f57434567c4141d37547ca11.bmp")]

        public void TestCachedFileNameGenerated(string path, string query, string expected)
        {
            string result1 = CachedImageHelper.GetCachedImageFileName(path);
            string result2 = CachedImageHelper.GetCachedImageFileName(path, query);

            Assert.AreEqual(result1, result2);
            Assert.AreEqual(result1, expected);
        }

        [Test]
        [TestCase("~/App_Data/cache", "13c47ee938933b64f863cc374f9fa042fad93672.gif", true, 6, "~/App_Data/cache/1/3/c/4/7/e/13c47ee938933b64f863cc374f9fa042fad93672.gif")]
        [TestCase("~/App_Data/cache", "13c47ee938933b64f863cc374f9fa042fad93672.gif", true, 3, "~/App_Data/cache/1/3/c/13c47ee938933b64f863cc374f9fa042fad93672.gif")]
        [TestCase("~/App_Data/cache", "13c47ee938933b64f863cc374f9fa042fad93672.gif", true, 0, "~/App_Data/cache/13c47ee938933b64f863cc374f9fa042fad93672.gif")]
        [TestCase("X:\\Projects\\ImageProcessor\\ImageProcessor\\tests\\OUTSIDE", "13c47ee938933b64f863cc374f9fa042fad93672.gif", false, 6, "X:\\Projects\\ImageProcessor\\ImageProcessor\\tests\\OUTSIDE\\1\\3\\c\\4\\7\\e\\13c47ee938933b64f863cc374f9fa042fad93672.gif")]
        [TestCase("X:\\Projects\\ImageProcessor\\ImageProcessor\\tests\\OUTSIDE", "13c47ee938933b64f863cc374f9fa042fad93672.gif", false, 3, "X:\\Projects\\ImageProcessor\\ImageProcessor\\tests\\OUTSIDE\\1\\3\\c\\13c47ee938933b64f863cc374f9fa042fad93672.gif")]
        [TestCase("X:\\Projects\\ImageProcessor\\ImageProcessor\\tests\\OUTSIDE", "13c47ee938933b64f863cc374f9fa042fad93672.gif", false, 0, "X:\\Projects\\ImageProcessor\\ImageProcessor\\tests\\OUTSIDE\\13c47ee938933b64f863cc374f9fa042fad93672.gif")]

        public void TestCachedFilePathGenerated(string path, string filename, bool makeVirtual, int depth, string expected)
        {
            string result = CachedImageHelper.GetCachedPath(path, filename, makeVirtual, depth);
            Assert.AreEqual(result, expected);
        }
    }
}