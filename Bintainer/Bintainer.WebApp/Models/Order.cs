using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class Order
{
    public int Id { get; set; }

    public string PartId { get; set; } = null!;

    public string OrderNumber { get; set; } = null!;

    public int? Qunatity { get; set; }

    public string Supplier { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public DateTime? HandOverDate { get; set; }

    public string UserId { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
