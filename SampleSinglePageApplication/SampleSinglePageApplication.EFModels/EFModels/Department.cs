using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels
{
    public partial class Department
    {
        public Department()
        {
            Users = new HashSet<User>();
        }

        public Guid DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ActiveDirectoryNames { get; set; }
        public bool? Enabled { get; set; }
        public Guid? DepartmentGroupId { get; set; }
        public Guid? TenantId { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
