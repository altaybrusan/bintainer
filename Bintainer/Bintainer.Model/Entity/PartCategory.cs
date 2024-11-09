using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class PartCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? ParentCategoryId { get; set; }

    public string UserId { get; set; } = null!;

    public Guid? GuidId { get; set; }

    public virtual ICollection<PartCategory> InverseParentCategory { get; set; } = new List<PartCategory>();

    public virtual PartCategory? ParentCategory { get; set; }

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    public virtual AspNetUser User { get; set; } = null!;
}
