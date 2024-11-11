using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.View
{
    public class PartGroupViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string UserId { get; set; } = null!;

        public Guid? GuidId { get; set; }
    }
}
