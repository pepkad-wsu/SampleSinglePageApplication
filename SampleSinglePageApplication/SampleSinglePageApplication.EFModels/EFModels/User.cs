using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels
{
    public partial class User
    {
        public User()
        {
            FileStorages = new HashSet<FileStorage>();
            Records = new HashSet<Record>();
        }

        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Username { get; set; } = null!;
        public string? EmployeeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public bool? Enabled { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool? Admin { get; set; }
        public string? Password { get; set; }
        public bool? PreventPasswordChange { get; set; }
        public int? FailedLoginAttempts { get; set; }
        public DateTime? LastLockoutDate { get; set; }
        public string? UDF01 { get; set; }
        public string? UDF02 { get; set; }
        public string? UDF03 { get; set; }
        public string? UDF04 { get; set; }
        public string? UDF05 { get; set; }
        public string? UDF06 { get; set; }
        public string? UDF07 { get; set; }
        public string? UDF08 { get; set; }
        public string? UDF09 { get; set; }
        public string? UDF10 { get; set; }

        public virtual Department? Department { get; set; }
        public virtual ICollection<FileStorage> FileStorages { get; set; }
        public virtual ICollection<Record> Records { get; set; }
    }
}
