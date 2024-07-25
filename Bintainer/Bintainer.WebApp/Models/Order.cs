using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class Order
{
    public int Id { get; set; }

    public int PartId { get; set; }

    public int? Qunatity { get; set; }

    public DateTime? DateTime { get; set; }

    public string? Number { get; set; }

    public string UserId { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
