namespace SampleSinglePageApplication.Transcriber // Note: actual namespace depends on the project name.
{
    public class TranscribedClass
    {
        public TranscribedClass(SampleSinglePageApplication.EFModels.EFModels.EFDataModel _data, Type type)
        {
            Info = TranscriberUtilities.CreateReflectedClassInfo(_data, type, type);
            Type = type;
        }
        public List<string> CSharpDataAccessLanguageDefaults { get; set; } = new List<string>();
        public List<string> CSharpDataAccess { get; set; } = new List<string>();
        public List<string> CSharpDataAccessEnums { get; set; } = new List<string>();
        public List<string> CSharpDataControllers { get; set; } = new List<string>();
        public List<string> CSharpDataObjects { get; set; } = new List<string>();
        public ReflectedClassInfo Info { get; set; }
        public List<string> PartialViews { get; set; } = new List<string>();
        public Type Type { get; set; }
        public List<string> DtoTypeScriptResults { get; set; } = new List<string>();
        public string? WindowInterfaceTypeScriptResults { get; set; }
        public List<string> KnockoutTypeScriptResults { get; set; } = new List<string>();
        public List<string> ViewModels { get; set; } = new List<string>();
    }
}
