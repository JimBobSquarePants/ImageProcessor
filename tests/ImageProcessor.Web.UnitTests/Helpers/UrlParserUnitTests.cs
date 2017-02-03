// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryParamParserUnitTests.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The query parameter parser unit tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.UnitTests.Helpers
{
    using NUnit.Framework;

    /// <summary>
    /// Url decoding tests
    /// </summary>
    [TestFixture]
    public class UrlParserUnitTests
    {
        [Test]
        [TestCase("/media/image.gif", "width=100&height=100")]
        [TestCase("/handler/123456", "width=100&height=100")]
        [TestCase("/some+path+with+plus/q_w-e+rty.jpeg", "width=100&height=100")]
        public void TestLocalUrl(string url, string query)
        {
            string requestPath, queryString;
            Web.Helpers.UrlParser.ParseUrl(url + "?" + query, "", out requestPath, out queryString);
            Assert.True(requestPath.Equals(url));
            Assert.True(queryString.Equals(query));
        }

        [Test]
        [TestCase("/", "remote.axd", "http://www.remotedomain.com/image.gif", "width=100&height=100")]
        [TestCase("/", "remote.axd", "http://www.remotedomain.com/image.jpg?a=qwerty&b=123456", "width=100&height=100")]
        [TestCase("/", "remote.axd", "http://www.remotedomain.com/?a=qwerty&b=123456", "width=100&height=100")]
        [TestCase("/", "remote.axd", "http://www.remotedomain.com/some+path/?a=qwerty&b=123&comment=some+comment+with+spaces", "width=100&height=100")]
        public void TestRemoteUrlUnencdoded(string baseUrl, string prefix, string path, string query)
        {
            //Passed url should not have been changed
            string requestPath, queryString;

            //Test url
            string url = baseUrl + "/" + prefix + "?" + path + "?" + query;
            Web.Helpers.UrlParser.ParseUrl(url, prefix, out requestPath, out queryString);
            Assert.True(queryString.Equals(query));
            Assert.True(requestPath.Equals(path));

            //Test non legacy url
            path = path.Substring(7);
            string nonLegacyUrl = baseUrl + "/" + prefix + "/" + path + "?" + query;
            Web.Helpers.UrlParser.ParseUrl(nonLegacyUrl, prefix, out requestPath, out queryString);
            Assert.True(queryString.Equals(query));
            Assert.True(requestPath.TrimStart('/').Equals(path));
        }

        [Test]
        [TestCase("/", "remote.axd", "http://www.remotedomain.com/?url=http%3A%2F%2Fwww.myotherdomain.com%2Fmedia%2F1566%2Fimage.jpg%3Fanchor%3Dcenter%26mode%3Dcrop%26width%3D1024%26rnd%3D130921477360000000", "width=100&height=100")]
        public void TestRemoteUrlWithEncodedQuerystring(string baseUrl, string prefix, string path, string query)
        {
            //Requestpath should be urldecoded resulting in an invalid requestpath
            string requestPath, queryString;

            // Test legacy url
            string legacyUrl = baseUrl + "/" + prefix + "?" + path + "?" + query;
            Web.Helpers.UrlParser.ParseUrl(legacyUrl, prefix, out requestPath, out queryString);
            Assert.True(queryString.Equals(query));
            Assert.True(requestPath.Equals(path));

            // Test non legacy url
            path = path.Substring(7);
            string nonLegacyUrl = baseUrl + "/" + prefix + "/" + path + "?" + query;
            Web.Helpers.UrlParser.ParseUrl(nonLegacyUrl, prefix, out requestPath, out queryString);
            Assert.True(queryString.Equals(query));
            Assert.True(requestPath.TrimStart('/').Equals(path));
        }

        [Test]
        [TestCase("/", "remote.axd", "http%3A%2F%2Fwww.remotedomain.com%2Fsome%2Bpath%2F%3Fa%3Dqwerty%26b%3D123%26comment%3Dsome%2Bcomment%2Bwith%2Bspaces", "width=100&height=100", "http://www.remotedomain.com/some+path/?a=qwerty&b=123&comment=some+comment+with+spaces")]
        public void TestRemoteUrlEncodedWithQuerystring(string baseUrl, string prefix, string path, string query, string expectedPath)
        {
            // Requestpath should be urldecoded resulting in an invalid requestpath
            string requestPath, queryString;

            // Test legacy url
            string legacyUrl = baseUrl + "/" + prefix + "?" + path + "?" + query;
            Web.Helpers.UrlParser.ParseUrl(legacyUrl, prefix, out requestPath, out queryString);
            Assert.True(queryString.Equals(query));
            Assert.True(requestPath.Equals(expectedPath));

            // This will not work with non legacy url since the protocol is encoded
        }

        [Test]
        [TestCase("/", "remote.axd", "http%3A%2F%2Fwww.remotedomain.com%2F%3Furl%3Dhttp%253A%252F%252Fwww.myotherdomain.com%252Fmedia%252F1566%252Fimage.jpg%253Fanchor%253Dcenter%2526mode%253Dcrop%2526width%253D1024%2526rnd%253D130921477360000000", "width=100&height=100", "http://www.remotedomain.com/?url=http%3A%2F%2Fwww.myotherdomain.com%2Fmedia%2F1566%2Fimage.jpg%3Fanchor%3Dcenter%26mode%3Dcrop%26width%3D1024%26rnd%3D130921477360000000")]
        public void TestRemoteUrlEncodedWithEncodedQuerystring(string baseUrl, string prefix, string path, string query, string expectedPath)
        {
            //Requestpath should be urldecoded resulting in an invalid requestpath
            string requestPath, queryString;

            //Test legacy url
            string legacyUrl = baseUrl + "/" + prefix + "?" + path + "?" + query;
            Web.Helpers.UrlParser.ParseUrl(legacyUrl, prefix, out requestPath, out queryString);
            Assert.True(queryString.Equals(query));
            Assert.True(requestPath.Equals(expectedPath));

            //This will not work with non legacy url since the protocol is encoded
        }

        [Test]
        [TestCase("/", "remote.axd", "https%3A%2F%2Fi.scdn.co%2Fimage%2F8851b36688b767d01a00c7d1f0981997db0062d2%3Fwidth=512&startcolor=aad774&endcolor=ac2833", "https://i.scdn.co/image/8851b36688b767d01a00c7d1f0981997db0062d2", "width=512&startcolor=aad774&endcolor=ac2833")]
        public void TestRemoteUrlCompletelyEncoded(string baseUrl, string prefix, string path, string expectedPath, string expectedQuery)
        {
            string requestPath, queryString;

            //Test legacy url
            string legacyUrl = baseUrl + "/" + prefix + "?" + path;
            Web.Helpers.UrlParser.ParseUrl(legacyUrl, prefix, out requestPath, out queryString);
            Assert.True(queryString.Equals(expectedQuery));
            Assert.True(requestPath.Equals(expectedPath));

            //This will not work with non legacy url since the protocol is encoded
        }
    }
}
