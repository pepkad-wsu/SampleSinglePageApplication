using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels;

public partial class DepartmentGroup
{
    public Guid DepartmentGroupId { get; set; }

    public string? DepartmentGroupName { get; set; }

    public Guid? TenantId { get; set; }
}
