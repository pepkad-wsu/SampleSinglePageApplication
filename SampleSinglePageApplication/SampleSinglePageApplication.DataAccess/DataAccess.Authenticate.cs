namespace SampleSinglePageApplication;
public partial class DataAccess
{
    /// <summary>
    /// authenticates a user login
    /// </summary>
    /// <param name="Username">the username to authenticate</param>
    /// <param name="Password">the password to authenticate</param>
    /// <returns>true if the credentials are valid, otherwise returns false</returns>
    public async Task<DataObjects.User> Authenticate(Guid TenantId, string Username, string Password, bool RequirePassword)
    {
        DataObjects.User output = new DataObjects.User();
        DataAccess da = new DataAccess();

        bool FoundActiveDirectoryInfo = false;
        string? DepartmentName = String.Empty;
        output.ActionResponse.Result = false;

        Username = StringOrEmpty(Username).Trim();
        Password = StringOrEmpty(Password).Trim();

        //if(RequirePassword && Password == "testing") {
        //    RequirePassword = false;
        //}

        var tenant = GetTenant(TenantId);

        if (tenant == null || !tenant.ActionResponse.Result) {
            output.ActionResponse.Messages.Add("Invalid Tenant Account");
            return output;
        }

        // For development just return true if the user is found, ignore that password
        var ldapRoot = GetSetting<string>("activedirectoryroot", DataObjects.SettingType.Text);
        if (RequirePassword) {
            // First, see if this is a local login
            var user = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username == Username && x.Enabled == true);
            if (user != null) {
                // Check if this user is currently locked out.
                if (user.LastLockoutDate.HasValue) {
                    // See if it has been at least X minutes since the last lockout.
                    DateTime lastLockoutDate = (DateTime)user.LastLockoutDate;
                    DateTime accountUnlockDate = lastLockoutDate.AddMinutes(_accountLockoutMinutes);
                    if(accountUnlockDate > DateTime.Now) {
                        // Still locked out.
                        output.ActionResponse.Messages.Add("Account locked out for " + _accountLockoutMinutes.ToString() +
                            " minutes at " + lastLockoutDate.ToString() + " due to too many failed login attempts. " + Environment.NewLine + 
                            "Please try again after " + accountUnlockDate.ToString() + " when the account unlocks.");

                        return output;
                    }

                    // At this point we are no longer under a lockout, so clear any previous lockouts.
                    user.LastLockoutDate = null;
                    user.FailedLoginAttempts = null;
                    await data.SaveChangesAsync();
                }

                // valid user, so see if password matches
                if (user.Password == Password) {
                    // password matches, but needs to be encrypted
                    user.Password = Encrypt(Password);
                    await data.SaveChangesAsync();
                    output.ActionResponse.Result = true;
                } else {
                    // See if the decrypted password matches
                    if (Decrypt(StringOrEmpty(user.Password)) == Password) {
                        output.ActionResponse.Result = true;
                    }
                }
                if (output.ActionResponse.Result) {
                    // Valid login, so return the User Object
                    output = await da.GetUser(TenantId, Username);
                    output.AuthToken = GetUserToken(output.TenantId, output.UserId);

                    user.LastLockoutDate = null;
                    user.FailedLoginAttempts = null;
                    await data.SaveChangesAsync();

                    return output;
                }
            }


            // If local authentication failed authenticate against AD to make sure this is a valid login
            if (!String.IsNullOrWhiteSpace(ldapRoot) && Username.ToLower() != "admin") {
                string ldapLocationAttribute = GetLdapOptionalLocationAttribute();

                try {
                    System.DirectoryServices.SearchResult? result = null;
                    string domainAndUsername = ldapRoot + @"\" + Username;
#pragma warning disable CA1416 // Validate platform compatibility
                    DirectoryEntry entry = new DirectoryEntry("LDAP://" + ldapRoot, domainAndUsername, Password);
                    try {
                        object obj = entry.NativeObject;
                        DirectorySearcher search = new DirectorySearcher(entry);
                        search.Filter = "(SAMAccountName=" + Username + ")";
                        search.PropertiesToLoad.Add("givenName");
                        search.PropertiesToLoad.Add("sn");
                        search.PropertiesToLoad.Add("mail");
                        search.PropertiesToLoad.Add("department");
                        search.PropertiesToLoad.Add(ldapLocationAttribute);
                        result = search.FindOne();
                        if (result != null) {
                            // Valid login
                            FoundActiveDirectoryInfo = true;
                            output.ActionResponse.Result = true;
                            output.Email = result.Properties["mail"][0].ToString();
                            output.FirstName = result.Properties["givenName"][0].ToString();
                            output.LastName = result.Properties["sn"][0].ToString();
                            output.UserId = entry.Guid;
                            output.Username = Username.ToLower();
                            DepartmentName = result.Properties["department"][0].ToString();
                            output.Location = result.Properties[ldapLocationAttribute][0].ToString();
                        }
                    } catch {
                        output.ActionResponse.Messages.Add("Invalid Login");
                    }
#pragma warning restore CA1416 // Validate platform compatibility

                } catch { }
            }
        }

        // Only continue if we have a valid login
        if (output.ActionResponse.Result || !RequirePassword) {
            // See if we can find this user in Active Directory to get the full details
            var ldapOptionalLocationAttribute = GetLdapOptionalLocationAttribute();
            DataObjects.ActiveDirectoryUserInfo? adUserInfo = GetActiveDirectoryInfo(Username, StringOrEmpty(ldapRoot), Username, Password, ldapOptionalLocationAttribute);
            if (adUserInfo != null) {
                FoundActiveDirectoryInfo = true;
                output.Email = adUserInfo.Email;
                output.FirstName = adUserInfo.FirstName;
                output.LastName = adUserInfo.LastName;
                output.UserId = adUserInfo.UserId.HasValue ? (Guid)adUserInfo.UserId : Guid.Empty;
                output.Username = Username;
                output.Phone = adUserInfo.Phone;
                output.Location = adUserInfo.Location;
                output.EmployeeId = adUserInfo.EmployeeId;
                output.Title = adUserInfo.Title;
                DepartmentName = adUserInfo.Department;
            }
            Guid DepartmentId = await DepartmentIdFromActiveDirectoryName(TenantId, DepartmentName);


            var rec = await data.Users.FirstOrDefaultAsync(x => x.Username == Username && x.Enabled == true);
            if (rec == null) {
                // This account does not yet exist, so create it if this tenant allows via the RequirePreExistingAccountToLogIn setting.
                if (tenant.TenantSettings.RequirePreExistingAccountToLogIn) {
                    // No preexisting account, so login is not allowed for this tenant.
                } else {
                    rec = new EFModels.EFModels.User();
                    if (DepartmentId != Guid.Empty) { rec.DepartmentId = DepartmentId; }

                    if (output.UserId == Guid.Empty) {
                        output.UserId = Guid.NewGuid();
                    }

                    rec.Email = !String.IsNullOrWhiteSpace(output.Email) ? output.Email : String.Empty;
                    rec.Enabled = true;
                    rec.FirstName = !String.IsNullOrWhiteSpace(output.FirstName) ? output.FirstName : String.Empty;
                    rec.LastLogin = DateTime.Now;
                    rec.LastName = !String.IsNullOrWhiteSpace(output.LastName) ? output.LastName : String.Empty;
                    rec.UserId = output.UserId;
                    rec.Username = !String.IsNullOrWhiteSpace(output.Username) ? output.Username : Username;
                    rec.Phone = !String.IsNullOrWhiteSpace(output.Phone) ? output.Phone : String.Empty;
                    rec.Location = !String.IsNullOrWhiteSpace(output.Location) ? output.Location : String.Empty;
                    rec.Title = !String.IsNullOrWhiteSpace(output.Title) ? output.Title : String.Empty;
                    try {
                        data.Users.Add(rec);
                        await data.SaveChangesAsync();
                        output.ActionResponse.Result = true;
                    } catch (Exception ex) {
                        output.ActionResponse.Result = false;
                        output.ActionResponse.Messages.Add("Error Creating New User Account - " + ex.Message);
                    }
                }
            } else {
                // This account already exists, so make sure it is enabled and update any necessary data
                output.ActionResponse.Result = rec.Enabled.HasValue ? (bool)rec.Enabled : false;

                if (DepartmentId != Guid.Empty) { rec.DepartmentId = DepartmentId; }

                if (FoundActiveDirectoryInfo) {
                    // Update the account info with the results from Active Directory
                    rec.Email = output.Email;
                    rec.FirstName = output.FirstName;
                    rec.LastLogin = DateTime.Now;
                    rec.LastName = output.LastName;
                    if (rec.UserId != output.UserId) {
                        // Eventually, we would update this and all related data, but for now we will just change the output
                        output.UserId = rec.UserId;
                    }
                    rec.Username = output.Username;
                    rec.Phone = output.Phone;
                    rec.Location = output.Location;
                    rec.Title = output.Title;
                    try {
                        await data.SaveChangesAsync();
                    } catch (Exception ex) {
                        output.ActionResponse.Result = false;
                        output.ActionResponse.Messages.Add("Error Updating User Account - " + ex.Message);
                    }
                }
            }
        }

        if (!output.ActionResponse.Result) {
            output.ActionResponse.Messages.Add("Invalid Username or Password");
            await SetUserLockout(TenantId, Username);
        } else {
            output.AuthToken = GetUserToken(output.TenantId, output.UserId);
        }

        return output;
    }

    private async Task SetUserLockout(Guid TenantId, string Username)
    {
        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username == Username);
        if(rec != null) {
            int currentAttempts = rec.FailedLoginAttempts.HasValue ? (int)rec.FailedLoginAttempts : 0;
            currentAttempts += 1;

            if(currentAttempts >= _accountLockoutMaxAttempts) {
                // Mark the account as locked out.
                rec.LastLockoutDate = DateTime.Now;
            }
            rec.FailedLoginAttempts = currentAttempts;

            await data.SaveChangesAsync();
        }
    }
}