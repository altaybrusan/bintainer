using System;
using System.Collections.Generic;

namespace Bintainer.Model.Entity;

public partial class PartTemplateAssignment
{
    public int PartId { get; set; }

    public int PartTemplateId { get; set; }

    public virtual PartTemplate PartTemplate { get; set; } = null!;
}
