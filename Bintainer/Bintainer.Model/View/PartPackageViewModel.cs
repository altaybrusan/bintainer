using Bintainer.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.View
{
    public class PartPackageViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? FullFileName { get; set; }

        public string UserId { get; set; } = null!;

    }
}
