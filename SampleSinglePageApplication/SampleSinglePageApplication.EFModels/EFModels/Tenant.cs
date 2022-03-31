using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels
{
    public partial class Tenant
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string TenantCode { get; set; } = null!;
        public bool Enabled { get; set; }
    }
}
