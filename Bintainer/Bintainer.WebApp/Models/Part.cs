using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class Part
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public string? ImageUri { get; set; }

    public string? DatasheetUri { get; set; }

    public int? FootPrint { get; set; }

    public int? Package { get; set; }

    public virtual PartFootprint? FootPrintNavigation { get; set; }

    public virtual PartPackage? PackageNavigation { get; set; }

    public virtual ICollection<PartAttribute> PartAttributes { get; set; } = new List<PartAttribute>();

    public virtual ICollection<PartGroup> PartGroups { get; set; } = new List<PartGroup>();

    public virtual ICollection<PartLabel> PartLabels { get; set; } = new List<PartLabel>();

    public virtual ICollection<PartNumber> PartNumbers { get; set; } = new List<PartNumber>();

    public virtual ICollection<Bin> Bins { get; set; } = new List<Bin>();
}
