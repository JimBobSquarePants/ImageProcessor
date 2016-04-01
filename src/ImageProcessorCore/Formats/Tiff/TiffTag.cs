namespace ImageProcessorCore.Formats
{

    public class TiffTag
    {
        public ushort TagId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string TagGroup { get; set; }
    }
}
