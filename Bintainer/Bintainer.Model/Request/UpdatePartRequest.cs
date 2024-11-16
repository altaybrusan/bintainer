using Bintainer.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.Request
{
    public class UpdatePartRequest
    {
        public Guid? GuidId { get; set; }
        public string? PartNumber { get; set; }
        public string? Description { get; set; }
        public string? Supplier { get; set; }
        public string? Package { get; set; } = string.Empty;
        public string? Group { get; set; }
        public string? OrderNumber { get; set; }
        public string? OrderQuantity { get; set; }
        public string? FootprintUrl { get; set; }
        public string? PartUrl { get; set; }
        public Guid? AttributeTemplateGuid { get; set; }
        public DateTime? OrderDate { get; set; }
        public List<PartAttributeViewModel>? Attributes { get; set; }
        public List<string?>? PathToCategory { get; set; }
    }
}
