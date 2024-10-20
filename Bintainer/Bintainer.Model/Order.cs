using System;
using System.Collections.Generic;

namespace Bintainer.Model;

public partial class Order
{
    public int Id { get; set; }

    public string? OrderNumber { get; set; }

    public string Supplier { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public DateTime? HandOverDate { get; set; }

    public string UserId { get; set; } = null!;

    public virtual ICollection<OrderPartAssociation> OrderPartAssociations { get; set; } = new List<OrderPartAssociation>();

    public virtual AspNetUser User { get; set; } = null!;
}
