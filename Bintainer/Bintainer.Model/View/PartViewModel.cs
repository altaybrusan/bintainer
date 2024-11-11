using Bintainer.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.View
{
    public class PartViewModel
    {
        public int Id { get; set; }

        public string Number { get; set; } = null!;

        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        public int PackageId { get; set; }

        public string Supplier { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public CategoryViewModel? Category { get; set; }

        public PartPackageViewModel Package { get; set; } = null!;
        
        public ICollection<PartAttributeViewModel>? Attributes { get; set; }  
        
        public ICollection<PartGroupViewModel> Groups { get; set; }

    }
}
