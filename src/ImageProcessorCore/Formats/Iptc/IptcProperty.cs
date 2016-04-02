namespace ImageProcessorCore.Formats
{
    public class IptcProperty
    {
        public IptcTag Tag { get; set; }

        public object Value { get; set; }


        public override string ToString()
        {
            return base.ToString();
        }
    }
}
