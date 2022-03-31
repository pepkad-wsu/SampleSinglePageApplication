using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels
{
    public partial class DataMigration
    {
        public int MigrationId { get; set; }
        public DateTime? MigrationDate { get; set; }
        public DateTime? MigrationApplied { get; set; }
        public string? Migration { get; set; }
    }
}
