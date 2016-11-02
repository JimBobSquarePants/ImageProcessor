// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryParamParserUnitTests.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The query parameter parser unit tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.UnitTests
{
    using NUnit.Framework;

    /// <summary>
    /// Url decoding tests
    /// </summary>
    [TestFixture]
    public class UrlParserUnitTests
    {
        [Test]
        [TestCase("http://www.mydomain.com/image.gif")]
        [TestCase("http://www.mydomain.com/image.jpg?a=qwerty&b=123456")]
        [TestCase("http://www.mydomain.com/?a=qwerty&b=123456")]
        [TestCase("http://www.mydomain.com/some+path/?a=qwerty&b=123&comment=some+comment+with+spaces")]
        public void TestUrlUnencdoded(string url)
        {
            //Passed url should not have been changed
            Assert.True(Helpers.UrlDecoder.Instance.DecodeUrl(url).Equals(url));
        }

        [Test]
        [TestCase("http://www.mydomain.com/?url=http%3A%2F%2Fwww.myotherdomain.com%2Fmedia%2F1566%2Fimage.jpg%3Fanchor%3Dcenter%26mode%3Dcrop%26width%3D1024%26rnd%3D130921477360000000")]
        public void TestUrlWithEncodedQuerystring(string url)
        {
            //Passed url should have its query decoded resulting in an invalid url
            Assert.False(Helpers.UrlDecoder.Instance.DecodeUrl(url).Equals(url));
        }

        [Test]
        [TestCase("http%3A%2F%2Fwww.mydomain.com%2Fsome%2Bpath%2F%3Fa%3Dqwerty%26b%3D123%26comment%3Dsome%2Bcomment%2Bwith%2Bspaces", "http://www.mydomain.com/some+path/?a=qwerty&b=123&comment=some+comment+with+spaces")]
        public void TestUrlEncodedWithQuerystring(string url, string expectedResult)
        {
            //Passed encoded url should be decoded
            Assert.True(Helpers.UrlDecoder.Instance.DecodeUrl(url).Equals(expectedResult));
        }

        [Test]
        [TestCase("http%3A%2F%2Fwww.mydomain.com%2F%3Furl%3Dhttp%253A%252F%252Fwww.myotherdomain.com%252Fmedia%252F1566%252Fimage.jpg%253Fanchor%253Dcenter%2526mode%253Dcrop%2526width%253D1024%2526rnd%253D130921477360000000", "http://www.mydomain.com/?url=http%3A%2F%2Fwww.myotherdomain.com%2Fmedia%2F1566%2Fimage.jpg%3Fanchor%3Dcenter%26mode%3Dcrop%26width%3D1024%26rnd%3D130921477360000000")]
        public void TestUrlEncodedWithEncodedQuerystring(string url, string expectedResult)
        {
            //Passed encoded url should be decoded but maintain its encoded query
            Assert.True(Helpers.UrlDecoder.Instance.DecodeUrl(url).Equals(expectedResult));
        }
    }
}
