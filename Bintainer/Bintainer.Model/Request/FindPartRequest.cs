using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.Request
{
    public class FindPartRequest
    {
            public string? SearchedPartNumber { get; set; }
            public string? SearchedPartCategory { get; set; }
            public string? SearchedPartGroup { get; set; }
    }
}
