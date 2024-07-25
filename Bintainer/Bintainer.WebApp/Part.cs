using System;
using System.Collections.Generic;

namespace Bintainer.WebApp;

public partial class Part
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public int FootPrint { get; set; }

    public int? Package { get; set; }

    public string UserId { get; set; } = null!;

    public virtual PartCategory? Category { get; set; }

    public virtual PartFootprint FootPrintNavigation { get; set; } = null!;

    public virtual ICollection<PartLabel> PartLabels { get; set; } = new List<PartLabel>();

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<Bin> Bins { get; set; } = new List<Bin>();

    public virtual ICollection<PartGroup> Groups { get; set; } = new List<PartGroup>();

    public virtual ICollection<PartTemplate> Templates { get; set; } = new List<PartTemplate>();
}
