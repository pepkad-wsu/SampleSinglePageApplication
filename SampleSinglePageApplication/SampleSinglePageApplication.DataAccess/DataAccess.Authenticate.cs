namespace SampleSinglePageApplication;
public partial class DataAccess
{
    /// <summary>
    /// authenticates a user login
    /// </summary>
    /// <param name="Username">the username to authenticate</param>
    /// <param name="Password">the password to authenticate</param>
    /// <returns>true if the credentials are valid, otherwise returns false</returns>
    public async Task<DataObjects.User> Authenticate(string Username, string Password, Guid? TenantId)
    {
        DataObjects.User output = new DataObjects.User();
        DataObjects.Tenant? tenant = null;
        Guid tenantId = TenantId != null ? GuidValue(TenantId) : Guid.Empty;

        bool FoundActiveDirectoryInfo = false;
        string? DepartmentName = String.Empty;
        output.ActionResponse.Result = false;

        string username = StringValue(Username).Trim();
        string password = StringValue(Password).Trim();

        if (tenantId != Guid.Empty) {
            tenant = GetTenant(tenantId);
            if (tenant == null || !tenant.ActionResponse.Result || !tenant.Enabled) {
                output.ActionResponse.Messages.Add("Invalid Tenant Account");
                return output;
            }
        }

        Guid userId = Guid.Empty;

        // First, see if this is a local login
        User? user = null;
        if (tenantId != Guid.Empty) {
            user = await data.Users
                .Include(x => x.Tenant)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Tenant.Enabled && x.Username.ToLower() == username.ToLower() && x.Enabled == true);
        } else {
            // Find the first user record for this user where the account is not in the admin tenant.
            user = await data.Users
                .Include(x => x.Tenant)
                .FirstOrDefaultAsync(x => x.TenantId != _guid1 && x.Tenant.Enabled && x.Username.ToLower() == username.ToLower() && x.Enabled == true);
        }

        if (user != null) {
            tenantId = user.TenantId;
            userId = user.UserId;

            // Check if this user is currently locked out.
            if (user.LastLockoutDate.HasValue) {
                // See if it has been at least X minutes since the last lockout.
                DateTime lastLockoutDate = (DateTime)user.LastLockoutDate;
                DateTime accountUnlockDate = lastLockoutDate.AddMinutes(_accountLockoutMinutes);
                if (accountUnlockDate > DateTime.UtcNow) {
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
            if (user.Password == password) {
                // password matches, but needs to be encrypted
                user.Password = Encrypt(password);
                await data.SaveChangesAsync();
                output.ActionResponse.Result = true;
            } else {
                // See if the decrypted password matches
                if (Decrypt(StringValue(user.Password)) == password) {
                    output.ActionResponse.Result = true;
                }
            }

            if (output.ActionResponse.Result) {
                // Valid login, so return the User Object
                output = await GetUser(user.UserId);
                output.AuthToken = GetUserToken(output.TenantId, output.UserId);

                user.LastLockoutDate = null;
                user.FailedLoginAttempts = null;
                await data.SaveChangesAsync();

                return output;
            }
        }

        // If the local login is not valid check LDAP
        if (!output.ActionResponse.Result && username.ToLower() != "admin") {
            // See if we can find this user in LDAP to get the full details

            if (tenantId == Guid.Empty) {
                // No Tenant Id was passed during login, so get the default tenant.
                string defaultTenantCode = DefaultTenantCode;
                if (!String.IsNullOrWhiteSpace(defaultTenantCode)) {
                    var defaultTenant = await GetTenantFromCode(defaultTenantCode);
                    if (defaultTenant != null) {
                        tenantId = defaultTenant.TenantId;
                    }
                }
            }
            if (tenantId == Guid.Empty) {
                // No default tenant was found, so use the app default.
                tenantId = _guid2;
            }

            var ldapAuth = AuthenticateWithLDAP(tenantId, username, password);
            if (ldapAuth != null) {
                output.ActionResponse.Result = true;
                FoundActiveDirectoryInfo = true;
                DepartmentName = ldapAuth.Department;

                output.TenantId = tenantId;
                output.UserId = GuidValue(ldapAuth.UserId);
                output.Username = username;
                output.FirstName = ldapAuth.FirstName;
                output.LastName = ldapAuth.LastName;
                output.Email = ldapAuth.Email;
                output.Phone = ldapAuth.Phone;
                output.EmployeeId = ldapAuth.EmployeeId;
                output.Title = ldapAuth.Title;
                output.Location = ldapAuth.Location;
            }
        }

        // Only continue if we have a valid login
        if (output.ActionResponse.Result) {
            Guid DepartmentId = await DepartmentIdFromNameAndLocation(tenantId, DepartmentName);

            User? rec = null;
            if (userId != Guid.Empty) {
                rec = await data.Users.FirstOrDefaultAsync(x => x.UserId == userId && x.Enabled == true);
            } else {
                rec = await data.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower() && x.Enabled == true);
            }

            if (rec == null) {
                // This account does not yet exist, so create it if this tenant allows via the RequirePreExistingAccountToLogIn setting.
                if (tenant == null) {
                    if (tenantId == Guid.Empty) {
                        // Get the default tenant.
                        string defaultTenantCode = DefaultTenantCode;
                        if (!String.IsNullOrWhiteSpace(defaultTenantCode)) {
                            tenant = await GetTenantFromCode(defaultTenantCode);
                        }
                    } else {
                        tenant = GetTenant(tenantId);
                    }
                }

                if (tenant == null) {
                    // If the tenant is still null just get the default tenant for the default customer account.
                    tenant = GetTenant(_guid2);
                }

                bool requirePreExistingAccountToLogIn = false;
                if (tenant != null) {
                    requirePreExistingAccountToLogIn = tenant.TenantSettings.RequirePreExistingAccountToLogIn;
                }

                if (requirePreExistingAccountToLogIn) {
                    // No preexisting account, so login is not allowed for this tenant.
                } else {
                    if (output.UserId == Guid.Empty) {
                        output.UserId = Guid.NewGuid();
                    }

                    try {
                        await data.Users.AddAsync(new User {
                            TenantId = tenantId,
                            UserId = output.UserId,
                            DepartmentId = DepartmentId != Guid.Empty ? DepartmentId : null,
                            Email = output.Email,
                            EmployeeId = output.EmployeeId,
                            Enabled = true,
                            FirstName = output.FirstName,
                            LastLogin = DateTime.UtcNow,
                            LastName = output.LastName,
                            Username = username,
                            Phone = output.Phone,
                            Location = output.Location,
                            Title = output.Title,
                            Source = "LDAP"
                        });

                        await data.SaveChangesAsync();
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
                    // Update the account info with the results from Active Directory if a value was returned
                    // and no local value exists
                    if (!String.IsNullOrWhiteSpace(output.Email)) {
                        rec.Email = output.Email;
                    }

                    if (!String.IsNullOrWhiteSpace(output.FirstName)) {
                        rec.FirstName = output.FirstName;
                    }

                    rec.LastLogin = DateTime.UtcNow;

                    if (!String.IsNullOrWhiteSpace(output.LastName)) {
                        rec.LastName = output.LastName;
                    }

                    if (rec.UserId != output.UserId) {
                        // Eventually, we would update this and all related data, but for now we will just change the output
                        output.UserId = rec.UserId;
                    }

                    if (!String.IsNullOrWhiteSpace(output.Username)) {
                        rec.Username = output.Username;
                    }

                    if (!String.IsNullOrWhiteSpace(output.Phone)) {
                        rec.Phone = output.Phone;
                    }

                    if (!String.IsNullOrWhiteSpace(output.Location)) {
                        rec.Location = output.Location;
                    }

                    if (!String.IsNullOrWhiteSpace(output.Title)) {
                        rec.Title = output.Title;
                    }

                    rec.LastLockoutDate = null;
                    rec.FailedLoginAttempts = null;

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
            await SetUserLockout(tenantId, username);
        } else {
            // Get a fresh copy of the user object
            output = await GetUser(output.UserId);
            output.AuthToken = GetUserToken(output.TenantId, output.UserId);
        }

        return output;
    }

    private async Task SetUserLockout(Guid TenantId, string Username)
    {
        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username == Username);
        if (rec != null) {
            int currentAttempts = rec.FailedLoginAttempts.HasValue ? (int)rec.FailedLoginAttempts : 0;
            currentAttempts += 1;

            if (currentAttempts >= _accountLockoutMaxAttempts) {
                // Mark the account as locked out.
                rec.LastLockoutDate = DateTime.UtcNow;
            }
            rec.FailedLoginAttempts = currentAttempts;

            await data.SaveChangesAsync();
        }
    }
}