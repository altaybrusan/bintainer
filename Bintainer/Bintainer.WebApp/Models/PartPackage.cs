using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartPackage
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
