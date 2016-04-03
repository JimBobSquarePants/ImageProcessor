using System.Collections.Generic;

namespace ImageProcessorCore.Formats
{
    public class IptcDirectory
    {
        public List<string> Errors { get; private set; }
        public List<IptcProperty> Properties { get; private set; }
        public IptcDirectory()
        {
            Properties = new List<IptcProperty>();
            Errors = new List<string>();
        }
        
    }
}
