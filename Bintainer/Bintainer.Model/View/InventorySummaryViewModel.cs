using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.View
{
    public class InventorySummaryViewModel
    {
        public string? PartNumber { get; set; }
        public string? Section { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
    }
}
