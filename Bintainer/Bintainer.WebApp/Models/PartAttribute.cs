using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartAttribute
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Value { get; set; }

    public virtual ICollection<PartTemplate> Templates { get; set; } = new List<PartTemplate>();
}
