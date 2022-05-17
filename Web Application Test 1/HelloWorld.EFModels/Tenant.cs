using System;
using System.Collections.Generic;

namespace HelloWorld.EFModels
{
    public partial class Tenant
    {
        public Tenant()
        {
            Sources = new HashSet<Source>();
        }

        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string TenantCode { get; set; } = null!;
        public bool Enabled { get; set; }

        public virtual ICollection<Source> Sources { get; set; }
    }
}
