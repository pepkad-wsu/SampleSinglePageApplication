namespace SampleSinglePageApplication;

public partial class DataObjects
{
    public class Record : ActionResponseObject
    {
        public Guid RecordId { get; set; }
        public string Name { get; set; } = null!;
        public int? Number { get; set; }
        public bool? Boolean { get; set; }
        public string? Text { get; set; }
        public Guid? TenantId { get; set; }
        public Guid UserId { get; set; }
    }
}
