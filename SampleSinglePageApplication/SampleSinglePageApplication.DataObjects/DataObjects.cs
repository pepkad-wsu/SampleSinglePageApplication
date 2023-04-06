using System.Text.Json.Serialization;

namespace SampleSinglePageApplication;
public class DataObjects
{
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

    public enum SignalRUpdateType
    {
        Setting,
        Unknown,
        Files
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

    public class AddModule
    {
        public string? Module { get; set; }
        public string? Name { get; set; }
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

    public class ApplicationSettings : ActionResponseObject
    {
        public string? ApplicationURL { get; set; }
        public string? DefaultTenantCode { get; set; }
        public string? EncryptionKey { get; set; }
        public string? MailServer { get; set; }
        public string? MailServerPassword { get; set; }
        public int? MailServerPort { get; set; }
        public string? MailServerUsername { get; set; }
        public bool MailServerUseSSL { get; set; }
        public string? DefaultReplyToAddress { get; set; }
        public bool UseTenantCodeInUrl { get; set; }
        public bool ShowTenantCodeFieldOnLoginForm { get; set; }
        public bool ShowTenantListingWhenMissingTenantCode { get; set; }
    }

    public class Authenticate
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? TenantCode { get; set; }
    }

    public class BooleanResponse
    {
        public List<string> Messages { get; set; } = new List<string>();
        public bool Result { get; set; }
    }

    public class ConnectionStringConfig : ActionResponseObject
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseType { get; set; } = null!;

        public string MySQL_Server { get; set; } = null!;
        public string MySQL_Database { get; set; } = null!;
        public string MySQL_User { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string MySQL_Password { get; set; } = null!;


        public string PostgreSql_Host { get; set; } = null!;
        public string PostgreSql_Database { get; set; } = null!;
        public string PostgreSql_Username { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string PostgreSql_Password { get; set; } = null!;


        public string SQLiteDatabase { get; set; } = null!;

        public string SqlServer_Server { get; set; } = null!;
        public string SqlServer_Database { get; set; } = null!;
        public string SqlServer_UserId { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string SqlServer_Password { get; set; } = null!;
        public bool SqlServer_IntegratedSecurity { get; set; }
        public bool SqlServer_PersistSecurityInfo { get; set; }
        public bool SqlServer_TrustServerCertificate { get; set; }
    }

    public class DataMigration
    {
        public List<string> Migration { get; set; } = new List<string>();
        public string MigrationId { get; set; } = String.Empty;
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

    public class Dictionary
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
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

    public class ExternalDataSource
    {
        public string Name { get; set; } = String.Empty;
        public string Type { get; set; } = String.Empty;
        public string? ConnectionString { get; set; } = String.Empty;
        public string Source { get; set; } = String.Empty;
        public int SortOrder { get; set; }
        public bool Active { get; set; }
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
        [JsonConverter(typeof(UTCDateTimeConverter))]
        public DateTime UploadDate { get; set; }
        public Guid? UserId { get; set; }
    }

    public class Filter : ActionResponseObject
    {
        public Guid TenantId { get; set; }
        public double ExecutionTime { get; set; }
        public bool Loading { get; set; }
        public bool ShowFilters { get; set; }
        [JsonConverter(typeof(UTCDateTimeConverter))]
        public DateTime? Start { get; set; }
        [JsonConverter(typeof(UTCDateTimeConverter))]
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
        public string? CultureCode { get; set; }
    }

    public class FilterColumn
    {
        public string? Align { get; set; }
        public string? Label { get; set; }
        public string? TipText { get; set; }
        public string? DataElementName { get; set; }
        public string? DataType { get; set; }
        public bool Sortable { get; set; }
        public string? Class { get; set; }
        public string? BooleanIcon { get; set; }
    }

    public class FilterUsers : Filter
    {
        public List<Guid>? FilterDepartments { get; set; }
        public string? Enabled { get; set; }
        public string? Admin { get; set; }
        public string? udf01 { get; set; }
        public string? udf02 { get; set; }
        public string? udf03 { get; set; }
        public string? udf04 { get; set; }
        public string? udf05 { get; set; }
        public string? udf06 { get; set; }
        public string? udf07 { get; set; }
        public string? udf08 { get; set; }
        public string? udf09 { get; set; }
        public string? udf10 { get; set; }
    }

    public class Language
    {
        public string Culture { get; set; } = String.Empty;
        public List<DataObjects.OptionPair> Phrases { get; set; } = new List<OptionPair>();
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
        public List<string> Items { get; set; } = new List<string>();
    }

    public class SimpleResponse
    {
        public bool Result { get; set; }
        public string? Message { get; set; }
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
        public List<udfLabel>? udfLabels { get; set; } = null!;
    }

    public class TenantList
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = "";
        public string TenantCode { get; set; } = "";
    }

    public class TenantSettings
    {
        public bool AllowUsersToManageAvatars { get; set; }
        public bool AllowUsersToManageBasicProfileInfo { get; set; }
        public List<string>? AllowUsersToManageBasicProfileInfoElements { get; set; }
        public bool AllowUsersToResetPasswordsForLocalLogin { get; set; }
        public bool AllowUsersToSignUpForLocalLogin { get; set; }
        public string? CookieDomain { get; set; }
        public string? CustomAuthenticationCode { get; set; }
        public string? CustomAuthenticationName { get; set; }
        public string? DefaultCultureCode { get; set; }
        public string? DefaultReplyToAddress { get; set; }
        public string? EitSsoUrl { get; set; }
        public string? JasonWebTokenKey { get; set; }
        public string? LdapLookupRoot { get; set; }
        public string? LdapLookupUsername { get; set; }
        public string? LdapLookupPassword { get; set; }
        public string? LdapLookupSearchBase { get; set; }
        public string? LdapLookupLocationAttribute { get; set; }
        public int LdapLookupPort { get; set; }
        public List<string> LoginOptions { get; set; } = new List<string>();
        public List<string> ModuleHideElements { get; set; } = new List<string>();
        public WorkSchedule WorkSchedule { get; set; } = new WorkSchedule();
        public bool RequirePreExistingAccountToLogIn { get; set; }
        public List<ListItem>? ListItems { get; set; } = null!;
        public List<ExternalDataSource>? ExternalUserDataSources { get; set; }
    }

    public class udfLabel
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string? Module { get; set; }
        public string? udf { get; set; }
        public string? Label { get; set; }
        public bool ShowColumn { get; set; }
        public bool ShowInFilter { get; set; }
        public bool IncludeInSearch { get; set; }
        public List<string> FilterOptions { get; set; }

        public udfLabel()
        {
            this.FilterOptions = new List<string>();
        }
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
        public string? Username { get; set; }
        public string? EmployeeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public bool Enabled { get; set; }
        [JsonConverter(typeof(UTCDateTimeConverter))]
        public DateTime? LastLogin { get; set; }
        public bool Admin { get; set; }
        public bool AppAdmin { get; set; }
        public Guid? Photo { get; set; }
        public string? Password { get; set; }
        public bool PreventPasswordChange { get; set; }
        public bool HasLocalPassword { get; set; }
        public string? AuthToken { get; set; }
        public int FailedLoginAttempts { get; set; }
        [JsonConverter(typeof(UTCDateTimeConverter))]
        public DateTime? LastLockoutDate { get; set; }
        public List<Tenant>? Tenants { get; set; }
        public List<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
        public string? Source { get; set; }
        public string? udf01 { get; set; }
        public string? udf02 { get; set; }
        public string? udf03 { get; set; }
        public string? udf04 { get; set; }
        public string? udf05 { get; set; }
        public string? udf06 { get; set; }
        public string? udf07 { get; set; }
        public string? udf08 { get; set; }
        public string? udf09 { get; set; }
        public string? udf10 { get; set; }
    }

    public class UserGroup : ActionResponseObject
    {
        public Guid GroupId { get; set; }
        public Guid TenantId { get; set; }
        public string? Name { get; set; }
        public bool Enabled { get; set; }
        public List<Guid>? Users { get; set; }
        public UserGroupSettings Settings { get; set; } = new UserGroupSettings();
    }

    public class UserGroupSettings
    {
        public string? SomeSetting { get; set; }
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

    public class VersionInfo
    {
        [JsonConverter(typeof(UTCDateTimeConverter))]
        public DateTime Released { get; set; }
        public double RunningSince { get; set; }
        public string? Version { get; set; }
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