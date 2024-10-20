using System;
using System.Collections.Generic;

namespace Bintainer.Model;

public partial class PartPackage
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Url { get; set; }

    public string? FullFileName { get; set; }

    public string UserId { get; set; } = null!;

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    public virtual AspNetUser User { get; set; } = null!;
}
