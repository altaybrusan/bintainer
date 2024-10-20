using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.DTO
{
    public class SearchOrderRequestModel
    {
        public string? OrderNumber { get; set; }
        public string? Supplier { get; set; }
        public DateTime? FromDate { get; set; } = null;
        public DateTime? ToDate { get; set; } = null;
    }
}
