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
        public List<CategoryViewModel> Children { get; set; } = new();
    }
}
