using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class ComponentAttribute
{
    public int Id { get; set; }

    public int ComponentId { get; set; }

    public string? Name { get; set; }

    public string? Value { get; set; }

    public virtual Component Component { get; set; } = null!;
}
