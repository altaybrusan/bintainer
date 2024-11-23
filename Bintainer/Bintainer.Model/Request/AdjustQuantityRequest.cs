using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.Request
{
    public class AdjustQuantityRequest
    {
        public int QuantityUsed { get; set; }
        public int BinId { get; set; }
        public string? PartNumber { get; set; }
        public int Quantity { get; set; }
        public string? SubspaceIndices { get; set; }
    }
}
