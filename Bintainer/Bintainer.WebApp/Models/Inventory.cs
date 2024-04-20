using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public virtual ICollection<InventorySection> InventorySections { get; set; } = new List<InventorySection>();
}
