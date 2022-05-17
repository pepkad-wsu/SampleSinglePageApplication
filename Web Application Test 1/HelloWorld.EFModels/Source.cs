using System;
using System.Collections.Generic;

namespace HelloWorld.EFModels
{
    public partial class Source
    {
        public Guid SourceId { get; set; }
        public string? SourceName { get; set; }
        public string? SourceType { get; set; }
        public string? SourceCategory { get; set; }
        public string? SourceTemplate { get; set; }
        public Guid? TenantId { get; set; }

        public virtual Tenant? Tenant { get; set; }
    }
}
