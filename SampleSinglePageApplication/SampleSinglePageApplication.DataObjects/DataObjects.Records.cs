namespace SampleSinglePageApplication;

public partial class DataObjects
{
    public class Record : ActionResponseObject
    {
        public Guid RecordId { get; set; }
        public string RedcordName { get; set; } = null!;
        public int? RecordNumber { get; set; }
        public bool? RecordBoolean { get; set; }
        public string? RecordText { get; set; }
        public Guid? TenantId { get; set; }
        public Guid UserId { get; set; }

        public string Username { get; set; } = null!;
    }
}
