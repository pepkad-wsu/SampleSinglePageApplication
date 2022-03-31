namespace SampleSinglePageApplication;

public static class GlobalSettings
{
    public static string DatabaseConnection { get; set; } = "";
    public static string MailPassword { get; set; } = "";
    public static int MailPort { get; set; }
    public static string MailServer { get; set; } = "";
    public static bool MailServerUseSSL { get; set; }
    public static string MailUsername { get; set; } = "";
    public static bool StartupRun { get; set; }
    public static string UserLookupClientUsername { get; set; } = "";
    public static string UserLookupClientPassword { get; set; } = "";
    public static string UserLookupClientEndpoint { get; set; } = "";

}