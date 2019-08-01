using System.IO;

namespace ImageProcessor.Tests
{
    public static class TestFiles
    {
        public static class Jpeg
        {
            public static TestFile EXIFCropIssue559 = TestUtils.GetTestFileByName("exif-crop-issue-559.jfif");
            public static TestFile Penguins = TestUtils.GetTestFileByName("format-Penguins.jpg");
        }

        public static class Png
        {
            public static TestFile Penguins = TestUtils.GetTestFileByName("format-Penguins.png");
        }

        public static class Gif
        {
            public static TestFile AnimatedPattern = TestUtils.GetTestFileByName("animated-pattern.gif");
            public static TestFile AnimatedZivan = TestUtils.GetTestFileByName("animated-zivan.gif");
            public static TestFile Penguins = TestUtils.GetTestFileByName("format-Penguins.gif");
        }

        public static class Bmp
        {
            public static TestFile Penguins = TestUtils.GetTestFileByName("format-Penguins.bmp");
        }

        public static class Tiff
        {
            public static TestFile Penguins = TestUtils.GetTestFileByName("format-Penguins.tif");
        }

        public static class WebP
        {
            public static TestFile Penguins = TestUtils.GetTestFileByName("format-Penguins.webp");
        }
    }

    public class TestFile
    {
        public TestFile(FileInfo info, string expectedRoot, string actualRoot)
        {
            this.FullName = info.FullName;
            this.Name = Path.GetFileName(info.Name);
            this.Extension = Path.GetExtension(info.Extension);
            this.ExpectedRoot = expectedRoot;
            this.ActualRoot = actualRoot;
        }

        public string Name { get; }

        public string FullName { get; }

        public string Extension { get; }

        public string ExpectedRoot { get; }

        public string ActualRoot { get; }
    }
}
