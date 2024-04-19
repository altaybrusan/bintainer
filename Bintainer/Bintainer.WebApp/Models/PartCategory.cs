using System;
using System.Collections.Generic;

namespace Bintainer.WebApp.Models;

public partial class PartCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? ParentCategoryId { get; set; }
}
