using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels;

public partial class UDFLabel
{
    public Guid Id { get; set; }

    public string? Module { get; set; }

    public string? UDF { get; set; }

    public string? Label { get; set; }

    public bool? ShowColumn { get; set; }

    public bool? ShowInFilter { get; set; }

    public bool? IncludeInSearch { get; set; }

    public Guid? TenantId { get; set; }
}
