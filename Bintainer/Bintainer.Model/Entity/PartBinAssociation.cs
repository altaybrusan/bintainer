using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class PartBinAssociation
{
    public int PartId { get; set; }

    public int BinId { get; set; }

    public int SubspaceId { get; set; }

    public int Quantity { get; set; }

    public virtual Bin Bin { get; set; } = null!;

    public virtual Part Part { get; set; } = null!;

    public virtual BinSubspace Subspace { get; set; } = null!;
}
