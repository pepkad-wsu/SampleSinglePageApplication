using System;
using System.Collections.Generic;

namespace SampleSinglePageApplication.EFModels.EFModels
{
    public partial class FileStorage
    {
        public Guid FileId { get; set; }
        public Guid? ItemId { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public long? Bytes { get; set; }
        public byte[]? Value { get; set; }
        public DateTime? UploadDate { get; set; }
        public Guid? UserId { get; set; }
        public string? SourceFileId { get; set; }
        public Guid? TenantId { get; set; }

        public virtual User? User { get; set; }
    }
}
