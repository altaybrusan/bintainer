using System;
using System.Collections.Generic;

namespace Bintainer.WebApp;

public partial class Bin
{
    public int Id { get; set; }

    public int? SectionId { get; set; }

    public int CoordinateX { get; set; }

    public int CoordinateY { get; set; }

    public bool? IsFilled { get; set; }

    public virtual ICollection<BinSubspace> BinSubspaces { get; set; } = new List<BinSubspace>();

    public virtual InventorySection? Section { get; set; }

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
