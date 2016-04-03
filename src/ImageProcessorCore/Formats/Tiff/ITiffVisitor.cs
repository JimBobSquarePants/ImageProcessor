namespace ImageProcessorCore.Formats
{
    interface ITiffVisitor
    {
        void Visit(TiffProperty property);

        void Visit(TiffDirectory directory);

        void Visit(IptcProperty property);
    }
}
