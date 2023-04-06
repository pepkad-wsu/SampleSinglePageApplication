namespace SampleSinglePageApplication;
public partial class DataAccess
{
    private DataObjects.User CopyUser(DataObjects.User user)
    {
        DataObjects.User output = new DataObjects.User();
        var dup = DuplicateObject<DataObjects.User>(user);
        if (dup != null) {
            output = dup;
        }
        return output;
    }

    public async Task<DataObjects.User> CreateNewUserFromEmailAddress(Guid TenantId, string EmailAddress)
    {
        DataObjects.User output = new DataObjects.User();
        // First, make sure the user doesn't already exist

        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Email != null && x.Email.ToLower() == EmailAddress.ToLower());
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

            Guid tenantId = GuidValue(rec.TenantId);

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
            Guid tenantId = GuidValue(rec.TenantId);
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

    public async Task<DataObjects.User> ForgotPassword(DataObjects.User user)
    {
        DataObjects.User output = new DataObjects.User();
        output.ActionResponse = GetNewActionResponse();

        var applicationURL = ApplicationURL;

        if (String.IsNullOrWhiteSpace(applicationURL)) {
            output.ActionResponse.Messages.Add("Unable to Determine Referring Website Address");
        }

        if (String.IsNullOrWhiteSpace(user.Email)) {
            output.ActionResponse.Messages.Add("Missing Required Email Address");
        } else if (!user.Email.IsEmailAddress()) {
            output.ActionResponse.Messages.Add("Invalid Email Address");
        }

        if (String.IsNullOrWhiteSpace(user.Password)) {
            output.ActionResponse.Messages.Add("Missing Required Password");
        }

        if (output.ActionResponse.Messages.Count() == 0) {
            // Make sure this username is a valid existing user
            var existing = await GetUser(user.TenantId, StringValue(user.Email));
            if (existing == null || !existing.ActionResponse.Result) {
                output.ActionResponse.Messages.Add("The email address '" + user.Email + "' is not a valid local account.");
                return output;
            }

            // Make sure the user account is enabled
            if (!existing.Enabled) {
                output.ActionResponse.Messages.Add("Your account has been disabled by an admin.");
                return output;
            }

            // Make sure this user doesn't have the flag set to prevent changing password
            if (existing.PreventPasswordChange) {
                output.ActionResponse.Messages.Add("Your account has been restricted by an admin to prevent password changes.");
                return output;
            }

            string websiteName = WebsiteName(applicationURL);
            if (String.IsNullOrWhiteSpace(websiteName)) {
                websiteName += applicationURL;
            }

            string code = GenerateRandomCode(6);

            string body = "<p>You are receiving this email because you used the Forgot Password option at <strong>" + websiteName + "</strong>.</p>" +
                    "<p>Use the following confirmation code on that page to confirm your new password:</p>" +
                    "<p style='font-size:2em;'>" + code + "</p>";

            List<string> to = new List<string>();
            to.Add(StringValue(user.Email));

            var settings = GetTenantSettings(user.TenantId);

            string from = String.Empty;
            if (settings != null) {
                from += settings.DefaultReplyToAddress;
            }

            var sent = SendEmail(new DataObjects.EmailMessage {
                From = from,
                To = to,
                Subject = "Forgot Password at " + websiteName,
                Body = body
            });

            if (sent.Result) {
                output = new DataObjects.User {
                    ActionResponse = GetNewActionResponse(true),
                    UserId = existing.UserId,
                    TenantId = existing.TenantId,
                    FirstName = existing.FirstName,
                    LastName = existing.LastName,
                    Email = existing.Email,
                    Username = StringValue(existing.Email),
                    Password = user.Password,
                    AuthToken = CompressByteArrayString(Encrypt(code))
                };
            } else {
                output.ActionResponse.Messages.Add("There was an error sending an email to the address you specified.");
            }
        }

        return output;
    }

    public async Task<DataObjects.User> ForgotPasswordConfirm(DataObjects.User user)
    {
        DataObjects.User output = new DataObjects.User();

        if (String.IsNullOrWhiteSpace(user.Email)) {
            output.ActionResponse.Messages.Add("Missing Required Email Address");
        }

        if (String.IsNullOrWhiteSpace(user.Password)) {
            output.ActionResponse.Messages.Add("Missing Required Password");
        }

        if (output.ActionResponse.Messages.Count() == 0) {
            string extended = CompressedByteArrayStringToFullString(user.AuthToken);
            string decrypted = Decrypt(extended);

            if (user.Location == decrypted) {
                // Update the user's password
                var currentUser = await GetUser(user.TenantId, StringValue(user.Email));
                if (currentUser != null && currentUser.ActionResponse.Result) {
                    currentUser.Password = Encrypt(user.Password);

                    var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == currentUser.UserId);
                    if (rec != null) {
                        rec.Password = currentUser.Password;
                        await data.SaveChangesAsync();

                        output = currentUser;
                    } else {
                        output.ActionResponse.Messages.Add("Unable to confirm and update password.");
                    }
                }
            } else {
                output.ActionResponse.Messages.Add("Invalid Confirmation Code");
            }
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

        EFModels.EFModels.User? rec = null;

        switch (Type) {
            case DataObjects.UserLookupType.Email:
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Email != null && x.Email.ToLower() == Lookup.ToLower());
                break;

            case DataObjects.UserLookupType.EmployeeId:
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.EmployeeId != null && x.EmployeeId.ToLower() == Lookup.ToLower());
                break;

            case DataObjects.UserLookupType.Guid:
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.UserId.ToString() == Lookup);
                break;

            case DataObjects.UserLookupType.Username:
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username != null && x.Username.ToLower() == Lookup.ToLower());
                break;
        }

        if (rec != null) {
            output = await GetUser(rec.UserId);
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

        if (!String.IsNullOrWhiteSpace(UserName)) {
            User? rec = null;

            if (UserName.Contains("@")) {
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Email != null && x.Email.ToLower() == UserName.ToLower());
            } else {
                rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username.ToLower() == UserName.ToLower());
            }

            if (rec != null) {
                output = await GetUser(rec.UserId);
            }

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

        var rec = await data.Users
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.UserId == UserId);
        if (rec != null) {
            output = new DataObjects.User {
                ActionResponse = GetNewActionResponse(true),
                TenantId = GuidValue(rec.TenantId),
                Admin = BooleanValue(rec.Admin),
                AppAdmin = output.AppAdmin,
                DepartmentId = GuidValue(rec.DepartmentId),
                DepartmentName = rec.DepartmentId.HasValue && rec.Department != null
                    ? rec.Department.DepartmentName
                    : String.Empty,
                Email = rec.Email,
                Phone = rec.Phone,
                Photo = await GetUserPhoto(UserId),
                Enabled = BooleanValue(rec.Enabled),
                FirstName = rec.FirstName,
                LastLogin = rec.LastLogin.HasValue ? Convert.ToDateTime(rec.LastLogin) : (DateTime?)null,
                LastName = rec.LastName,
                Location = rec.Location,
                Title = rec.Title,
                UserId = rec.UserId,
                Username = rec.Username,
                EmployeeId = rec.EmployeeId,
                Password = String.Empty,
                PreventPasswordChange = BooleanValue(rec.PreventPasswordChange),
                HasLocalPassword = !String.IsNullOrWhiteSpace(rec.Password),
                LastLockoutDate = rec.LastLockoutDate,
                Source = rec.Source,
                udf01 = rec.UDF01,
                udf02 = rec.UDF02,
                udf03 = rec.UDF03,
                udf04 = rec.UDF04,
                udf05 = rec.UDF05,
                udf06 = rec.UDF06,
                udf07 = rec.UDF07,
                udf08 = rec.UDF08,
                udf09 = rec.UDF09,
                udf10 = rec.UDF10
            };

            if (output.AppAdmin) {
                output.Admin = true;
            }

            output.Tenants = await GetUserTenants(output.Username, output.Email);
            output.UserTenants = await GetUserTenantList(output.Username, output.Email);

            output.DisplayName = DisplayNameFromLastAndFirst(output.LastName, output.FirstName, output.Email, output.DepartmentName, output.Location);

            if(_inMemoryDatabase && output.DepartmentId.HasValue && String.IsNullOrEmpty(output.DepartmentName)) {
                output.DepartmentName = GetDepartmentName(output.TenantId, GuidValue(output.DepartmentId));
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

        var language = GetTenantLanguage(output.TenantId, StringValue(output.CultureCode));

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
                Label = GetLanguageItem("FirstName", language),
                TipText = "",
                Sortable = true,
                DataElementName = "firstName",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = GetLanguageItem("LastName", language),
                TipText = "",
                Sortable = true,
                DataElementName = "lastName",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = GetLanguageItem("Email", language),
                TipText = "",
                Sortable = true,
                DataElementName = "email",
                DataType = "string"
            },
            new DataObjects.FilterColumn{
                Align = "",
                Label = GetLanguageItem("Username", language),
                TipText = "",
                Sortable = true,
                DataElementName = "username",
                DataType = "string"
            }
        };

        var settings = GetTenantSettings(output.TenantId);
        List<string> blockedModules = settings.ModuleHideElements;
        bool hideDepartments = false;
        bool hideEmployeeId = false;
        bool hideUDF = false;
        if (blockedModules.Any()) {
            hideDepartments = blockedModules.Contains("departments");
            hideEmployeeId = blockedModules.Contains("employeeid");
            hideUDF = blockedModules.Contains("udf");
        }

        if (!hideEmployeeId) {
            output.Columns.Add(new DataObjects.FilterColumn {
                Align = "",
                Label = GetLanguageItem("EmployeeId", language),
                TipText = "",
                Sortable = true,
                DataElementName = "employeeId",
                DataType = "string"
            });
        }

        if (!hideDepartments) {
            output.Columns.Add(new DataObjects.FilterColumn {
                Align = "",
                Label = GetLanguageItem("Department", language),
                TipText = "",
                Sortable = true,
                DataElementName = "departmentName",
                DataType = "string"
            });
        }

        output.Columns.Add(new DataObjects.FilterColumn {
            Align = "center",
            Label = "icon:RecordsTableIconEnabled",
            TipText = "Enabled",
            Sortable = true,
            DataElementName = "enabled",
            DataType = "boolean"
        });

        output.Columns.Add(new DataObjects.FilterColumn {
            Align = "center",
            Label = "icon:RecordsTableIconAdmin",
            TipText = "Admin",
            Sortable = true,
            DataElementName = "admin",
            DataType = "boolean"
        });

        output.Columns.Add(new DataObjects.FilterColumn {
            Align = "",
            Label = GetLanguageItem("LastLogin", language),
            TipText = "",
            Sortable = true,
            DataElementName = "lastLogin",
            DataType = "datetime"
        });

        // See if any UDF labels need to be included in the column output
        var udfLabels = await GetUDFLabels(output.TenantId, false);
        if (!hideUDF) {
            for (int x = 1; x < 11; x++) {
                bool show = ShowUDFColumn("Users", x, udfLabels);
                if (show) {
                    string label = UDFLabel("Users", x, udfLabels);
                    string udf = "udf" + x.ToString().PadLeft(2, '0');
                    if (String.IsNullOrEmpty(label)) {
                        label = udf.ToUpper();
                    }
                    output.Columns.Add(new DataObjects.FilterColumn {
                        Align = "",
                        Label = label,
                        TipText = "",
                        Sortable = true,
                        DataElementName = udf,
                        DataType = "string"
                    });
                }
            }
        }

        var recs = data.Users
            .Include(x => x.Department)
            .Where(x => x.TenantId == output.TenantId && x.Username != null && x.Username.ToLower() != "admin");

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

        if (!String.IsNullOrWhiteSpace(output.udf01)) { recs = recs.Where(x => x.UDF01 != null && x.UDF01.Contains(output.udf01)); }
        if (!String.IsNullOrWhiteSpace(output.udf02)) { recs = recs.Where(x => x.UDF02 != null && x.UDF02.Contains(output.udf02)); }
        if (!String.IsNullOrWhiteSpace(output.udf03)) { recs = recs.Where(x => x.UDF03 != null && x.UDF03.Contains(output.udf03)); }
        if (!String.IsNullOrWhiteSpace(output.udf04)) { recs = recs.Where(x => x.UDF04 != null && x.UDF04.Contains(output.udf04)); }
        if (!String.IsNullOrWhiteSpace(output.udf05)) { recs = recs.Where(x => x.UDF05 != null && x.UDF05.Contains(output.udf05)); }
        if (!String.IsNullOrWhiteSpace(output.udf06)) { recs = recs.Where(x => x.UDF06 != null && x.UDF06.Contains(output.udf06)); }
        if (!String.IsNullOrWhiteSpace(output.udf07)) { recs = recs.Where(x => x.UDF07 != null && x.UDF07.Contains(output.udf07)); }
        if (!String.IsNullOrWhiteSpace(output.udf08)) { recs = recs.Where(x => x.UDF08 != null && x.UDF08.Contains(output.udf08)); }
        if (!String.IsNullOrWhiteSpace(output.udf09)) { recs = recs.Where(x => x.UDF09 != null && x.UDF09.Contains(output.udf09)); }
        if (!String.IsNullOrWhiteSpace(output.udf10)) { recs = recs.Where(x => x.UDF10 != null && x.UDF10.Contains(output.udf10)); }

        // Add any filters
        if (!String.IsNullOrEmpty(output.Keyword)) {
            string keyword = output.Keyword.ToLower();
            // Dynamically include only the UDF fields that are needed
            bool includeUdf01 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF01", udfLabels);
            bool includeUdf02 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF02", udfLabels);
            bool includeUdf03 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF03", udfLabels);
            bool includeUdf04 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF04", udfLabels);
            bool includeUdf05 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF05", udfLabels);
            bool includeUdf06 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF06", udfLabels);
            bool includeUdf07 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF07", udfLabels);
            bool includeUdf08 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF08", udfLabels);
            bool includeUdf09 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF09", udfLabels);
            bool includeUdf10 = !hideUDF && UDFLabelIncludedInSearch("Users", "UDF10", udfLabels);

            if (includeUdf01 || includeUdf02 || includeUdf03 || includeUdf04 || includeUdf05 || includeUdf06 || includeUdf07 || includeUdf08 || includeUdf09 || includeUdf10) {
                recs = recs.Where(x => (x.LastName != null && x.LastName.ToLower().Contains(keyword))
                    || (x.FirstName != null && x.FirstName.ToLower().Contains(keyword))
                    || (x.Email != null && x.Email.ToLower().Contains(keyword))
                    || (x.Username != null && x.Username.ToLower().Contains(keyword))
                    || (includeUdf01 ? x.UDF01 != null && x.UDF01.ToLower().Contains(keyword) : false)
                    || (includeUdf02 ? x.UDF02 != null && x.UDF02.ToLower().Contains(keyword) : false)
                    || (includeUdf03 ? x.UDF03 != null && x.UDF03.ToLower().Contains(keyword) : false)
                    || (includeUdf04 ? x.UDF04 != null && x.UDF04.ToLower().Contains(keyword) : false)
                    || (includeUdf05 ? x.UDF05 != null && x.UDF05.ToLower().Contains(keyword) : false)
                    || (includeUdf06 ? x.UDF06 != null && x.UDF06.ToLower().Contains(keyword) : false)
                    || (includeUdf07 ? x.UDF07 != null && x.UDF07.ToLower().Contains(keyword) : false)
                    || (includeUdf08 ? x.UDF08 != null && x.UDF08.ToLower().Contains(keyword) : false)
                    || (includeUdf09 ? x.UDF09 != null && x.UDF09.ToLower().Contains(keyword) : false)
                    || (includeUdf10 ? x.UDF10 != null && x.UDF10.ToLower().Contains(keyword) : false)
                );
            } else {
                recs = recs.Where(x => (x.LastName != null && x.LastName.ToLower().Contains(keyword))
                    || (x.FirstName != null && x.FirstName.ToLower().Contains(keyword))
                    || (x.Email != null && x.Email.ToLower().Contains(keyword))
                    || (x.Username != null && x.Username.ToLower().Contains(keyword))
                );
            }
        }

        if (String.IsNullOrWhiteSpace(output.Sort)) {
            output.Sort = "lastLogin";
            output.SortOrder = "DESC";
        }

        if (String.IsNullOrWhiteSpace(output.SortOrder)) {
            switch (output.Sort.ToUpper()) {
                case "LASTLOGIN":
                    output.SortOrder = "DESC";
                    break;

                default:
                    output.SortOrder = "ASC";
                    break;
            }
        }

        bool Ascending = true;
        if (StringValue(output.SortOrder).ToUpper() == "DESC") {
            Ascending = false;
        }

        switch (StringValue(output.Sort).ToUpper()) {
            case "FIRSTNAME":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.FirstName).ThenBy(x => x.LastName);
                } else {
                    recs = recs.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName);
                }
                break;

            case "LASTNAME":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
                }
                break;

            case "EMAIL":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.Email);
                } else {
                    recs = recs.OrderByDescending(x => x.Email);
                }
                break;

            case "USERNAME":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.Username);
                } else {
                    recs = recs.OrderByDescending(x => x.Username);
                }
                break;

            case "EMPLOYEEID":
                if (Ascending) {
                    recs = recs.OrderBy(x => x.EmployeeId).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderByDescending(x => x.EmployeeId).ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
                }
                break;

            case "DEPARTMENTNAME":
                if (Ascending) {
                    recs = recs.OrderBy(x => (x.Department != null ? x.Department.DepartmentName : String.Empty))
                        .ThenBy(x => x.LastName).ThenBy(x => x.FirstName);
                } else {
                    recs = recs.OrderByDescending(x => (x.Department != null ? x.Department.DepartmentName : String.Empty))
                        .ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
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

            case "UDF01":
                recs = Ascending ? recs.OrderBy(x => x.UDF01) : recs.OrderByDescending(x => x.UDF01);
                break;

            case "UDF02":
                recs = Ascending ? recs.OrderBy(x => x.UDF02) : recs.OrderByDescending(x => x.UDF02);
                break;

            case "UDF03":
                recs = Ascending ? recs.OrderBy(x => x.UDF03) : recs.OrderByDescending(x => x.UDF03);
                break;

            case "UDF04":
                recs = Ascending ? recs.OrderBy(x => x.UDF04) : recs.OrderByDescending(x => x.UDF04);
                break;

            case "UDF05":
                recs = Ascending ? recs.OrderBy(x => x.UDF05) : recs.OrderByDescending(x => x.UDF05);
                break;

            case "UDF06":
                recs = Ascending ? recs.OrderBy(x => x.UDF06) : recs.OrderByDescending(x => x.UDF06);
                break;

            case "UDF07":
                recs = Ascending ? recs.OrderBy(x => x.UDF07) : recs.OrderByDescending(x => x.UDF07);
                break;

            case "UDF08":
                recs = Ascending ? recs.OrderBy(x => x.UDF08) : recs.OrderByDescending(x => x.UDF08);
                break;

            case "UDF09":
                recs = Ascending ? recs.OrderBy(x => x.UDF09) : recs.OrderByDescending(x => x.UDF09);
                break;

            case "UDF10":
                recs = Ascending ? recs.OrderBy(x => x.UDF10) : recs.OrderByDescending(x => x.UDF10);
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
                    PreventPasswordChange = rec.PreventPasswordChange.HasValue ? (bool)rec.PreventPasswordChange : false,
                    HasLocalPassword = !String.IsNullOrWhiteSpace(rec.Password),
                    LastLockoutDate = rec.LastLockoutDate,
                    Source = rec.Source,
                    udf01 = rec.UDF01,
                    udf02 = rec.UDF02,
                    udf03 = rec.UDF03,
                    udf04 = rec.UDF04,
                    udf05 = rec.UDF05,
                    udf06 = rec.UDF06,
                    udf07 = rec.UDF07,
                    udf08 = rec.UDF08,
                    udf09 = rec.UDF09,
                    udf10 = rec.UDF10
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
                    PreventPasswordChange = rec.PreventPasswordChange.HasValue ? (bool)rec.PreventPasswordChange : false,
                    HasLocalPassword = !String.IsNullOrWhiteSpace(rec.Password),
                    LastLockoutDate = rec.LastLockoutDate,
                    Source = rec.Source,
                    udf01 = rec.UDF01,
                    udf02 = rec.UDF02,
                    udf03 = rec.UDF03,
                    udf04 = rec.UDF04,
                    udf05 = rec.UDF05,
                    udf06 = rec.UDF06,
                    udf07 = rec.UDF07,
                    udf08 = rec.UDF08,
                    udf09 = rec.UDF09,
                    udf10 = rec.UDF10
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
            var u1 = await data.Users.Where(x => x.Username == username || (x.Email != null && x.Email.ToLower() == email.ToLower())).ToListAsync();
            if (u1 != null && u1.Any()) {
                foreach (var rec in u1) {
                    bool enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false;
                    if (enabled || !enabledUsersOnly) {
                        output.Add(new DataObjects.UserTenant { UserId = rec.UserId, TenantId = GuidValue(rec.TenantId) });
                    }
                }
            }
        } else if (!String.IsNullOrEmpty(username)) {
            var u2 = await data.Users.Where(x => x.Username != null && x.Username.ToLower() == username.ToLower()).ToListAsync();
            if (u2 != null && u2.Any()) {
                foreach (var rec in u2) {
                    bool enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false;
                    if (enabled || !enabledUsersOnly) {
                        output.Add(new DataObjects.UserTenant { UserId = rec.UserId, TenantId = GuidValue(rec.TenantId) });
                    }
                }
            }
        } else if (!String.IsNullOrEmpty(email)) {
            var u3 = await data.Users.Where(x => x.Email != null && x.Email.ToLower() == email.ToLower()).ToListAsync();
            if (u3 != null && u3.Any()) {
                foreach (var rec in u3) {
                    bool enabled = rec.Enabled.HasValue ? (bool)rec.Enabled : false;
                    if (enabled || !enabledUsersOnly) {
                        output.Add(new DataObjects.UserTenant { UserId = rec.UserId, TenantId = GuidValue(rec.TenantId) });
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
            var u1 = await data.Users.Where(x => (x.Username != null && x.Username.ToLower() == username.ToLower()) ||
                (x.Email != null && x.Email.ToLower() == email.ToLower())).ToListAsync();
            if (u1 != null && u1.Any()) {
                if (enabledUsersOnly) {
                    tenantIds = u1.Where(x => x.Enabled == true).Select(x => x.TenantId).Distinct().ToList();
                } else {
                    tenantIds = u1.Select(x => x.TenantId).Distinct().ToList();
                }
            }
        } else if (!String.IsNullOrEmpty(username)) {
            var u2 = await data.Users.Where(x => x.Username != null && x.Username.ToLower() == username.ToLower()).ToListAsync();
            if (u2 != null && u2.Any()) {
                if (enabledUsersOnly) {
                    tenantIds = u2.Where(x => x.Enabled == true).Select(x => x.TenantId).Distinct().ToList();
                } else {
                    tenantIds = u2.Select(x => x.TenantId).Distinct().ToList();
                }
            }
        } else if (!String.IsNullOrEmpty(email)) {
            var u3 = await data.Users.Where(x => x.Email != null && x.Email.ToLower() == email.ToLower()).ToListAsync();
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
                if (Decrypt(StringValue(rec.Password)) != reset.CurrentPassword) {
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

        user.Email = MaxStringLength(user.Email, 100);
        rec.Email = user.Email;

        user.EmployeeId = MaxStringLength(user.EmployeeId, 50);
        rec.EmployeeId = user.EmployeeId;

        user.FirstName = MaxStringLength(user.FirstName, 100);
        rec.FirstName = user.FirstName;

        rec.LastLogin = user.LastLogin.HasValue ? (DateTime)user.LastLogin : (DateTime?)null;

        user.LastName = MaxStringLength(user.LastName, 100);
        rec.LastName = user.LastName;

        user.Location = MaxStringLength(user.Location, 255);
        rec.Location = user.Location;

        user.Phone = MaxStringLength(user.Phone, 20);
        rec.Phone = user.Phone;

        user.Title = MaxStringLength(user.Title, 255);
        rec.Title = user.Title;

        user.Source = MaxStringLength(user.Source, 100);
        rec.Source = user.Source;

        user.udf01 = MaxStringLength(user.udf01, 500);
        user.udf02 = MaxStringLength(user.udf02, 500);
        user.udf03 = MaxStringLength(user.udf03, 500);
        user.udf04 = MaxStringLength(user.udf04, 500);
        user.udf05 = MaxStringLength(user.udf05, 500);
        user.udf06 = MaxStringLength(user.udf06, 500);
        user.udf07 = MaxStringLength(user.udf07, 500);
        user.udf08 = MaxStringLength(user.udf08, 500);
        user.udf09 = MaxStringLength(user.udf09, 500);
        user.udf10 = MaxStringLength(user.udf10, 500);
        rec.UDF01 = user.udf01;
        rec.UDF02 = user.udf02;
        rec.UDF03 = user.udf03;
        rec.UDF04 = user.udf04;
        rec.UDF05 = user.udf05;
        rec.UDF06 = user.udf06;
        rec.UDF07 = user.udf07;
        rec.UDF08 = user.udf08;
        rec.UDF09 = user.udf09;
        rec.UDF10 = user.udf10;

        user.Username = MaxStringLength(user.Username, 100);
        rec.Username = user.Username;

        rec.DepartmentId = user.DepartmentId.HasValue && user.DepartmentId != Guid.Empty ? (Guid)user.DepartmentId : null;
        if (!user.DepartmentId.HasValue && !String.IsNullOrEmpty(user.DepartmentName)) {
            var deptId = await DepartmentIdFromNameAndLocation(user.TenantId, user.DepartmentName);
            if (deptId != Guid.Empty) {
                rec.DepartmentId = deptId;
            }
        }

        rec.Enabled = user.Enabled;
        rec.Admin = user.Admin;
        rec.PreventPasswordChange = user.PreventPasswordChange;

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

        if (GuidValue(user.TenantId) == Guid.Empty) {
            user.ActionResponse.Messages.Add("Invalid TenantId");
            return user;
        }

        bool newRecord = false;
        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == user.TenantId && x.Username != null && x.Username.ToLower() == user.Username.ToLower());
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

        user.Email = MaxStringLength(user.Email, 100);
        rec.Email = user.Email;

        user.EmployeeId = MaxStringLength(user.EmployeeId, 50);
        rec.EmployeeId = user.EmployeeId;

        user.FirstName = MaxStringLength(user.FirstName, 100);
        rec.FirstName = user.FirstName;

        rec.LastLogin = user.LastLogin.HasValue ? (DateTime)user.LastLogin : (DateTime?)null;

        user.LastName = MaxStringLength(user.LastName, 100);
        rec.LastName = user.LastName;

        user.Location = MaxStringLength(user.Location, 255);
        rec.Location = user.Location;

        user.Phone = MaxStringLength(user.Phone, 20);
        rec.Phone = user.Phone;

        user.Title = MaxStringLength(user.Title, 255);
        rec.Title = user.Title;

        user.Source = MaxStringLength(user.Source, 100);
        rec.Source = user.Source;

        user.udf01 = MaxStringLength(user.udf01, 500);
        user.udf02 = MaxStringLength(user.udf02, 500);
        user.udf03 = MaxStringLength(user.udf03, 500);
        user.udf04 = MaxStringLength(user.udf04, 500);
        user.udf05 = MaxStringLength(user.udf05, 500);
        user.udf06 = MaxStringLength(user.udf06, 500);
        user.udf07 = MaxStringLength(user.udf07, 500);
        user.udf08 = MaxStringLength(user.udf08, 500);
        user.udf09 = MaxStringLength(user.udf09, 500);
        user.udf10 = MaxStringLength(user.udf10, 500);
        rec.UDF01 = user.udf01;
        rec.UDF02 = user.udf02;
        rec.UDF03 = user.udf03;
        rec.UDF04 = user.udf04;
        rec.UDF05 = user.udf05;
        rec.UDF06 = user.udf06;
        rec.UDF07 = user.udf07;
        rec.UDF08 = user.udf08;
        rec.UDF09 = user.udf09;
        rec.UDF10 = user.udf10;

        user.Username = MaxStringLength(user.Username, 100);
        rec.Username = user.Username;

        rec.DepartmentId = user.DepartmentId.HasValue && user.DepartmentId != Guid.Empty ? (Guid)user.DepartmentId : null;

        if (!user.DepartmentId.HasValue && !String.IsNullOrEmpty(user.DepartmentName)) {
            var deptId = await DepartmentIdFromNameAndLocation(user.TenantId, user.DepartmentName);
            if (deptId != Guid.Empty) {
                rec.DepartmentId = deptId;
            }
        }

        rec.Enabled = user.Enabled;
        rec.Admin = user.Admin;
        rec.PreventPasswordChange = user.PreventPasswordChange;

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

    public async Task<DataObjects.User> UnlockUserAccount(Guid UserId)
    {
        DataObjects.User output = await GetUser(UserId);

        if (output.ActionResponse.Result) {
            if (output.LastLockoutDate.HasValue) {
                var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == UserId);
                if (rec != null) {
                    rec.LastLockoutDate = null;
                    await data.SaveChangesAsync();
                    output = await GetUser(UserId);
                } else {
                    output.ActionResponse = GetNewActionResponse(false, "UserId not found.");
                }
            } else {
                output.ActionResponse = GetNewActionResponse(false, "Account Has Already Been Unlocked");
            }
        }

        return output;
    }

    public async Task<DataObjects.User?> UpdateUserFromExternalDataSources(DataObjects.User User, DataObjects.TenantSettings? settings = null)
    {
        DataObjects.User? output = null;

        if (settings == null) {
            settings = GetTenantSettings(User.TenantId);
        }

        if (settings != null && settings.ExternalUserDataSources != null && settings.ExternalUserDataSources.Any()) {
            bool updated = false;
            bool updatedDepartmentOrLocation = false;
            var updatedUser = CopyUser(User);

            foreach (var source in settings.ExternalUserDataSources.Where(x => x.Active == true).OrderBy(x => x.SortOrder).ThenBy(x => x.Name)) {
                if (!String.IsNullOrWhiteSpace(source.Type)) {
                    switch (source.Type.ToUpper()) {
                        case "SQL":
                            if (!String.IsNullOrWhiteSpace(source.ConnectionString) && !String.IsNullOrWhiteSpace(source.Source)) {
                                try {
                                    string connectionString = source.ConnectionString;

                                    string query = source.Source
                                        .Replace("{{employeeid}}", User.EmployeeId)
                                        .Replace("{{username}}", User.Username)
                                        .Replace("{{email}}", User.Email);

                                    using (Sql2LINQ s = new Sql2LINQ(connectionString)) {
                                        var records = s.RunQuery<DataObjects.User>(query);
                                        if (records != null && records.Count() == 1) {
                                            var userRecord = records.FirstOrDefault();
                                            if (userRecord != null) {
                                                if (!String.IsNullOrWhiteSpace(userRecord.FirstName) && updatedUser.FirstName != userRecord.FirstName) {
                                                    updatedUser.FirstName = userRecord.FirstName;
                                                    updated = true;
                                                }
                                                if (!String.IsNullOrWhiteSpace(userRecord.LastName) && updatedUser.LastName != userRecord.LastName) {
                                                    updatedUser.LastName = userRecord.LastName;
                                                    updated = true;
                                                }
                                                if (!String.IsNullOrWhiteSpace(userRecord.DepartmentName) && updatedUser.DepartmentName != userRecord.DepartmentName) {
                                                    updatedUser.DepartmentName = userRecord.DepartmentName;
                                                    updated = true;
                                                    updatedDepartmentOrLocation = true;
                                                }
                                                if (!String.IsNullOrWhiteSpace(userRecord.Location) && updatedUser.Location != userRecord.Location) {
                                                    updatedUser.Location = userRecord.Location;
                                                    updated = true;
                                                    updatedDepartmentOrLocation = true;
                                                }
                                            }
                                        }
                                    }
                                } catch { }
                            }

                            break;

                        case "CSHARP":
                            // By convention the code must have a namespace of CustomCode, a public class of CustomDynamicCode,
                            // and a function named FindUser.
                            if (!String.IsNullOrWhiteSpace(source.Source)) {
                                string employeeId = StringValue(User.EmployeeId);
                                string username = StringValue(User.Username);
                                string email = StringValue(User.Email);

                                var findUserResult = ExecuteDynamicCSharpCode<DataObjects.User>(source.Source, new object[] { employeeId, username, email, this }, null, "CustomCode", "CustomDynamicCode", "FindUser");
                                if (findUserResult != null) {
                                    if (!String.IsNullOrWhiteSpace(findUserResult.FirstName) && updatedUser.FirstName != findUserResult.FirstName) {
                                        updatedUser.FirstName = findUserResult.FirstName;
                                        updated = true;
                                    }
                                    if (!String.IsNullOrWhiteSpace(findUserResult.LastName) && updatedUser.LastName != findUserResult.LastName) {
                                        updatedUser.LastName = findUserResult.LastName;
                                        updated = true;
                                    }
                                    if (!String.IsNullOrWhiteSpace(findUserResult.DepartmentName) && updatedUser.DepartmentName != findUserResult.DepartmentName) {
                                        updatedUser.DepartmentName = findUserResult.DepartmentName;
                                        updated = true;
                                        updatedDepartmentOrLocation = true;
                                    }
                                    if (!String.IsNullOrWhiteSpace(findUserResult.Location) && updatedUser.Location != findUserResult.Location) {
                                        updatedUser.Location = findUserResult.Location;
                                        updated = true;
                                        updatedDepartmentOrLocation = true;
                                    }
                                }
                            }

                            break;
                    }
                }
            }

            if (updated) {
                if (updatedDepartmentOrLocation) {
                    var departmentId = await DepartmentIdFromNameAndLocation(updatedUser.TenantId, updatedUser.DepartmentName, updatedUser.Location);

                    if (departmentId != Guid.Empty && departmentId != updatedUser.DepartmentId) {
                        updatedUser.DepartmentId = departmentId;
                    }
                }

                output = updatedUser;
            }
        }

        return output;
    }

    public DataObjects.User? UpdateUserFromSsoSettings(DataObjects.User User, SSO.Auth.SingleSignOn ssoInfo)
    {
        DataObjects.User? output = null;
        bool updated = false;

        DataObjects.User updatedUser = CopyUser(User);

        // Update only certain properties
        if (!String.IsNullOrWhiteSpace(User.DepartmentName)) {
            updatedUser.DepartmentName = User.DepartmentName;
            updated = true;
        }
        if (!String.IsNullOrWhiteSpace(User.FirstName)) {
            updatedUser.FirstName = User.FirstName;
            updated = true;
        }
        if (!String.IsNullOrWhiteSpace(User.LastName)) {
            updatedUser.LastName = User.LastName;
            updated = true;
        }
        if (!String.IsNullOrWhiteSpace(User.Email)) {
            updatedUser.Email = User.Email;
            updated = true;
        }
        if (!String.IsNullOrWhiteSpace(User.Username)) {
            updatedUser.Username = User.Username;
            updated = true;
        }
        if (!String.IsNullOrWhiteSpace(User.EmployeeId)) {
            updatedUser.EmployeeId = User.EmployeeId;
            updated = true;
        }

        if (updated) {
            output = updatedUser;
        }

        return output;
    }

    public async Task UpdateUserLastLoginTime(Guid UserId)
    {
        var rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == UserId);
        if (rec != null) {
            rec.LastLogin = DateTime.UtcNow;
            await data.SaveChangesAsync();
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
                ((x.Username != null && x.Username != "" && x.Username.ToLower() == rec.Username.ToLower())
                ||
                (x.Email != null && x.Email != "" && rec.Email != null && x.Email.ToLower() == rec.Email.ToLower()))
            );
            output = adminUser != null;
        }

        return output;
    }

    public async Task<DataObjects.User> UserSignup(DataObjects.User user)
    {
        DataObjects.User output = user;
        output.ActionResponse = GetNewActionResponse();

        // First, validate the given TenantId
        var tenant = GetTenant(user.TenantId);

        if (tenant == null || !tenant.ActionResponse.Result) {
            output.ActionResponse.Messages.Add("Invalid Customer Code");
            return output;
        }

        var appUrl = ApplicationURL;
        string websiteName = WebsiteName(appUrl);
        if (String.IsNullOrWhiteSpace(websiteName)) {
            websiteName = StringValue(appUrl);
        }

        if (String.IsNullOrWhiteSpace(appUrl)) {
            output.ActionResponse.Messages.Add("Application URL Not Configured");
            return output;
        }

        if (!appUrl.EndsWith("/")) {
            appUrl += "/";
        }
        appUrl += tenant.TenantCode + "/";

        // Next, make sure this email address does not already exist for this customer
        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == user.TenantId && x.Email == user.Email);
        if (rec != null) {
            output.ActionResponse.Messages.Add("An account already exists for the email address you entered. Please use the Forgot Password option to reset your password.");
            return output;
        }

        string code = GenerateRandomCode(6);

        string body = "<p>You are receiving this email because you signed up for an account at <strong>" + websiteName + "</strong>.</p>" +
                "<p>Use the following confirmation code on that page to confirm your new account:</p>" +
                "<p style='font-size:2em;'>" + code + "</p>";

        List<string> to = new List<string>();
        to.Add(StringValue(user.Email));

        var settings = GetTenantSettings(user.TenantId);

        string from = String.Empty;
        if (settings != null) {
            from += settings.DefaultReplyToAddress;
        }

        var sent = SendEmail(new DataObjects.EmailMessage {
            From = from,
            To = to,
            Subject = "Forgot Password at " + websiteName,
            Body = body
        });

        if (sent.Result) {
            output = new DataObjects.User {
                ActionResponse = GetNewActionResponse(true),
                UserId = user.UserId,
                TenantId = user.TenantId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = StringValue(user.Email),
                Password = user.Password,
                AuthToken = CompressByteArrayString(Encrypt(code))
            };
        } else {
            output.ActionResponse.Messages.Add("There was an error sending an email to the address you specified.");
        }

        return output;
    }

    public async Task<DataObjects.User> UserSignupConfirm(DataObjects.User user)
    {
        DataObjects.User output = new DataObjects.User();

        if (String.IsNullOrWhiteSpace(user.Email)) {
            output.ActionResponse.Messages.Add("Missing Required Email Address");
        }

        if (String.IsNullOrWhiteSpace(user.Password)) {
            output.ActionResponse.Messages.Add("Missing Required Password");
        }

        if (output.ActionResponse.Messages.Count() == 0) {
            string extended = CompressedByteArrayStringToFullString(user.AuthToken);
            string decrypted = Decrypt(extended);

            if (user.Location == decrypted) {
                // Create the account.
                var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == user.TenantId && x.Email == user.Email);
                if (rec != null) {
                    output.ActionResponse.Messages.Add("An account already exists for the email address you entered. Please use the Forgot Password option to reset your password.");
                    return output;
                }

                try {

                    await data.Users.AddAsync(new User {
                        UserId = Guid.NewGuid(),
                        TenantId = user.TenantId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Username = StringValue(user.Email),
                        Password = Encrypt(user.Password),
                        Enabled = true,
                        Source = "Local Login Signup Form"
                    });

                    await data.SaveChangesAsync();
                    output.ActionResponse.Result = true;
                } catch {
                    output.ActionResponse.Messages.Add("An error occurred attempting to create the new user account.");
                }
            } else {
                output.ActionResponse.Messages.Add("Invalid Confirmation Code");
            }
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
                    UserId = _guid1,
                    PreventPasswordChange = false
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
                            UserId = Guid.NewGuid(),
                            PreventPasswordChange = false
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
                            (x.Username != null && x.Username != "" && x.Username.ToLower() == user.Username.ToLower())
                            ||
                            (x.Email != null && x.Email != "" && x.Email.ToLower() == StringValue(user.Email).ToLower())
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
                            Password = user.Password,
                            PreventPasswordChange = false
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

        if (!output.Result && output.Messages.Count() == 0) {
            output.Messages.Add("Unable to Validate UserId '" + UserId.ToString() + "'");
        }
        return output;
    }
}