using System;
using System.Collections.Generic;

namespace Bintainer.Model;

public partial class Bin
{
    public int Id { get; set; }

    public int? SectionId { get; set; }

    public int CoordinateX { get; set; }

    public int CoordinateY { get; set; }

    public virtual ICollection<BinSubspace> BinSubspaces { get; set; } = new List<BinSubspace>();

    public virtual ICollection<PartBinAssociation> PartBinAssociations { get; set; } = new List<PartBinAssociation>();

    public virtual InventorySection? Section { get; set; }
}
