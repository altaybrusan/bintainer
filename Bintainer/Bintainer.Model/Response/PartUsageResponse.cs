using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.Response
{
    public class PartUsageResponse
    {
        public int PartId { get; set; }
        public string? PartName { get; set; }
        public string? Section { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public string? SubspaceIndices { get; set; }
        public int Quantity { get; set; }
        public string? Label { get; set; }
        public int BinId { get; set; }
    }
}
