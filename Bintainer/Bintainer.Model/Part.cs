using System;
using System.Collections.Generic;

namespace Bintainer.Model;

public partial class Part
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public int PackageId { get; set; }

    public string Supplier { get; set; } = null!;

    public string? ImageUri { get; set; }

    public string? DatasheetUri { get; set; }

    public string? SupplierUri { get; set; }

    public string UserId { get; set; } = null!;

    public int? OrderId { get; set; }

    public virtual PartCategory? Category { get; set; }

    public virtual ICollection<OrderPartAssociation> OrderPartAssociations { get; set; } = new List<OrderPartAssociation>();

    public virtual PartPackage Package { get; set; } = null!;

    public virtual ICollection<PartBinAssociation> PartBinAssociations { get; set; } = new List<PartBinAssociation>();

    public virtual ICollection<PartLabel> PartLabels { get; set; } = new List<PartLabel>();

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<PartAttributeTemplate> AttributeTemplates { get; set; } = new List<PartAttributeTemplate>();

    public virtual ICollection<PartGroup> Groups { get; set; } = new List<PartGroup>();
}
