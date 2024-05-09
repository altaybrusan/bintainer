using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartAttributeTemplate
{
    public int Id { get; set; }

    public string? TemplateName { get; set; }

    public int AttributeId { get; set; }

    public virtual PartAttribute Attribute { get; set; } = null!;

    public virtual ICollection<PartTemplate> Templates { get; set; } = new List<PartTemplate>();
}
