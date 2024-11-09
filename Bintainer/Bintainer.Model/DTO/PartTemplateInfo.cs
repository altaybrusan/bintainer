using Bintainer.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.DTO
{
    public class PartTemplateInfo
    {
        public string? TemplateName { get; set; } = null!;
        public Guid? GuidId { get; set; }
        public IEnumerable<PartAttributeInfo>? PartAttributes { get; set; }
    }
}
