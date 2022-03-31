namespace SampleSinglePageApplication;
public class DataObjects
{
    public enum SignalRUpdateType
    {
        This = 0,
        That = 1,
        Setting = 2,
        Unknown = 3,
        Files = 4
    }

    public enum SettingType
    {
        Boolean = 0,
        DateTime = 1,
        EncryptedText = 2,
        Guid = 3,
        NumberDecimal = 4,
        NumberDouble = 5,
        NumberInt = 6,
        Object = 7,
        Text = 8
    }

    public enum UserLookupType
    {
        Email,
        EmployeeId,
        Guid,
        Username
    }

    public class ActionResponseObject
    {
        public BooleanResponse ActionResponse { get; set; } = new BooleanResponse();
    }

    public class ActiveDirectorySearchResults
    {
        public Guid TenantId { get; set; }
        public Guid? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
    }

    public class ActiveDirectoryUserInfo
    {
        public Guid TenantId { get; set; }
        public Guid? UserId { get; set; }
        public string? Department { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? EmployeeId { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
    }

    public class AjaxLookup : ActionResponseObject
    {
        public Guid TenantId { get; set; }
        public string? Search { get; set; }
        public List<string>? Parameters { get; set; }
        public List<AjaxResults>? Results { get; set; }
    }

    public class AjaxResults
    {
        public string? label { get; set; }
        public string? value { get; set; }
        public string? email { get; set; }
        public string? username { get; set; }
        public string? extra1 { get; set; }
        public string? extra2 { get; set; }
        public string? extra3 { get; set; }
    }

    public class Authenticate
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class BooleanResponse
    {
        public List<string> Messages { get; set; } = new List<string>();
        public bool Result { get; set; }
    }

    public class ConnectionStringConfig : ActionResponseObject
    {
        public string Server { get; set; } = null!;
        public string Database { get; set; } = null!;
        public string UserId { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
    }

    public class DataMigration
    {
        public List<string>? Migration { get; set; } = new List<string>();
        public DateTime MigrationDate { get; set; }
        public int MigrationId { get; set; }
    }

    public class Department : ActionResponseObject
    {
        public Guid DepartmentId { get; set; }
        public Guid TenantId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ActiveDirectoryNames { get; set; }
        public bool Enabled { get; set; }
        public Guid? DepartmentGroupId { get; set; }
    }

    public class DepartmentGroup : ActionResponseObject
    {
        public Guid DepartmentGroupId { get; set; }
        public Guid TenantId { get; set; }
        public string? DepartmentGroupName { get; set; }
    }

    public class EmailMessage : ActionResponseObject
    {
        public string? From { get; set; }
        public string? FromDisplayName { get; set; }
        public List<string>? To { get; set; }
        public List<string>? Cc { get; set; }
        public List<string>? Bcc { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<DataObjects.FileStorage> Files { get; set; }

        public EmailMessage()
        {
            this.To = new List<string>();
            this.Cc = new List<string>();
            this.Bcc = new List<string>();
            this.Files = new List<FileStorage>();
        }
    }

    public class FileStorage : ActionResponseObject
    {
        public Guid FileId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? ItemId { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public string? SourceFileId { get; set; }
        public long? Bytes { get; set; }
        public Byte[]? Value { get; set; }
        public DateTime UploadDate { get; set; }
        public Guid? UserId { get; set; }
    }

    public class Filter : ActionResponseObject
    {
        public Guid TenantId { get; set; }
        public double ExecutionTime { get; set; }
        public bool Loading { get; set; }
        public bool ShowFilters { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? Keyword { get; set; }
        public string? Sort { get; set; }
        public string? SortOrder { get; set; }
        public int RecordsPerPage { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public int Page { get; set; }
        public byte[]? Export { get; set; }
        public List<Guid>? Tenants { get; set; }
        public List<FilterColumn>? Columns { get; set; }
        public object[]? Records { get; set; }
    }

    public class FilterColumn
    {
        public string? Align { get; set; }
        public string? Label { get; set; }
        public string? TipText { get; set; }
        public string? DataElementName { get; set; }
        public string? DataType { get; set; }
        public bool Sortable { get; set; }
    }

    public class FilterUsers : Filter
    {
        public List<Guid>? FilterDepartments { get; set; }
        public string? Enabled { get; set; }
        public string? Admin { get; set; }
    }

    public class ListItem : ActionResponseObject
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public int SortOrder { get; set; }
        public bool Enabled { get; set; }
    }

    public class MailServerConfig
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class OptionPair
    {
        public string? Id { get; set; }
        public string? Value { get; set; }
    }

    public class Setting : ActionResponseObject
    {
        public int SettingId { get; set; }
        public string SettingName { get; set; } = null!;
        public string? SettingType { get; set; }
        public string? SettingNotes { get; set; }
        public string? SettingText { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? UserId { get; set; }
    }

    public class SignalRUpdate
    {
        public Guid? TenantId { get; set; }
        public Guid? RequestId { get; set; }
        public string? ItemId { get; set; }
        public Guid? UserId { get; set; }
        public SignalRUpdateType UpdateType { get; set; }
        public string? UpdateTypeString { get; set; }
        public string? Message { get; set; }
        public object? Object { get; set; }
    }

    public class SimplePost
    {
        public string? SingleItem { get; set; }
        public List<string> Items { get; set; }

        public SimplePost()
        {
            this.Items = new List<string>();
        }
    }

    public class Tenant : ActionResponseObject
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string TenantCode { get; set; } = null!;
        public bool Enabled { get; set; }
        public List<Department>? Departments { get; set; } = null!;
        public List<DepartmentGroup>? DepartmentGroups { get; set; } = null!;
        public TenantSettings TenantSettings { get; set; } = new TenantSettings();
    }

    public class TenantSettings
    {
        public bool AllowUsersToManageAvatars { get; set; }
        public bool AllowUsersToManageBasicProfileInfo { get; set; }
        public List<string>? AllowUsersToManageBasicProfileInfoElements { get; set; }
        public string? JasonWebTokenKey { get; set; }
        public List<string> LoginOptions { get; set; } = new List<string>();
        public WorkSchedule WorkSchedule { get; set; } = new WorkSchedule();
        public bool RequirePreExistingAccountToLogIn { get; set; }
        public List<ListItem>? ListItems { get; set; } = null!;
    }

    public class User : ActionResponseObject
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Username { get; set; } = null!;
        public string? EmployeeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool Admin { get; set; }
        public bool AppAdmin { get; set; }
        public Guid? Photo { get; set; }
        public string? Password { get; set; }
        public bool HasLocalPassword { get; set; }
        public string? AuthToken { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LastLockoutDate { get; set; }
        public List<Tenant>? Tenants { get; set; }
        public List<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    }

    public class UserPasswordReset
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    public class UserTenant
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string TenantCode { get; set; } = null!;
        public string TenantName { get; set; } = null!;
    }

    public class WorkSchedule
    {
        public bool Sunday { get; set; }
        public bool SundayAllDay { get; set; }
        public string? SundayStart { get; set; } = null!;
        public string? SundayEnd { get; set; } = null!;

        public bool Monday { get; set; }
        public bool MondayAllDay { get; set; }
        public string? MondayStart { get; set; } = null!;
        public string? MondayEnd { get; set; } = null!;

        public bool Tuesday { get; set; }
        public bool TuesdayAllDay { get; set; }
        public string? TuesdayStart { get; set; } = null!;
        public string? TuesdayEnd { get; set; } = null!;

        public bool Wednesday { get; set; }
        public bool WednesdayAllDay { get; set; }
        public string? WednesdayStart { get; set; } = null!;
        public string? WednesdayEnd { get; set; } = null!;

        public bool Thursday { get; set; }
        public bool ThursdayAllDay { get; set; }
        public string? ThursdayStart { get; set; } = null!;
        public string? ThursdayEnd { get; set; } = null!;

        public bool Friday { get; set; }
        public bool FridayAllDay { get; set; }
        public string? FridayStart { get; set; } = null!;
        public string? FridayEnd { get; set; } = null!;

        public bool Saturday { get; set; }
        public bool SaturdayAllDay { get; set; }
        public string? SaturdayStart { get; set; } = null!;
        public string? SaturdayEnd { get; set; } = null!;
    }
}