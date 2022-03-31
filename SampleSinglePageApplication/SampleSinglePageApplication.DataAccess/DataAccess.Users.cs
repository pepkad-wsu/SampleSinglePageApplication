namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task<DataObjects.User> CreateNewUserFromEmailAddress(Guid TenantId, string EmailAddress)
    {
        DataObjects.User output = new DataObjects.User();
        // First, make sure the user doesn't already exist

        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Email == EmailAddress);
        if (rec != null) {
            // Already exists
            output = await GetUser(rec.UserId);
            output.ActionResponse.Messages.Add("User Already Exists");
            return output;
        }

        // Create a new user record
        Guid UserId = Guid.NewGuid();
        rec = new EFModels.EFModels.User();
        rec.Admin = false;
        rec.DepartmentId = null;
        rec.Email = EmailAddress;
        rec.Enabled = true;
        rec.FirstName = "";
        rec.LastName = EmailAddress;
        rec.UserId = UserId;
        rec.Username = EmailAddress;
        try {
            data.Users.Add(rec);
            await data.SaveChangesAsync();
            output = await GetUser(UserId);
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("There was an error adding a new user for '" + EmailAddress + "' - " + ex.Message);
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteUser(Guid UserId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == UserId);
        if (rec == null) {
            output.Messages.Add("Error Deleting User " + UserId.ToString() + " - Record No Longer Exists");
            return output;
        } else {
            // First, fix or delete all relational user records
            try {
                data.FileStorages.RemoveRange(data.FileStorages.Where(x => x.UserId == UserId));
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Related User Records for User " + UserId.ToString() + " - " + ex.Message);
                return output;
            }

            Guid tenantId = GuidOrEmpty(rec.TenantId);

            // Now, delete the main user record
            data.Users.Remove(rec);

            try {
                await data.SaveChangesAsync();
                output.Result = true;

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = tenantId,
                    ItemId = UserId.ToString(),
                    UpdateType = DataObjects.SignalRUpdateType.Setting,
                    Message = "DeletedUser"
                });

            } catch (Exception ex) {
                output.Messages.Add("Error Deleting User " + UserId.ToString() + " - " + ex.Message);
            }
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteUserPhoto(Guid UserId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.ItemId == null && x.UserId == UserId);
        if (rec != null) {
            data.FileStorages.Remove(rec);
            Guid tenantId = GuidOrEmpty(rec.TenantId);
            try {
                await data.SaveChangesAsync();
                output.Result = true;

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = tenantId,
                    ItemId = UserId.ToString(),
                    UpdateType = DataObjects.SignalRUpdateType.Setting,
                    Message = "DeletedTechPhoto"
                });
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Photo for UserId '" + UserId.ToString() + "' - " + ex.Message);
            }
        } else {
            output.Messages.Add("File '" + UserId.ToString() + "' no longer exists");
        }

        return output;
    }

    public string DisplayNameFromLastAndFirst(string? LastName, string? FirstName, string? Email, string? DepartmentName, string? Location)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(FirstName)) {
            output += FirstName;
        }

        if (!String.IsNullOrEmpty(LastName)) {
            if (!String.IsNullOrEmpty(output)) {
                output += " ";
            }
            output += LastName;
        }

        if (String.IsNullOrEmpty(output) && !String.IsNullOrEmpty(Email)) {
            output = Email;
        }

        if (!String.IsNullOrEmpty(DepartmentName) || !String.IsNullOrEmpty(Location)) {
            output += " [";
            if (!String.IsNullOrEmpty(Location) && !String.IsNullOrEmpty(DepartmentName)) {
                output += Location + "/" + DepartmentName;
            } else if (!String.IsNullOrEmpty(Location)) {
                output += Location;
            } else {
                output += DepartmentName;
            }

            output += "]";
        }
        return output;
    }

    public async Task<string> GetDisplayNameFromUserId(Guid? UserId, bool LastNameFirst = false)
    {
        string output = String.Empty;
        if (UserId.HasValue && UserId != Guid.Empty) {
            var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == (Guid)UserId);
            if (rec != null) {
                if (LastNameFirst) {
                    string deptName = String.Empty;
                    if (rec.Department != null && !String.IsNullOrEmpty(rec.Department.DepartmentName)) {
                        deptName = rec.Department.DepartmentName;
                    }

                    output = DisplayNameFromLastAndFirst(rec.LastName, rec.FirstName, rec.Email, deptName, rec.Location);
                } else {
                    output = rec.FirstName + " " + rec.LastName;
                }
            }
        }
        return output;
    }

    public async Task<string> GetEmailFromUserId(Guid? UserId)
    {
        string output = String.Empty;
        if (UserId.HasValue && UserId != Guid.Empty) {
            var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == (Guid)UserId);
            if (rec != null && !String.IsNullOrEmpty(rec.Email)) {
                output = rec.Email;
            }
        }
        return output;
    }

    private async Task<DataObjects.User> GetExistingUser(Guid TenantId, string Lookup, DataObjects.UserLookupType Type, bool AddIfNotFound = true)
    {
        DataObjects.User output = new DataObjects.User();
        string DepartmentName = String.Empty;
        var ldapRoot = GetSetting<string>("activedirectoryroot", DataObjects.SettingType.Text);
        Guid DepartmentId = Guid.Empty;

        // Try and update the user info from Active Directory
        var ldapQueryUsername = GetSetting<string>("LdapUsername", DataObjects.SettingType.EncryptedText);
        var ldapQueryPassword = GetSetting<string>("LdapPassword", DataObjects.SettingType.EncryptedText);

        if (String.IsNullOrWhiteSpace(ldapQueryUsername) || String.IsNullOrWhiteSpace(ldapQueryPassword)) {
            ldapQueryUsername = "";
            ldapQueryPassword = "";
        }

        string ldapOptionalLocationAttribute = GetLdapOptionalLocationAttribute();
        DataObjects.ActiveDirectoryUserInfo? adUserInfo = GetActiveDirectoryInfo(Lookup, Type, StringOrEmpty(ldapRoot), ldapQueryUsername, ldapQueryPassword, ldapOptionalLocationAttribute);

        EFModels.EFModels.User? rec = null;

        switch (Type) {
            case DataObjects.UserLookupType.Email:
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Email == Lookup);
                break;

            case DataObjects.UserLookupType.EmployeeId:
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.EmployeeId == Lookup);
                break;

            case DataObjects.UserLookupType.Guid:
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.UserId.ToString() == Lookup);
                break;

            case DataObjects.UserLookupType.Username:
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username == Lookup);
                break;
        }

        bool newRecord = false;

        if (rec != null) {
            output = await GetUser(rec.UserId);

            if (adUserInfo != null) {
                bool UserUpdated = false;
                DepartmentId = await DepartmentIdFromActiveDirectoryName(TenantId, adUserInfo.Department);
                if (DepartmentId != Guid.Empty && DepartmentId != output.DepartmentId) {
                    output.DepartmentId = DepartmentId;
                    UserUpdated = true;
                }
                if (!String.IsNullOrWhiteSpace(adUserInfo.Username) && output.Username != adUserInfo.Username) {
                    output.Username = adUserInfo.Username;
                    UserUpdated = true;
                }
                if (!String.IsNullOrWhiteSpace(adUserInfo.FirstName) && output.FirstName != adUserInfo.FirstName) {
                    output.FirstName = adUserInfo.FirstName;
                    UserUpdated = true;
                }
                if (!String.IsNullOrWhiteSpace(adUserInfo.LastName) && output.LastName != adUserInfo.LastName) {
                    output.LastName = adUserInfo.LastName;
                    UserUpdated = true;
                }
                if (!String.IsNullOrWhiteSpace(adUserInfo.Phone) && output.Phone != adUserInfo.Phone) {
                    output.Phone = adUserInfo.Phone;
                    UserUpdated = true;
                }
                if (!String.IsNullOrWhiteSpace(adUserInfo.EmployeeId) && output.EmployeeId != adUserInfo.EmployeeId) {
                    output.EmployeeId = adUserInfo.EmployeeId;
                    UserUpdated = true;
                }
                if (UserUpdated) {
                    await SaveUser(output);
                }
            }

        } else {
            if (adUserInfo != null) {
                string FirstName = StringOrEmpty(adUserInfo.FirstName);
                string LastName = StringOrEmpty(adUserInfo.LastName);

                // Create a new user record
                if (String.IsNullOrWhiteSpace(FirstName) && String.IsNullOrWhiteSpace(LastName)) {
                    // If first and last are both empty from AD just set the last name to the username
                    LastName = Lookup;
                }

                Guid UserId = Guid.NewGuid();

                // If a valid Guid was returned from AD try and use that Guid.
                if (adUserInfo.UserId.HasValue && adUserInfo.UserId != Guid.Empty) {
                    UserId = (Guid)adUserInfo.UserId;
                }

                DepartmentId = await DepartmentIdFromActiveDirectoryName(TenantId, adUserInfo.Department);

                // Before just adding a user see if we can match an existing user based on the returned email address from AD.
                if (!String.IsNullOrWhiteSpace(adUserInfo.Email)) {
                    rec = await data.Users.FirstOrDefaultAsync(x => x.Email == adUserInfo.Email);
                } else if (!String.IsNullOrWhiteSpace(adUserInfo.Username)) {
                    rec = await data.Users.FirstOrDefaultAsync(x => x.Username == adUserInfo.Username);
                }

                if (rec == null) {
                    // Add a new user. These values only get updated when adding, not if this is just an update.
                    rec = new EFModels.EFModels.User();
                    rec.UserId = UserId;
                    rec.TenantId = TenantId;
                    rec.Admin = false;
                    rec.Enabled = true;
                    rec.Email = Type == DataObjects.UserLookupType.Email ? Lookup : adUserInfo.Email;
                    newRecord = true;
                } else {
                    UserId = rec.UserId;
                }

                // Only update the remaining values if we received a valid value and the record is not already set to that value.
                if (DepartmentId != Guid.Empty && rec.DepartmentId != DepartmentId) {
                    rec.DepartmentId = DepartmentId;
                }

                if (!String.IsNullOrWhiteSpace(FirstName) && rec.FirstName != FirstName) {
                    rec.FirstName = FirstName;
                }

                if (!String.IsNullOrWhiteSpace(LastName) && rec.LastName != LastName) {
                    rec.LastName = LastName;
                }

                if (!String.IsNullOrWhiteSpace(adUserInfo.Username) && rec.Username != adUserInfo.Username) {
                    rec.Username = adUserInfo.Username;
                }

                if (!String.IsNullOrWhiteSpace(adUserInfo.EmployeeId) && rec.EmployeeId != adUserInfo.EmployeeId) {
                    rec.EmployeeId = adUserInfo.EmployeeId;
                }

                try {
                    if (newRecord) {
                        data.Users.Add(rec);
                    }
                    await data.SaveChangesAsync();
                    output = await GetUser(UserId);
                } catch (Exception ex) {
                    output.ActionResponse.Messages.Add("There was an error adding a new user with an Lookup Value of '" + Lookup + "' - " + ex.Message);
                }

            } else if (AddIfNotFound) {
                // Just add a new user with only the lookup value
                Guid UserId = Guid.NewGuid();

                if (Type == DataObjects.UserLookupType.Guid) {
                    // Try and use the supplied Guid
                    Guid providedGuid = Guid.Empty;
                    try {
                        providedGuid = new Guid(Lookup);
                    } catch { }
                    if (providedGuid != Guid.Empty) {
                        UserId = providedGuid;
                    }
                }

                newRecord = true;
                rec = new EFModels.EFModels.User();
                rec.Admin = false;
                rec.DepartmentId = null;
                rec.Email = Type == DataObjects.UserLookupType.Email ? Lookup : String.Empty;
                rec.EmployeeId = Type == DataObjects.UserLookupType.EmployeeId ? Lookup : String.Empty;
                rec.Enabled = true;
                rec.FirstName = "";
                rec.LastName = Lookup;
                rec.UserId = UserId;
                rec.Username = Type == DataObjects.UserLookupType.Username ? Lookup : String.Empty;
                try {
                    if (newRecord) {
                        data.Users.Add(rec);
                    }
                    await data.SaveChangesAsync();
                    output = await GetUser(UserId);
                } catch (Exception ex) {
                    output.ActionResponse.Messages.Add("There was an error adding a new user for '" + Lookup + "' - " + ex.Message);
                }
            } else {
                output.ActionResponse.Messages.Add("Unable to locate a user with the Lookup Value of '" + Lookup + "' in the local database or in Active Directory");
            }
        }

        return output;
    }

    public async Task<string> GetFirstNameFromUserId(Guid? UserId)
    {
        string output = String.Empty;
        if (UserId.HasValue && UserId != Guid.Empty) {
            var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == (Guid)UserId);
            if (rec != null && !String.IsNullOrEmpty(rec.FirstName)) {
                output = rec.FirstName;
            }
        }
        return output;
    }

    public async Task<DataObjects.User> GetUser(Guid TenantId, string UserName)
    {
        DataObjects.User output = new DataObjects.User();

        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username == UserName);
        if (rec != null) {
            output = await GetUser(rec.UserId);
        }

        return output;
    }

    public async Task<DataObjects.User> GetUser(Guid UserId, bool ValidateMainAdminAccess = false)
    {
        DataObjects.User output = new DataObjects.User();

        output.AppAdmin = await UserIsMainAdmin(UserId);
        // If a user is a MainAdmin user, they need to have an account in every tenant.
        // That account in the tenant doesn't have to be an Admin user in that tenant, as
        // they will always be considered an Admin in a tenant if they are an admin in the
        // Admin account (Guid1). This needs to be done before getting the rest of the
        // user details, so that when we get to the point of getting all their Tenant
        // and UserTenant objects they will have the full list.
        if (output.AppAdmin && ValidateMainAdminAccess) {
            await ValidateMainAdminUser(UserId);
        }

        var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == UserId);
        if (rec != null) {
            output.ActionResponse.Result = true;
            output.TenantId = GuidOrEmpty(rec.TenantId);
            output.Admin = rec.Admin.HasValue ? (bool)rec.Admin : false;
            output.DepartmentId = rec.DepartmentId.HasValue ? (Guid)rec.DepartmentId : (Guid?)null;
            output.DepartmentName = rec.DepartmentId.HasValue && rec.Department != null
                ? rec.Department.DepartmentName
                : String.Empty;
            output.Email = rec.Email;
            output.Phone = rec.Phone;
            output.Photo = await GetUserPhoto(UserId);
            output.Enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false;
            output.FirstName = rec.FirstName;
            output.LastLogin = rec.LastLogin.HasValue ? Convert.ToDateTime(rec.LastLogin) : (DateTime?)null;
            output.LastName = rec.LastName;
            output.UserId = rec.UserId;
            output.Username = rec.Username;
            output.EmployeeId = rec.EmployeeId;
            output.Password = String.Empty;
            output.HasLocalPassword = !String.IsNullOrWhiteSpace(rec.Password);

            output.Tenants = await GetUserTenants(output.Username, output.Email);
            output.UserTenants = await GetUserTenantList(output.Username, output.Email);

            output.DisplayName = DisplayNameFromLastAndFirst(output.LastName, output.FirstName, output.Email, output.DepartmentName, output.Location);

            if(_inMemoryDatabase && output.DepartmentId.HasValue && String.IsNullOrEmpty(output.DepartmentName)) {
                output.DepartmentName = GetDepartmentName(output.TenantId, GuidOrEmpty(output.DepartmentId));
            }

            if (output.UserTenants != null && output.UserTenants.Any() && output.Tenants != null && output.Tenants.Any()) {
                // The Tenant Code and Name aren't loaded in GetUserTenantList, so load those from Tenants here.
                foreach (var userTenant in output.UserTenants) {
                    var tenant = output.Tenants.FirstOrDefault(x => x.TenantId == userTenant.TenantId);
                    if (tenant != null) {
                        userTenant.TenantCode = tenant.TenantCode;
                        userTenant.TenantName = tenant.Name;
                    }
                }
            }
        }

        return output;
    }

    public async Task<DataObjects.User> GetUserByEmailAddress(Guid TenantId, string EmailAddress, bool AddIfNotFound = true)
    {
        var output = await GetExistingUser(TenantId, EmailAddress, DataObjects.UserLookupType.Email, AddIfNotFound);
        return output;
    }

    public async Task<DataObjects.User> GetUserByEmployeeId(Guid TenantId, string EmployeeId, bool AddIfNotFound = false)
    {
        var output = await GetExistingUser(TenantId, EmployeeId, DataObjects.UserLookupType.EmployeeId, AddIfNotFound);
        return output;

    }

    public async Task<DataObjects.User> GetUserByUsername(Guid TenantId, string Username, bool AddIfNotFound = false)
    {
        var output = await GetExistingUser(TenantId, Username, DataObjects.UserLookupType.Username, AddIfNotFound);
        return output;

    }

    public async Task<DataObjects.User> GetUserByUsernameOrEmail(Guid TenantId, string search, bool AddIfNotFound = true)
    {
        DataObjects.User output = new DataObjects.User();
        if (!String.IsNullOrWhiteSpace(search)) {
            if (search.Contains("@")) {
                output = await GetUserByEmailAddress(TenantId, search, AddIfNotFound);
            } else {
                output = await GetUserByUsername(TenantId, search, AddIfNotFound);
            }
        }
        return output;
    }

    public async Task<string> GetUserDisplayName(Guid? UserId)
    {
        string output = String.Empty;

        if (UserId != null) {
            DataObjects.User user = await GetUser((Guid)UserId);
            if (user.ActionResponse.Result) {
                output += user.FirstName + " " + user.LastName;
            }
        }

        return output;
    }

    public async Task<DataObjects.FilterUsers> GetUsersFiltered(DataObjects.FilterUsers filter)
    {
        DataObjects.FilterUsers output = filter;
        output.ActionResponse = GetNewActionResponse();
        output.Records = null;

        output.Columns = new List<DataObjects.FilterColumn> {
            new DataObjects.FilterColumn{ 
                Align = "center",
                Label = "",
                TipText = "",
                Sortable = false,
                DataElementName = "photo",
                DataType = "photo"
            },
            new DataObjects.FilterColumn{ 
                Align = "",
                Label = "First",
                TipText = "",
                Sortable = true,
                DataElementName = "firstName",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = "Last",
                TipText = "",
                Sortable = true,
                DataElementName = "lastName",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = "Email",
                TipText = "",
                Sortable = true,
                DataElementName = "email",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = "Username",
                TipText = "",
                Sortable = true,
                DataElementName = "username",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = "Employee Id",
                TipText = "",
                Sortable = true,
                DataElementName = "employeeId",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = "Department",
                TipText = "",
                Sortable = true,
                DataElementName = "departmentName",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "center",
                Label = "Enabled",
                TipText = "",
                Sortable = true,
                DataElementName = "enabled",
                DataType = "boolean"
            },
            new DataObjects.FilterColumn{
                Align = "center",
                Label = "Admin",
                TipText = "",
                Sortable = true,
                DataElementName = "admin",
                DataType = "boolean"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = "Last Login",
                TipText = "",
                Sortable = true,
                DataElementName = "lastLogin",
                DataType = "datetime"
            }
        };

        var recs = data.Users.Where(x => x.TenantId == output.TenantId && x.Username != "admin");

        if (output.FilterDepartments != null && output.FilterDepartments.Count() > 0) {
            recs = recs.Where(x => x.DepartmentId != null && output.FilterDepartments.Contains((Guid)x.DepartmentId));
        }

        if (!String.IsNullOrWhiteSpace(output.Enabled)) {
            switch (output.Enabled.ToUpper()) {
                case "ENABLED":
                    recs = recs.Where(x => x.Enabled == true);
                    break;

                case "DISABLED":
                    recs = recs.Where(x => x.Enabled != true);
                    break;
            }
        }

        if (!String.IsNullOrWhiteSpace(output.Admin)) {
            switch (output.Admin.ToUpper()) {
                case "ADMIN":
                    recs = recs.Where(x => x.Admin == true);
                    break;

                case "STANDARD":
                    recs = recs.Where(x => x.Admin != true);
                    break;
            }
        }

        // Add any filters
        if (!String.IsNullOrEmpty(output.Keyword)) {
            // Dynamically include only the UDF fields that are needed
            recs = recs.Where(x => (x.LastName != null && x.LastName.Contains(output.Keyword))
                || (x.FirstName != null && x.FirstName.Contains(output.Keyword))
                || (x.Email != null && x.Email.Contains(output.Keyword))
                || (x.Username != null && x.Username.Contains(output.Keyword))
            );
        }

        bool Ascending = true;
        if (StringOrEmpty(output.SortOrder).ToUpper() == "DESC") {
            Ascending = false;
        }
        switch (StringOrEmpty(output.Sort).ToUpper()) {
            case "LAST":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
                }
                break;

            case "FIRST":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.FirstName).ThenBy(x => x.LastName);
                } else {
                    recs = recs.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName);
                }
                break;

            case "EMAIL":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.Email);
                } else {
                    recs = recs.OrderByDescending(x => x.Email);
                }
                break;

            case "EMPLOYEEID":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.EmployeeId).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderByDescending(x => x.EmployeeId).ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
                }
                break;

            case "USERNAME":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.Username);
                } else {
                    recs = recs.OrderByDescending(x => x.Username);
                }
                break;

            case "DEPARTMENT":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.Department.DepartmentName).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderByDescending(x => x.Department.DepartmentName).ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
                }
                break;

            case "ENABLED":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.Enabled == false).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderBy(x => x.Enabled == true).ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
                }
                break;

            case "ADMIN":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.Admin == false).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderBy(x => x.Admin == true).ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
                }
                break;

            case "LASTLOGIN":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.LastLogin).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderByDescending(x => x.LastLogin).ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
                }
                break;
        }

        if (recs != null && recs.Count() > 0) {

            int TotalRecords = recs.Count();
            output.RecordCount = TotalRecords;

            if (output.RecordsPerPage > 0) {
                // We are filtering records per page
                if (output.RecordsPerPage >= TotalRecords) {
                    output.Page = 1;
                    output.PageCount = 1;
                } else {
                    // Figure out the page count
                    if (output.Page < 1) { output.Page = 1; }
                    if (output.RecordsPerPage < 1) { output.RecordsPerPage = 25; }
                    decimal decPages = (decimal)TotalRecords / (decimal)output.RecordsPerPage;
                    decPages = Math.Ceiling(decPages);
                    output.PageCount = (int)decPages;

                    if (output.Page > output.PageCount) {
                        output.Page = output.PageCount;
                    }

                    if (output.Page > 1) {
                        recs = recs.Skip((output.Page - 1) * output.RecordsPerPage).Take(output.RecordsPerPage);
                    } else {
                        recs = recs.Take(output.RecordsPerPage);
                    }

                }
            }

            List<DataObjects.User> records = new List<DataObjects.User>();

            List<DataObjects.Department> departments = new List<DataObjects.Department>();
            if (_inMemoryDatabase) {
                departments = await GetDepartments(output.TenantId);
            }

            foreach (var rec in recs) {
                var u = new DataObjects.User {
                    ActionResponse = GetNewActionResponse(true),
                    Admin = rec.Admin.HasValue ? (bool)rec.Admin : false,
                    DepartmentId = rec.DepartmentId.HasValue ? (Guid)rec.DepartmentId : (Guid?)null,
                    DepartmentName = rec.DepartmentId.HasValue && rec.Department != null ? rec.Department.DepartmentName : String.Empty,
                    Email = rec.Email,
                    EmployeeId = rec.EmployeeId,
                    Enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false,
                    FirstName = rec.FirstName,
                    LastLogin = rec.LastLogin.HasValue ? (DateTime)rec.LastLogin : (DateTime?)null,
                    LastName = rec.LastName,
                    Phone = rec.Phone,
                    UserId = rec.UserId,
                    Username = rec.Username,
                    Photo = await GetUserPhoto(rec.UserId),
                    HasLocalPassword = !String.IsNullOrWhiteSpace(rec.Password)
                };

                if(_inMemoryDatabase && u.DepartmentId.HasValue && String.IsNullOrEmpty(u.DepartmentName)) {
                    var dept = departments.FirstOrDefault(x => x.DepartmentId == u.DepartmentId);
                    if(dept != null) {
                        u.DepartmentName = dept.DepartmentName;
                    }
                }

                u.DisplayName = DisplayNameFromLastAndFirst(u.LastName, u.FirstName, u.Email, u.DepartmentName, u.Location);

                records.Add(u);
            }

            output.Records = records.ToArray();
        }

        output.ActionResponse.Result = true;

        return output;
    }

    public async Task<DataObjects.User> GetUserFromToken(Guid TenantId, string Token)
    {
        DataObjects.User output = new DataObjects.User();

        Guid UserId = Guid.Empty;
        Dictionary<string, object> decrypted = JwtDecode(TenantId, Token);
        try {
            string guid = decrypted["UserId"] + String.Empty;
            UserId = new Guid(guid);
        } catch { }
        if (UserId != Guid.Empty) {
            output = await GetUser(UserId);
        }

        return output;
    }

    public async Task<Guid?> GetUserPhoto(Guid UserId)
    {
        Guid? output = (Guid?)null;
        try {
            var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.ItemId == null && x.UserId == UserId);
            if (rec != null) {
                output = rec.FileId;
            }
        } catch (Exception ex) {
            if (ex != null) {

            }
        }
        return output;
    }

    public async Task<List<DataObjects.User>> GetUsers(Guid TenantId)
    {
        List<DataObjects.User> output = new List<DataObjects.User>();

        var recs = await data.Users.Where(x => x.TenantId == TenantId).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync();
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                var u = new DataObjects.User {
                    ActionResponse = GetNewActionResponse(true),
                    TenantId = TenantId,
                    Admin = rec.Admin.HasValue ? (bool)rec.Admin : false,
                    DepartmentId = rec.DepartmentId.HasValue ? (Guid)rec.DepartmentId : (Guid?)null,
                    DepartmentName = rec.DepartmentId.HasValue && rec.Department != null ? rec.Department.DepartmentName : String.Empty,
                    Email = rec.Email,
                    Phone = rec.Phone,
                    Enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false,
                    FirstName = rec.FirstName,
                    LastLogin = rec.LastLogin,
                    LastName = rec.LastName,
                    Photo = await GetUserPhoto(rec.UserId),
                    UserId = rec.UserId,
                    Username = rec.Username,
                    EmployeeId = rec.EmployeeId,
                    Password = String.Empty,
                    HasLocalPassword = !String.IsNullOrWhiteSpace(rec.Password)
                };

                u.DisplayName = DisplayNameFromLastAndFirst(u.LastName, u.FirstName, u.Email, u.DepartmentName, u.Location);

                output.Add(u);
            }
        }

        return output;
    }

    public async Task<List<DataObjects.User>> GetUsersInDepartment(Guid TenantId, Guid DepartmentId)
    {
        List<DataObjects.User> output = new List<DataObjects.User>();

        var recs = await data.Users.Where(x => x.TenantId == TenantId && x.DepartmentId == DepartmentId).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync();
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                var u = await GetUser(rec.UserId);
                output.Add(u);
            }
        }

        return output;
    }

    public async Task<List<DataObjects.UserTenant>> GetUserTenantList(string? username, string? email, bool enabledUsersOnly = true)
    {
        List<DataObjects.UserTenant> output = new List<DataObjects.UserTenant>();

        if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(email)) {
            var u1 = await data.Users.Where(x => x.Username == username || x.Email == email).ToListAsync();
            if (u1 != null && u1.Any()) {
                foreach (var rec in u1) {
                    bool enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false;
                    if (enabled || !enabledUsersOnly) {
                        output.Add(new DataObjects.UserTenant { UserId = rec.UserId, TenantId = GuidOrEmpty(rec.TenantId) });
                    }
                }
            }
        } else if (!String.IsNullOrEmpty(username)) {
            var u2 = await data.Users.Where(x => x.Username == username).ToListAsync();
            if (u2 != null && u2.Any()) {
                foreach (var rec in u2) {
                    bool enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false;
                    if (enabled || !enabledUsersOnly) {
                        output.Add(new DataObjects.UserTenant { UserId = rec.UserId, TenantId = GuidOrEmpty(rec.TenantId) });
                    }
                }
            }
        } else if (!String.IsNullOrEmpty(email)) {
            var u3 = await data.Users.Where(x => x.Email == email).ToListAsync();
            if (u3 != null && u3.Any()) {
                foreach (var rec in u3) {
                    bool enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false;
                    if (enabled || !enabledUsersOnly) {
                        output.Add(new DataObjects.UserTenant { UserId = rec.UserId, TenantId = GuidOrEmpty(rec.TenantId) });
                    }
                }
            }
        }

        return output;
    }

    public async Task<List<DataObjects.Tenant>?> GetUserTenants(string? username, string? email, bool enabledUsersOnly = true)
    {
        List<DataObjects.Tenant>? output = null;

        List<Guid> tenantIds = new List<Guid>();

        if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(email)) {
            var u1 = await data.Users.Where(x => x.Username == username || x.Email == email).ToListAsync();
            if (u1 != null && u1.Any()) {
                if (enabledUsersOnly) {
                    tenantIds = u1.Where(x => x.Enabled == true).Select(x => x.TenantId).Distinct().ToList();
                } else {
                    tenantIds = u1.Select(x => x.TenantId).Distinct().ToList();
                }
            }
        } else if (!String.IsNullOrEmpty(username)) {
            var u2 = await data.Users.Where(x => x.Username == username).ToListAsync();
            if (u2 != null && u2.Any()) {
                if (enabledUsersOnly) {
                    tenantIds = u2.Where(x => x.Enabled == true).Select(x => x.TenantId).Distinct().ToList();
                } else {
                    tenantIds = u2.Select(x => x.TenantId).Distinct().ToList();
                }
            }
        } else if (!String.IsNullOrEmpty(email)) {
            var u3 = await data.Users.Where(x => x.Email == email).ToListAsync();
            if (u3 != null && u3.Any()) {
                if (enabledUsersOnly) {
                    tenantIds = u3.Where(x => x.Enabled == true).Select(x => x.TenantId).Distinct().ToList();
                } else {
                    tenantIds = u3.Select(x => x.TenantId).Distinct().ToList();
                }
            }
        }

        if (tenantIds != null && tenantIds.Any()) {
            foreach (var tenantId in tenantIds) {
                var tenant = GetTenant((Guid)tenantId);
                if (tenant != null) {
                    if (output == null) {
                        output = new List<DataObjects.Tenant>();
                    }
                    output.Add(tenant);
                }
            }
        }

        return output;
    }

    public string GetUserToken(Guid TenantId, Guid UserId)
    {
        // jwtencode
        Dictionary<string, object> Payload = new Dictionary<string, object> {
            { "UserId", UserId }
        };
        string output = JwtEncode(TenantId, Payload);
        return output;
    }

    public async Task<DataObjects.BooleanResponse> ResetUserPassword(DataObjects.UserPasswordReset reset, DataObjects.User currentUser)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        if (reset == null || reset.UserId == Guid.Empty) {
            output.Messages.Add("No UserId Specified");
            return output;
        }

        // Admin users don't need to provide a current password
        if (currentUser.Admin) {
            var recForAdmin = await data.Users.FirstOrDefaultAsync(x => x.TenantId == currentUser.TenantId && x.UserId == reset.UserId);
            if(recForAdmin != null) {
                recForAdmin.Password = Encrypt(reset.NewPassword);
                await data.SaveChangesAsync();
                output.Result = true;
                output.Messages.Add("Password Reset");
                return output;
            } else {
                output.Messages.Add("Access Denied.");
                return output;
            }
        }
        
        if (String.IsNullOrWhiteSpace(reset.CurrentPassword) || String.IsNullOrWhiteSpace(reset.NewPassword)) {
            output.Messages.Add("Missing Current or New Password");
        } else {
            var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == reset.UserId);
            if (rec == null) {
                output.Messages.Add("UserId Not Found");
            } else {
                // Make sure the current password matches what was given
                if (Decrypt(StringOrEmpty(rec.Password)) != reset.CurrentPassword) {
                    output.Messages.Add("Incorrect Current Password");
                } else {
                    rec.Password = Encrypt(reset.NewPassword);
                    try {
                        await data.SaveChangesAsync();
                        output.Result = true;
                        output.Messages.Add("Password Reset");
                    } catch (Exception ex) {
                        output.Messages.Add("Error Resetting Password - " + ex.Message);
                    }
                }
            }
        }

        return output;
    }

    public async Task<DataObjects.User> SaveUser(DataObjects.User user)
    {
        user.ActionResponse = GetNewActionResponse();

        var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == user.UserId);
        bool newRecord = false;
        if (rec == null) {
            if (user.UserId == Guid.Empty) {
                rec = new EFModels.EFModels.User();
                user.UserId = Guid.NewGuid();
                rec.UserId = user.UserId;
                newRecord = true;
            } else {
                user.ActionResponse.Messages.Add("Error Saving User " + user.UserId.ToString() + " - Record No Longer Exists");
                return user;
            }
        }

        rec.TenantId = user.TenantId;
        rec.FirstName = user.FirstName;
        rec.LastName = user.LastName;
        rec.Email = user.Email;
        rec.Phone = user.Phone;
        rec.Username = user.Username;
        rec.EmployeeId = user.EmployeeId;
        rec.DepartmentId = user.DepartmentId.HasValue ? (Guid)user.DepartmentId : (Guid?)null;
        rec.Enabled = user.Enabled;
        rec.LastLogin = user.LastLogin.HasValue ? (DateTime)user.LastLogin : (DateTime?)null;
        rec.Admin = user.Admin;

        try {
            if (newRecord) {
                data.Users.Add(rec);
            }
            await data.SaveChangesAsync();
            user.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = user.TenantId,
                ItemId = user.UserId.ToString(),
                UpdateType = DataObjects.SignalRUpdateType.Setting,
                Message = "SavedUser",
                Object = user
            });
        } catch (Exception ex) {
            user.ActionResponse.Messages.Add("Error Saving User " + user.UserId.ToString() + " - " + ex.Message);
        }

        return user;
    }

    public async Task<DataObjects.User> SaveUserByUsername(DataObjects.User user, bool CreateIfNotFound)
    {
        user.ActionResponse = GetNewActionResponse();

        if (GuidOrEmpty(user.TenantId) == Guid.Empty) {
            user.ActionResponse.Messages.Add("Invalid TenantId");
            return user;
        }

        bool newRecord = false;
        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == user.TenantId && x.Username == user.Username);
        if (rec == null) {
            if (CreateIfNotFound) {
                newRecord = true;
                rec = new EFModels.EFModels.User();
                if (user.UserId == Guid.Empty) {
                    // Only create a new UserId if we don't have one, otherwise, use the one supplied
                    user.UserId = Guid.NewGuid();
                }
                rec.TenantId = user.TenantId;
                rec.UserId = user.UserId;
            } else {
                user.ActionResponse.Messages.Add("Error Saving User " + user.UserId.ToString() + " - Record No Longer Exists");
                return user;
            }
        }

        rec.FirstName = user.FirstName;
        rec.LastName = user.LastName;
        rec.Email = user.Email;
        rec.Phone = user.Phone;
        rec.Username = user.Username;
        rec.EmployeeId = user.EmployeeId;
        rec.DepartmentId = user.DepartmentId.HasValue ? (Guid)user.DepartmentId : (Guid?)null;

        if (!user.DepartmentId.HasValue && !String.IsNullOrEmpty(user.DepartmentName)) {
            var deptId = await DepartmentIdFromActiveDirectoryName(user.TenantId, user.DepartmentName);
            if(deptId != Guid.Empty) {
                rec.DepartmentId = deptId;
            }
        }

        rec.Enabled = user.Enabled;
        rec.LastLogin = user.LastLogin.HasValue ? (DateTime)user.LastLogin : (DateTime?)null;
        rec.Admin = user.Admin;

        try {
            if (newRecord) {
                data.Users.Add(rec);
            }
            await data.SaveChangesAsync();
            user.ActionResponse.Result = true;
        } catch (Exception ex) {
            user.ActionResponse.Messages.Add("Error Saving User " + user.UserId.ToString() + " - " + ex.Message);
        }

        return user;
    }

    public async Task<List<DataObjects.User>> SaveUsers(List<DataObjects.User> users)
    {
        List<DataObjects.User> output = new List<DataObjects.User>();
        foreach (var user in users) {
            var saved = await SaveUser(user);
            output.Add(saved);
        }
        return output;
    }

    public async Task UpdateUserLastLoginTime(Guid UserId)
    {
        if (_inMemoryDatabase) {
            var rec = data.Users.FirstOrDefault(x => x.UserId == UserId);
            if(rec != null) {
                rec.LastLogin = DateTime.Now;
                data.SaveChanges();
            }
        } else {
            await data.Database.ExecuteSqlRawAsync("UPDATE Users SET LastLogin={0} WHERE UserId={1}", DateTime.Now, UserId);
        }
    }

    public async Task<bool> UserCanEditUser(Guid UserId, Guid EditUserId)
    {
        if (UserId == EditUserId) {
            return true;
        }

        DataObjects.User u = await GetUser(UserId);
        if (u.Admin) {
            return true;
        }

        return false;
    }

    public async Task<bool> UserCanViewUser(Guid UserId, Guid ViewUserId)
    {
        DataObjects.User u = await GetUser(UserId);
        if (u.Admin) {
            // An admin or Tech can view any user
            return true;
        }

        DataObjects.User ViewUser = await GetUser(ViewUserId);
        if (ViewUser.UserId == u.UserId) {
            // This is the user's own account
            return true;
        }

        return false;
    }

    public async Task<bool> UserIsMainAdmin(Guid UserId)
    {
        bool output = false;

        var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == UserId && x.Enabled == true);
        if (rec != null) {
            var adminUser = await data.Users.FirstOrDefaultAsync(x => x.TenantId == _guid1 && x.Admin == true && x.Enabled == true &&
                ((x.Username != null && x.Username != "" && x.Username == rec.Username)
                ||
                (x.Email != null && x.Email != "" && x.Email == rec.Email))
            );
            output = adminUser != null;
        }

        return output;
    }

    private void ValidateAdminUserExists()
    {
        try {
            // Make sure there is an Admin record in the admin tenant
            bool newRecord = false;
            string adminPassword = String.Empty;
            var rec = data.Users.FirstOrDefault(x => x.TenantId == _guid1 && x.UserId == _guid1);
            if (rec == null) {
                rec = new EFModels.EFModels.User {
                    TenantId = _guid1,
                    UserId = _guid1
                };
                newRecord = true;
            }
            rec.FirstName = "Admin";
            rec.LastName = "User";
            rec.Email = "admin@local";
            rec.Username = "admin";
            rec.EmployeeId = "app.admin";
            rec.Enabled = true;
            rec.Admin = true;
            if (String.IsNullOrEmpty(rec.Password)) {
                adminPassword = "admin";
                rec.Password = Encrypt("admin");
            } else {
                adminPassword = Decrypt(rec.Password);
            }

            if (newRecord) {
                data.Users.Add(rec);
            }

            // Next, make sure that an admin user exists in each tenant account using the same password that is set on the admin tenant account.
            var tenants = data.Tenants.Where(x => x.TenantId != _guid1);
            if (tenants != null && tenants.Any()) {
                foreach (var tenant in tenants) {
                    newRecord = false;
                    var tenantAdmin = data.Users.FirstOrDefault(x => x.TenantId == tenant.TenantId && x.Username == "admin");
                    if (tenantAdmin == null) {
                        tenantAdmin = new EFModels.EFModels.User {
                            TenantId = tenant.TenantId,
                            UserId = Guid.NewGuid()
                        };
                        newRecord = true;
                    }
                    tenantAdmin.FirstName = "Admin";
                    tenantAdmin.LastName = "User";
                    tenantAdmin.Email = "admin@local";
                    tenantAdmin.Username = "admin";
                    tenantAdmin.EmployeeId = "app.admin";
                    tenantAdmin.Enabled = true;
                    tenantAdmin.Admin = true;
                    tenantAdmin.Password = Encrypt(adminPassword);

                    if (newRecord) {
                        data.Users.Add(tenantAdmin);
                    }
                }
            }
            data.SaveChanges();
        } catch { }
    }

    private async Task ValidateMainAdminUser(Guid UserId)
    {
        var tenants = await GetTenants();
        if (tenants != null && tenants.Any()) {
            var user = await data.Users.FirstOrDefaultAsync(x => x.UserId == UserId);
            if (user != null) {
                foreach (var tenant in tenants.Where(x => x.TenantId != _guid1)) {
                    var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == tenant.TenantId
                        && (
                            (x.Username != null && x.Username != "" && x.Username == user.Username)
                            ||
                            (x.Email != null && x.Email != "" && x.Email == user.Email)
                        ));

                    if (rec == null) {
                        rec = new EFModels.EFModels.User {
                            UserId = Guid.NewGuid(),
                            TenantId = tenant.TenantId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Phone = user.Phone,
                            Username = user.Username,
                            EmployeeId = user.EmployeeId,
                            Enabled = true,
                            Admin = false,
                            Password = user.Password
                        };

                        await data.Users.AddAsync(rec);
                        await data.SaveChangesAsync();
                    }
                }
            }
        }
    }

    public async Task<DataObjects.BooleanResponse> ValidateSelectedUserAccount(Guid TenantId, Guid UserId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        // See if this is an existing user
        DataObjects.User user = await GetUser(UserId);
        if (user.ActionResponse.Result) {
            output.Result = true;
            output.Messages.Add("User Already Exists");
            return output;
        }

        // Not a current user, see if we can find and add this user via AD
        var ldapRoot = GetSetting<string>("activedirectoryroot", DataObjects.SettingType.Text);
        if (!String.IsNullOrWhiteSpace(ldapRoot)) {
            var ldapQueryUsername = GetSetting<string>("LdapUsername", DataObjects.SettingType.EncryptedText);
            var ldapQueryPassword = GetSetting<string>("LdapPassword", DataObjects.SettingType.EncryptedText);

            if (String.IsNullOrWhiteSpace(ldapQueryUsername) || String.IsNullOrWhiteSpace(ldapQueryPassword)) {
                ldapQueryUsername = "";
                ldapQueryPassword = "";
            }

            var ldapOptionalLocationAttribute = GetLdapOptionalLocationAttribute();
            var adUser = GetActiveDirectoryInfo(UserId, ldapRoot, ldapQueryUsername, ldapQueryPassword, ldapOptionalLocationAttribute);
            if (adUser != null) {
                // Add a new user for this account unless we have a GUID conflict for some reason
                Guid? adUserId = adUser.UserId;
                if (adUserId.HasValue) {
                    DataObjects.User conflictUser = await GetUser((Guid)adUserId);
                    if (conflictUser.ActionResponse.Result) {
                        // This GUID exists
                        output.Messages.Add("There is a conflict trying to add UserId '" + ((Guid)adUserId).ToString() + "'");
                        return output;
                    } else {
                        // Add or update this user
                        Guid DepartmentId = await DepartmentIdFromActiveDirectoryName(TenantId, adUser.Department);

                        string Message = "User Updated from Active Directory Lookup";
                        bool newRecord = false;
                        var newUser = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username == adUser.Username);
                        if (newUser == null) {
                            newUser = new EFModels.EFModels.User();
                            newUser.UserId = (Guid)adUserId;
                            newUser.TenantId = TenantId;
                            newUser.Enabled = true;
                            newRecord = true;
                            Message = "User Added from Active Directory Lookup";
                        }

                        newUser.DepartmentId = DepartmentId != Guid.Empty ? DepartmentId : (Guid?)null;
                        newUser.Username = StringOrEmpty(adUser.Username);
                        newUser.FirstName = adUser.FirstName;
                        newUser.LastName = adUser.LastName;
                        newUser.Email = adUser.Email;
                        newUser.Phone = adUser.Phone;
                        newUser.EmployeeId = adUser.EmployeeId;
                        try {
                            if (newRecord) {
                                data.Users.Add(newUser);
                            }
                            await data.SaveChangesAsync();
                            output.Result = true;
                            output.Messages.Add(Message);

                            if (newUser.UserId != adUserId) {
                                output.Messages.Add("ADUserId:" + adUserId.ToString());
                                output.Messages.Add("LocalUserId:" + newUser.UserId.ToString());
                            }

                        } catch (Exception ex) {
                            output.Messages.Add("There was an error adding/updating the AD User '" + ((Guid)adUserId).ToString() + "' - " + ex.Message);
                        }
                    }
                }
            }
        }

        if (!output.Result && output.Messages.Count() == 0) {
            output.Messages.Add("Unable to Validate UserId '" + UserId.ToString() + "'");
        }
        return output;
    }


}