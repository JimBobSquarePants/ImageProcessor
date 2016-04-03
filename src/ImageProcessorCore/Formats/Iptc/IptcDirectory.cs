using System.Collections.Generic;
using ImageProcessorCore.Formats.Tiff;

namespace ImageProcessorCore.Formats
{
    internal class IptcDirectory : ITiffAcceptor
    {
        public List<string> Errors { get; private set; }
        public List<IptcProperty> Properties { get; private set; }
        public IptcDirectory()
        {
            Properties = new List<IptcProperty>();
            Errors = new List<string>();
        }


        public void Accept(ITiffVisitor visitor)
        {
            foreach (IptcProperty property in Properties)
            {
                visitor.Visit(property);
            }
        }

    }
}
