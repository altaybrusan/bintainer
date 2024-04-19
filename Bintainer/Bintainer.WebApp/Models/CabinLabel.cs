using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class CabinLabel
{
    public int Id { get; set; }

    public string? Value { get; set; }

    public int CabinId { get; set; }

    public virtual Cabin IdNavigation { get; set; } = null!;
}
