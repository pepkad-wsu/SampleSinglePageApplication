namespace SampleSinglePageApplication;

public partial class DataObjects
{
    public class Source : ActionResponseObject
    {
        public Guid SourceId { get; set; }
        public string Name { get; set; } = null!;
        public int? Number { get; set; }
        public bool? Boolean { get; set; }
        public string? Type { get; set; }
        public DateTime? Date { get; set; }
        public Guid? TenantId { get; set; }
        public Guid UserId { get; set; }
    }
}
