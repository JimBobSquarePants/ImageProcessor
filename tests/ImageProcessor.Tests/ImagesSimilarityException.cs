using System;

namespace ImageProcessor.Tests
{
    public class ImagesSimilarityException : Exception
    {
        public ImagesSimilarityException(string message)
            : base(message)
        {
        }
    }
}
