using Bintainer.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.View
{
    public class PartBinViewModel
    {
        public string Number { get; set; } = null!;
        public string? SectionName { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public List<int?>? SubspaceIndices { get; set; }
    }
}
