using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels
{
    public partial class Record
    {
        public Guid RecordId { get; set; }
        public string RedcordName { get; set; } = null!;
        public int? RecordNumber { get; set; }
        public bool? RecordBoolean { get; set; }
        public string? RecordText { get; set; }
        public Guid? TenantId { get; set; }
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
