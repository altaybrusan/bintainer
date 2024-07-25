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

    public string UserId { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<PartAttributeTemplate> AttributeTemplates { get; set; } = new List<PartAttributeTemplate>();

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
