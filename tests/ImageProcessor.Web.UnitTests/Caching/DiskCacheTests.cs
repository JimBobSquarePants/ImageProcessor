using System;
using System.IO;
using ImageProcessor.Web.Caching;
using NUnit.Framework;

namespace ImageProcessor.Web.UnitTests.Caching
{
    [TestFixture]
    public class DiskCacheTests
    {
        [Test]
        public void GetValidatedAbsolutePath_Virtual_In_WebRoot()
        {
            var absPath = DiskCache.GetValidatedCachePathsImpl("~/App_Data/TEMP/IP", this.TestMapPath, this.GetDirectoryInfo, out var virtualCachePath);

            Assert.AreEqual(@"X:\Sites\MySite\App_Data\TEMP\IP", absPath);
            Assert.AreEqual("~/App_Data/TEMP/IP", virtualCachePath);
        }

        [Test]
        public void GetValidatedAbsolutePath_Virtual_Outside_WebRoot()
        {
            var absPath = DiskCache.GetValidatedCachePathsImpl("~/../OUTSIDE", this.TestMapPath, this.GetDirectoryInfo, out var virtualCachePath);

            Assert.AreEqual(@"X:\Sites\OUTSIDE", absPath);
            Assert.AreEqual(null, virtualCachePath);
        }

        [Test]
        public void GetValidatedAbsolutePath_Absolute_In_WebRoot()
        {
            var absPath = DiskCache.GetValidatedCachePathsImpl(@"X:\Sites\MySite\App_Data\TEMP\IP", this.TestMapPath, this.GetDirectoryInfo, out var virtualCachePath);

            Assert.AreEqual(@"X:\Sites\MySite\App_Data\TEMP\IP", absPath);
            Assert.AreEqual("~/App_Data/TEMP/IP", virtualCachePath);
        }

        [Test]
        public void GetValidatedAbsolutePath_Absolute_Outside_WebRoot()
        {
            var absPath = DiskCache.GetValidatedCachePathsImpl(@"X:\Sites\OUTSIDE", this.TestMapPath, this.GetDirectoryInfo, out var virtualCachePath);

            Assert.AreEqual(@"X:\Sites\OUTSIDE", absPath);
            Assert.AreEqual(null, virtualCachePath);
        }

        private FileSystemInfo GetDirectoryInfo(string path)
        {
            return new TestDirectoryInfo(path);
        }

        private class TestDirectoryInfo : FileSystemInfo
        {
            public TestDirectoryInfo(string path)
            {
                this._dirInfo = new DirectoryInfo(path);
            }

            private readonly DirectoryInfo _dirInfo;

            public override string FullName => this._dirInfo.FullName;

            public override void Delete()
            {
                throw new NotImplementedException();
            }

            public override string Name => this._dirInfo.Name;

            /// <summary>
            /// Always return true for these tests
            /// </summary>
            public override bool Exists => true;
        }

        private string TestMapPath(string path)
        {
            var root = "X:/Sites/MySite/";
            return path.Replace("~/", root).Replace("/", @"\");
        }

    }
}

