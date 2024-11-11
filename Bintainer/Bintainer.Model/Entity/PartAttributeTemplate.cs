using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class PartAttributeTemplate
{
    public int Id { get; set; }

    public string TemplateName { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public Guid? GuidId { get; set; }

    public virtual ICollection<PartAttributeDefinition> PartAttributeDefinitions { get; set; } = new List<PartAttributeDefinition>();

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    public virtual AspNetUser User { get; set; } = null!;
}
