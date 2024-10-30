using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class PartLabel
{
    public int Id { get; set; }

    public int PartId { get; set; }

    public string? Value { get; set; }

    public virtual Part Part { get; set; } = null!;
}
