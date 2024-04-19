using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class Cabin
{
    public int Id { get; set; }

    public int? SectionId { get; set; }

    public int CoordinateX { get; set; }

    public int CoordinateY { get; set; }

    public int? Capacity { get; set; }

    public virtual CabinLabel? CabinLabel { get; set; }

    public virtual InventorySection? Section { get; set; }

    public virtual ICollection<Component> Components { get; set; } = new List<Component>();
}
