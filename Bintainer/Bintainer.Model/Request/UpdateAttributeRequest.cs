using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.Request
{
    public class UpdateAttributeRequest
    {
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public string PartName { get; set; } = string.Empty;
    }
}
