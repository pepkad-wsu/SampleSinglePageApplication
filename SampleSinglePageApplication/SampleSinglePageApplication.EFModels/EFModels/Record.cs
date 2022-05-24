using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels
{
    public partial class Record
    {
        public Guid RecordId { get; set; }
        public string Name { get; set; } = null!;
        public int? Number { get; set; }
        public bool? Boolean { get; set; }
        public string? Text { get; set; }
        public Guid? TenantId { get; set; }
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
