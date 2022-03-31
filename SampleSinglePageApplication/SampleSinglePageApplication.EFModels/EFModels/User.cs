using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels
{
    public partial class User
    {
        public User()
        {
            FileStorages = new HashSet<FileStorage>();
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
        public int? FailedLoginAttempts { get; set; }
        public DateTime? LastLockoutDate { get; set; }

        public virtual Department? Department { get; set; }
        public virtual ICollection<FileStorage> FileStorages { get; set; }
    }
}
