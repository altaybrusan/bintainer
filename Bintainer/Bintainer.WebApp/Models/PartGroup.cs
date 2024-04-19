using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartGroup
{
    public int Id { get; set; }

    public int PartId { get; set; }

    public string? Name { get; set; }

    public virtual Part Part { get; set; } = null!;
}
