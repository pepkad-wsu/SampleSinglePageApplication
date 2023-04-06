namespace SampleSinglePageApplication;

public interface IDataAccess
{
    DataObjects.BooleanResponse AddModule(DataObjects.AddModule module);
    Task<DataObjects.BooleanResponse> AddUserToGroup(Guid UserId, Guid GroupId);
    string AppendWithComma(string Original, string New);
    string ApplicationURL { get; }
    string AppName { get; }
    Task<T?> AppSetting<T>(string SettingName);
    DataObjects.AjaxLookup AjaxUserSearch(DataObjects.AjaxLookup Lookup, bool LocalOnly = false);
    Task<DataObjects.User> Authenticate(string Username, string Password, Guid? TenantId);
    bool BooleanValue(bool? value);
    string CleanHtml(string html);
    string ConnectionString(bool full = false);
    string ConnectionStringReport(string input);
    string CookieRead(string cookieName);
    void CookieWrite(string cookieName, string value, string cookieDomain = "");
    string Copyright { get; }
    Task<DataObjects.User> CreateNewUserFromEmailAddress(Guid TenantId, string EmailAddress);
    bool DatabaseOpen { get; }
    string DatabaseType { get; }
    string Decrypt(string? input);
    string DefaultTenantCode { get; }
    Task<DataObjects.BooleanResponse> DeleteDepartment(Guid DepartmentId);
    Task<DataObjects.BooleanResponse> DeleteDepartmentGroup(Guid DepartmentGroupId);
    Task<DataObjects.BooleanResponse> DeleteFileStorage(Guid FileId);
    Task<DataObjects.BooleanResponse> DeleteTenant(Guid TenantId);
    Task<DataObjects.BooleanResponse> DeleteUser(Guid UserId);
    Task<DataObjects.BooleanResponse> DeleteUserGroup(Guid GroupId);
    Task<DataObjects.BooleanResponse> DeleteUserPhoto(Guid UserId);
    Task<Guid> DepartmentIdFromNameAndLocation(Guid TenantId, string? Department, string? Location = "");
    T? DeserializeObject<T>(string? SerializedObject);
    T? DeserializeObjectFromXmlOrJson<T>(string? SerializedObject);
    string DisplayNameFromLastAndFirst(string? LastName, string? FirstName, string? Email, string? DepartmentName, string? Location);
    string Encrypt(string? input);
    T? ExecuteDynamicCSharpCode<T>(string code, object[] objects, List<string>? additionalAssemblies, string Namespace, string Classname, string invokerFunction);
    Task<DataObjects.User> ForgotPassword(DataObjects.User user);
    Task<DataObjects.User> ForgotPasswordConfirm(DataObjects.User user);
    string GenerateRandomCode(int Length);
    DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfo(Guid TenantId, string Lookup, DataObjects.UserLookupType Type);
    DataObjects.ApplicationSettings GetApplicationSettings();
    DataObjects.ConnectionStringConfig GetConnectionStringConfig();
    DataObjects.Language GetDefaultLanguage();
    Task<DataObjects.Department> GetDepartment(Guid DepartmentId);
    Task<DataObjects.DepartmentGroup> GetDepartmentGroup(Guid DepartmentGroupId);
    Task<List<DataObjects.DepartmentGroup>> GetDepartmentGroups(Guid TenantId);
    string GetDepartmentName(Guid TenantId, Guid DepartmentId);
    Task<List<DataObjects.Department>> GetDepartments(Guid TenantId);
    Task<string> GetDisplayNameFromUserId(Guid? UserId, bool LastNameFirst = false);
    Task<string> GetEmailFromUserId(Guid? UserId);
    Task<DataObjects.FileStorage> GetFileStorage(Guid FileId);
    Task<List<DataObjects.FileStorage>> GetFileStorageItems(Guid ItemId, bool ImagesOnly, bool ResizeAsThumbnail);
    Task<string> GetFirstNameFromUserId(Guid? UserId);
    string GetFullUrl();
    string GetFullUrlWithoutQuerystring();
    List<DataObjects.OptionPair> GetLanguageCultureCodes();
    Task<List<string>> GetLanguageCultures(Guid TenantId);
    string GetLanguageItem(string? item, DataObjects.Language? language = null);
    DataObjects.MailServerConfig GetMailServerConfig();
    DataObjects.BooleanResponse GetNewActionResponse(bool result = false, string? message = null);
    string GetNewEncryptionKey();
    Task<DataObjects.Setting> GetSetting(string SettingName);
    T? GetSetting<T>(string SettingName, DataObjects.SettingType SettingType, Guid? TenantId = null, Guid? UserId = null);
    DataObjects.TenantSettings GetSettings(Guid TenantId, bool FullSettings = false);
    string GetSourceJWT(Guid TenantId, string Source);
    DataObjects.Tenant? GetTenant(Guid TenantId);
    Task<DataObjects.Tenant> GetTenantFull(Guid TenantId);
    Task<DataObjects.Tenant> GetTenantFromCode(string tenantCode);
    Guid GetTenantIdFromCode(string tenantCode);
    DataObjects.Language GetTenantLanguage(Guid TenantId, string Culture = "en-US");
    Task<List<DataObjects.Tenant>> GetTenants();
    DataObjects.TenantSettings GetTenantSettings(Guid TenantId);
    Task<List<DataObjects.udfLabel>> GetUDFLabels(Guid TenantId, bool includeFilterOptions = true);
    Task<DataObjects.User> GetUser(Guid TenantId, string UserName);
    Task<DataObjects.User> GetUser(Guid UserId, bool ValidateMainAdminAccess = false);
    Task<DataObjects.User> GetUserByEmailAddress(Guid TenantId, string EmailAddress, bool AddIfNotFound = true);
    Task<DataObjects.User> GetUserByEmployeeId(Guid TenantId, string EmployeeId, bool AddIfNotFound = false);
    Task<DataObjects.User> GetUserByUsername(Guid TenantId, string Username, bool AddIfNotFound = false);
    Task<DataObjects.User> GetUserByUsernameOrEmail(Guid TenantId, string search, bool AddIfNotFound = true);
    Task<string> GetUserDisplayName(Guid? UserId);
    Task<DataObjects.User> GetUserFromToken(Guid TenantId, string Token);
    Task<DataObjects.UserGroup> GetUserGroup(Guid GroupId, bool IncludeUsers = false);
    Task<List<DataObjects.UserGroup>> GetUserGroups(Guid TenantId, bool IncludeUsers = false);
    Task<Guid?> GetUserPhoto(Guid UserId);
    Task<List<DataObjects.User>> GetUsers(Guid TenantId);
    Task<DataObjects.FilterUsers> GetUsersFiltered(DataObjects.FilterUsers filter);
    Task<List<DataObjects.User>> GetUsersInDepartment(Guid TenantId, Guid DepartmentId);
    Task<List<DataObjects.UserTenant>> GetUserTenantList(string? username, string? email, bool enabledUsersOnly = true);
    Task<List<DataObjects.Tenant>?> GetUserTenants(string? username, string? email, bool enabledUsersOnly = true);
    string GetUserToken(Guid TenantId, Guid UserId);
    Guid GuidValue(Guid? guid);
    int IntValue(int? value);
    string JsonWebTokenKey(Guid TenantId);
    Dictionary<string, object> JwtDecode(Guid TenantId, string Encrypted);
    string JwtEncode(Guid TenantId, Dictionary<string, object> Payload);
    List<string> MessageToListOfString(string message);
    double NowFromUnixEpoch();
    string QueryStringValue(string valueName);
    void Redirect(string url);
    DateTime Released { get; }
    Task<DataObjects.BooleanResponse> RemoveUserFromGroup(Guid UserId, Guid GroupId);
    string Replace(string input, string replaceText, string withText);
    string Request(string parameter);
    Task<DataObjects.BooleanResponse> ResetUserPassword(DataObjects.UserPasswordReset reset, DataObjects.User currentUser);
    double RunningSince { get; }
    Task<DataObjects.ApplicationSettings> SaveApplicationSettings(DataObjects.ApplicationSettings settings, DataObjects.User CurrentUser);
    Task<DataObjects.Department> SaveDepartment(DataObjects.Department department);
    Task<DataObjects.DepartmentGroup> SaveDepartmentGroup(DataObjects.DepartmentGroup departmentGroup);
    Task<List<DataObjects.Department>> SaveDepartments(List<DataObjects.Department> departments);
    Task<DataObjects.FileStorage> SaveFileStorage(DataObjects.FileStorage fileStorage);
    Task<List<DataObjects.FileStorage>> SaveFileStorages(List<DataObjects.FileStorage> fileStorages);
    Task<DataObjects.BooleanResponse> SaveLanguage(Guid TenantId, DataObjects.Language language);
    Task<DataObjects.Setting> SaveSetting(DataObjects.Setting setting, Guid? TenantId = null, Guid? UserId = null);
    DataObjects.BooleanResponse SaveSetting(string SettingName, DataObjects.SettingType SettingType, dynamic? Value, Guid? TenantId = null, Guid? UserId = null, string? Description = "");
    Task<DataObjects.Tenant> SaveTenant(DataObjects.Tenant tenant);
    void SaveTenantSettings(Guid TenantId, DataObjects.TenantSettings settings);
    Task<DataObjects.BooleanResponse> SaveUDFLabels(Guid TenantId, List<DataObjects.udfLabel> labels);
    Task<DataObjects.User> SaveUser(DataObjects.User user);
    Task<DataObjects.User> SaveUserByUsername(DataObjects.User user, bool CreateIfNotFound);
    Task<DataObjects.UserGroup> SaveUserGroup(DataObjects.UserGroup Group);
    Task<List<DataObjects.User>> SaveUsers(List<DataObjects.User> users);
    DataObjects.BooleanResponse SendEmail(DataObjects.EmailMessage message, DataObjects.MailServerConfig? config = null);
    string SerializeObject(object? Object);
    string Serialize_ObjectToXml(object o, bool OmitXmlDeclaration = true);
    T? Serialize_XmlToObject<T>(string? xml);
    void SetHttpContext(Microsoft.AspNetCore.Http.HttpContext context);
    Task SignalRUpdate(DataObjects.SignalRUpdate update);
    DataObjects.SignalRUpdateType SignalRUpdateTypeFromString(string updateType);
    string SignalRUpdateTypeToString(DataObjects.SignalRUpdateType updateType);
    string StringValue(string? input);
    Task<DataObjects.User> UnlockUserAccount(Guid UserId);
    Task<DataObjects.User?> UpdateUserFromExternalDataSources(DataObjects.User User, DataObjects.TenantSettings? settings = null);
    DataObjects.User? UpdateUserFromSsoSettings(DataObjects.User User, SSO.Auth.SingleSignOn ssoInfo);
    Task UpdateUserLastLoginTime(Guid UserId);
    string UrlDecode(string? input);
    string UrlEncode(string? input);
    Task<bool> UserCanEditUser(Guid UserId, Guid EditUserId);
    Task<bool> UserCanViewUser(Guid UserId, Guid ViewUserId);
    Task<bool> UserIsMainAdmin(Guid UserId);
    Task<DataObjects.User> UserSignup(DataObjects.User user);
    Task<DataObjects.User> UserSignupConfirm(DataObjects.User user);
    Task<DataObjects.BooleanResponse> ValidateSelectedUserAccount(Guid TenantId, Guid UserId);
    bool ValidateSourceJWT(Guid TenantId, string Source, string JWT);
    string Version { get; }
    DataObjects.VersionInfo VersionInfo { get; }
}