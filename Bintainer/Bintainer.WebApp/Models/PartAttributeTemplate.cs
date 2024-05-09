using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartAttributeTemplate
{
    public int Id { get; set; }

    public string? TemplateName { get; set; }

    public virtual ICollection<PartAttribute> PartAttributes { get; set; } = new List<PartAttribute>();

    public virtual ICollection<PartTemplate> Templates { get; set; } = new List<PartTemplate>();
}
