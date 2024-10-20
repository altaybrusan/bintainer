using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class OrderPartAssociation
{
    public int OrderId { get; set; }

    public int PartId { get; set; }

    public int? Qunatity { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Part Part { get; set; } = null!;
}
