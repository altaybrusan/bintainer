using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class Inventory
{
    public int Id { get; set; }

    public string Admin { get; set; } = null!;

    public string? Name { get; set; }

    public string UserId { get; set; } = null!;

    public virtual ICollection<InventorySection> InventorySections { get; set; } = new List<InventorySection>();

    public virtual AspNetUser User { get; set; } = null!;
}
