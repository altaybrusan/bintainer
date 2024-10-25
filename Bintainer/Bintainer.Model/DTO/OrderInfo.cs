using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.DTO
{
    public class OrderInfo
    {
        public string? OrderNumber { get; set; }
        public string Supplier { get; set; } = null!;
        public DateTime? OrderDate { get; set; }
        public DateTime? HandOverDate { get; set; }
        public List<PartSummary> Parts { get; set; }
    }
}
