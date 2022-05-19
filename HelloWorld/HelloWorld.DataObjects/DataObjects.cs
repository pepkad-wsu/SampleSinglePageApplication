using System.ComponentModel;

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

        public void LogException(Exception ex, string SourceDescription = "")
        {
            var exception = ex;
            var count = 1;
            Messages.Add($"#EXCEPTION ({SourceDescription}) START");
            while(exception != null)
            {
                Messages.Add($"\t{count}: {exception.Message}");
                exception = exception.InnerException;
            }
            Messages.Add($"#EXCEPTION ({SourceDescription}) END");
        }
    }

    public class GetSourcesResult : ActionResponseObject
    {

        public List<Source> Sources { get; set; } = new List<Source>();
    }



    public class Tenant : ActionResponseObject
    {

        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string TenantCode { get; set; } = null!;
        public bool Enabled { get; set; }
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
        [Description("FirstType")]
        FirstType,
        [Description("SecondType")]
        SecondType,
        [Description("ThirdType")]
        ThirdType
    }

}