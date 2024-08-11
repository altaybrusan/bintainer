using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartAttributeTemplate
{
    public int Id { get; set; }

    public string? TemplateName { get; set; }

    public string UserId { get; set; } = null!;

    public virtual ICollection<PartAttribute> PartAttributes { get; set; } = new List<PartAttribute>();

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<PartTemplate> PartTemplates { get; set; } = new List<PartTemplate>();
}
