using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bintainer.Model.Entity;

public partial class InventorySection
{
    public int Id { get; set; }

    public string? SectionName { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public int InventoryId { get; set; }

    public int? SubspaceCount { get; set; }
    [JsonIgnore]
    public virtual ICollection<Bin> Bins { get; set; } = new List<Bin>();
    [JsonIgnore]
    public virtual Inventory? Inventory { get; set; } = null!;
}
