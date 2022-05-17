namespace HelloWorld;

public class DataObjects
{
    public class ActionResponseObject
    {
        public BooleanResponse ActionResponse { get; set; } = new BooleanResponse();
    }

    public class BooleanResponse
    {
        public List<string> Messages { get; set; } = new List<string>();
        public bool Result { get; set; }
    }

    public class Tenant : ActionResponseObject
    {

        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string TenantCode { get; set; } = null!;
        public bool Enabled { get; set; }


        public bool HasFinishedLoading { get; set; }
    }

    public class Source : ActionResponseObject
    {
        public Guid SourceId { get; set; }
        public string? SourceName { get; set; }
        public SourceType SourceType { get; set; }
        public string? SourceCategory { get; set; }
        public string? SourceTemplate { get; set; }
        public Guid? TenantId { get; set; }

        public Tenant? Tenant { get; set; }
    }

    public enum SourceType
    {
        FirstType,
        SecondType,
        ThirdType
    }

}