using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public string Admin { get; set; } = null!;

    public string? Name { get; set; }

    public virtual ICollection<InventorySection> InventorySections { get; set; } = new List<InventorySection>();
}
