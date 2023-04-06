using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels;

public partial class UserGroup
{
    public Guid GroupId { get; set; }

    public Guid TenantId { get; set; }

    public string? Name { get; set; }

    public bool Enabled { get; set; }

    public string? Settings { get; set; }

    public virtual ICollection<UserInGroup> UserInGroups { get; } = new List<UserInGroup>();
}
