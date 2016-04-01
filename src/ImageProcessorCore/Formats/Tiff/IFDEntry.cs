namespace ImageProcessorCore.Formats
{
    public class IFDEntry
    {
        public TiffReaderSnapshot EntrySnapshot { get; set; }

        public  byte[] OffsetBuffer { get; set; }

        public ushort TagId { get; set; }

        public IFDEntryType FieldType { get; set; }

        public int ValueCount { get; set; }

        public int ValueOffset { get; set; }

    }
}
