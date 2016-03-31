using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessorCore.Formats.Tiff
{

    public class TiffTag
    {
        public ushort TagId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string TagGroup { get; set; }
    }
}
