using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartNumber
{
    public int Id { get; set; }

    public string? Supplier { get; set; }

    public string? Number { get; set; }

    public int PartId { get; set; }

    public virtual Part Part { get; set; } = null!;
}
