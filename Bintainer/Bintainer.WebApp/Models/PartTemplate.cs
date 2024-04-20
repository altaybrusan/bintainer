using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartTemplate
{
    public int Id { get; set; }

    public string? Supplier { get; set; }

    public string? PartNumber { get; set; }

    public string? ImageUri { get; set; }

    public string? DatasheetUri { get; set; }

    public virtual ICollection<PartAttribute> Attributes { get; set; } = new List<PartAttribute>();

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
