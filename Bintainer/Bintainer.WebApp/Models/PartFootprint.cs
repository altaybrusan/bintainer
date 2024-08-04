using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartFootprint
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Url { get; set; }

    public string? FullFileName { get; set; }

    public string UserId { get; set; } = null!;

    public virtual Part? Part { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
