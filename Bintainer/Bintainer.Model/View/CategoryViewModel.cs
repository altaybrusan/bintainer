using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.View
{
    public class CategoryViewModel
    {
        public string Title { get; set; } = string.Empty;
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public Guid? GuidId { get; set; }
        public string UserId { get; set; } = null!;
        public List<CategoryViewModel>? Children { get; set; } = new();
        public string? FlattenedHierarchy { get; set; } // New property to hold the flattened hierarchy
    }
}
