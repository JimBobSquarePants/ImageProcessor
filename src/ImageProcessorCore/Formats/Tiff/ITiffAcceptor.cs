namespace ImageProcessorCore.Formats.Tiff
{
    internal interface ITiffAcceptor
    {
        void Accept(ITiffVisitor visitor);
    }
}
