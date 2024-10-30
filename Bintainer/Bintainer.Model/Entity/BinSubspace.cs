using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class BinSubspace
{
    public int Id { get; set; }

    public int? BinId { get; set; }

    public int? SubspaceIndex { get; set; }

    public string? Label { get; set; }

    public virtual Bin? Bin { get; set; }

    public virtual ICollection<PartBinAssociation> PartBinAssociations { get; set; } = new List<PartBinAssociation>();
}
