using System;
using System.Collections.Generic;

namespace Bintainer.WebApp;

public partial class PartAttribute
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Value { get; set; }

    public int TemplateId { get; set; }

    public virtual PartAttributeTemplate Template { get; set; } = null!;
}
