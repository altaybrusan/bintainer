using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.Request
{
    public class CreateAttributeTemplateRequest
    {
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public string TableName { get; set; } = string.Empty;
    }
}
