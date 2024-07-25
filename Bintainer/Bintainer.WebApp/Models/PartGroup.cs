using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartGroup
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string UserId { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
