using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class PartAttributeDefinition
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Value { get; set; }

    public int TemplateId { get; set; }

    public Guid? GuidId { get; set; }

    public virtual PartAttributeTemplate Template { get; set; } = null!;
}
