using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class Component
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? CategoryId { get; set; }

    public int? Count { get; set; }

    public int InventoryId { get; set; }

    public virtual ICollection<ComponentAttribute> ComponentAttributes { get; set; } = new List<ComponentAttribute>();

    public virtual Inventory Inventory { get; set; } = null!;

    public virtual ICollection<Cabin> Cabins { get; set; } = new List<Cabin>();
}
