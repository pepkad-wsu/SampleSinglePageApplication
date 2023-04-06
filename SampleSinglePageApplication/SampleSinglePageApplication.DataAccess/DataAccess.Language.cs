namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public DataObjects.Language GetDefaultLanguage()
    {
        DataObjects.Language output = new DataObjects.Language {
            Culture = "en-US",
            Phrases = new List<DataObjects.OptionPair>()
        };

        Dictionary<string, string> language = new Dictionary<string, string> {
            { "AccessDenied", "Access Denied" },
            { "Action", "Action" },
            { "Active", "Active" },
            { "ActiveDirectoryNames", "Active Directory Names" },
            { "ActiveDirectoryNamesInfo", "Bracket-separated names of AD groups to match for this department (eg: {Enrollment IT]{Admissions}, etc.)" },
            { "Add", "Add" },
            { "Added", "Added" },
            { "AddLanguage", "Add a Language" },
            { "AddLanguageInfo", "Select a new language to add. The default English (en-US) language elements will be copied into a new language set for the selected language. You can then translate the words and phrases into the desired language." },
            { "AddModule", "Add a New Module" },
            { "AddNewDepartment", "Add a New Department" },
            { "AddNewDepartmentGroup", "Add a New Department Group" },
            { "AddNewUser", "Add a New User" },
            { "AddNewUserGroup", "Add a New User Group" },
            { "AddTenant", "Add a New Tenant" },
            { "AddUsersToGroup", "Add Users to Group" },
            { "Admin", "Admin" },
            { "AllItems", "All Items" },
            { "AllowedFileTypes", "Allowed File Types" },
            { "AllowUsersToManageAvatar", "Allow Users to Manage Avatars" },
            { "AllowUsersToManageBasicProfileInfo", "Allow Users to Manage Basic Profile Info" },
            { "AllowUsersToManageBasicProfileInfoElements", "Select the User Profile Elements Users Can Edit" },
            { "AllowUsersToResetLocalPasswordsAtLogin", "Allow Users to Reset Passwords on the Local Login Form" },
            { "AllowUsersToSignUpAtLogin", "Allow Users to Sign Up for an Account on the Local Login Form" },
            { "AppSettings", "Application Settings" },
            { "AppTitle", "App Title" },
            { "AppUrl", "Application URL" },
            { "AutoCompleteUserLookup", "User Lookup" },
            { "Back", "Back" },
            { "BackToLogin", "Back to Login" },
            { "Cancel", "Cancel" },
            { "ChangePassword", "Change Password" },
            { "ChangePasswordInstructions", "To change your password enter your current password, your new password, and confirm your new password." },
            { "Clear", "Clear" },
            { "Code", "Code" },
            { "ConfirmDelete", "Confirm Delete" },
            { "ConfirmDeleteTenant", "Confirm Delete Tenant" },
            { "ConfirmPassword", "Confirm Password" },
            { "CookieDomain", "CookieDomain" },
            { "Created", "Created" },
            { "CurrentPassword", "Current Password" },
            { "DefaultCultureCode", "Default Culture Code" },
            { "DefaultReplyToAddress", "Default Reply-to Email Address" },
            { "DefaultTenantCode", "Default Tenant Code" },
            { "Delete", "Delete" },
            { "DeleteAvatar", "Delete Current Avatar Photo" },
            { "DeleteTenant", "Delete Tenant" },
            { "DeleteTenantWarning", "WARNING: Your are about to delete a tenant. There is no 'undo' for this operation. To confirm, type 'CONFIRM' below:" },
            { "DeletingTenant", "Deleting Tenant, Please Wait..." },
            { "DeletingTenantNotification", "Deleting tenant. Do not close this window until complete." },
            { "DeletingWait", "Deleting, Please Wait..." },
            { "Department", "Department" },
            { "DepartmentGroup", "Department Group" },
            { "DepartmentGroupName", "Department Group Name" },
            { "DepartmentGroups", "Department Groups" },
            { "DepartmentName", "Department Name" },
            { "Departments", "Departments" },
            { "Description", "Description" },
            { "Edit", "Edit" },
            { "EditDepartment", "Edit Department" },
            { "EditDepartmentGroup", "Edit Department Group" },
            { "EditLanguage", "Edit Language" },
            { "EditTenant", "Edit Tenant" },
            { "EditUser", "Edit User" },
            { "EditUserGroup", "Edit User Group" },
            { "Email", "Email" },
            { "EmailAddress", "Email Address" },
            { "EmployeeId", "Employee ID" },
            { "Enabled", "Enabled" },
            { "EncryptionKey", "Encryption Key" },
            { "EncryptionKeyWarning", "<strong>WARNING</strong><br />Modifying the Encryption Key is very dangerous. This is the key used by the application to encrypt all sensitive data.<br /><br />When changing this value you must supply a valid 32-bit key represeted as a byte array string (eg: 0x00,0x01,0x02,etc.).<br /><br />When this value is changed the software will attempt to decrypt all values in the database, the key will be updated, then those values will be re-encrypted. If this fails you will be left with encrypted data that cannot be accessed." },
            { "EndDate", "End Date" },
            { "FirstName", "First Name" },
            { "ForgotPassword", "Forgot Password" },
            { "ForgotPasswordValidateInstructions", "To validate your new password, enter the code that was emailed to the address you provided." },
            { "Help", "Help" },
            { "HideHelp", "Hide Help" },
            { "HideFilter", "Hide Filter" },
            { "HomeMenuIcon", "<i class=\"fa-solid fa-house\"></i>" },
            { "HomeMenuText", "Home" },
            { "HomePage", "Home Page" },
            { "HtmlEditorPlaceholder", "Enter Your HTML Here" },
            { "IncludeInSearch", "Include in Search" },
            { "IncludeInSearchInfo", "If this option is selected this field will be included when using the keyword search." },
            { "InvalidImageFileType", "Invalid Image File Type" },
            { "InvalidLogin", "Invalid Login" },
            { "InvalidLoginNoLocalAccount", "Your credentials were valid, but you do not have an account configured in this application." },
            { "InvalidTenantCode", "Invalid Tenant Code" },
            { "InvalidTenantCodeMessage", "Please check your URL and ensure a valid URL is used." },
            { "InvalidUsernameOrPassword", "Invalid Username or Password" },
            { "JsonWebTokenKey", "JSON Web Token Key" },
            { "Label", "Label" },
            { "Language", "Language" },
            { "LastLogin", "Last Login" },
            { "LastName", "Last Name" },
            { "Loading", "Loading" },
            { "LoadingWait", "Loading, Please Wait..." },
            { "Location", "Location" },
            { "Login", "Login" },
            { "LoginIntro", "Select a login option below:" },
            { "LoginOptions", "Login Options" },
            { "LoginTextPassword", "Password" },
            { "LoginTextTenantCode", "Tenant Code" },
            { "LoginTextUsername", "Username" },
            { "LoginTitle", "Login Required" },
            { "LoginSuccessMessage", "Logged In, Please Wait..." },
            { "LoginWithEITSSO", "Login with OKTA Single Sign-On" },
            { "LoginWithFacebook", "Log in with Facebook" },
            { "LoginWithGoogle", "Log in with Google" },
            { "LoginWithLocalAccount", "Log in with a Local Account" },
            { "LoginWithMicrosoftAccount", "Log in with a Microsoft Account" },
            { "LoginWithOpenId", "Log in with OpenId" },
            { "Log-in", "Log In" },
            { "Logout", "Logout" },
            { "Log-out", "Log Out" },
            { "MailServer", "Mail Server" },
            { "MailServerConfiguration", "Mail Server Configuration" },
            { "MailServerPassword", "Mail Server Password" },
            { "MailServerPort", "Mail Server Port" },
            { "MailServerUsername", "Mail Server Username" },
            { "MailServerUsesSSL", "Mail Server Uses SSL" },
            { "ManageAvatar", "Manage Your Avatar" },
            { "ManageAvatarInstructions", "Drag and drop an image here to upload a user image. Images are limited to a maximum of 5MB." },
            { "ManageAvatarInstructionsAdmin", "Drag and drop an image here to upload a user image. Images are limited to a maximum of 5MB. There is no need to save after uploading the user photo, as the photo is saved automatically." },
            { "ManageProfile", "Manage Profile" },
            { "ManageProfileInfo", "Manage Basic Profile Info" },
            { "ManageProfileInfoInstructions", "You can make some basic profile updates below." },
            { "MenuAdmin", "Admin" },
            { "MenuAdminDepartments", "Departments" },
            { "MenuAdminSettings", "Settings" },
            { "MenuAdminUDFLabels", "UDF Labels" },
            { "MenuAdminUsers", "Users" },
            { "Modified", "Modified" },
            { "ModifiedItems", "Modified Items" },
            { "Module", "Module" },
            { "ModuleName", "Module Name" },
            { "ModuleNameInfo", "Use a name in Pascal Case with no spaces (eg: MyModule)" },
            { "MostRecentFirst", "most-recent items listed first" },
            { "Name", "Name" },
            { "New", "New" },
            { "NewPassword", "New Password" },
            { "NewPasswordAndConfirmDontMatch", "New Password and Confirm Password Don't Match" },
            { "NewTenant", "New Tenant" },
            { "NewUserGroup", "New User Group" },
            { "NoItemsToShow", "There are no items to show." },
            { "Ok", "Ok" },
            { "Option", "Option" },
            { "Password", "Password" },
            { "PasswordChanged", "Password Changed" },
            { "PasswordReset", "Password Reset" },
            { "PasswordResetMessage", "Your password has been updated. You may now return to the login page and log in using your new password." },
            { "PhoneNumber", "Phone Number" },
            { "Photo", "Photo" },
            { "PreventPasswordChange", "Prevent User from Changing Password" },
            { "ProcessingWait", "Processing, Please Wait..." },
            { "RecordNavigationFirst", "Go to First Page of Records" },
            { "RecordNavigationLast", "Go to Last Page of Records" },
            { "RecordNavigationNext", "Go to Next Page of Records" },
            { "RecordNavigationPrevious", "Go to Previous Page of Records" },
            { "Refresh", "Refresh" },
            { "RequiredMissing", "{0} is Required" },
            { "RequirePreExistingAccount", "Require Pre-Existing Account to Log In" },
            { "RequirePreExistingAccountInfo", "If this is set to true then a user cannot login unless a user account already exists in the database. For applications that should allow any user to log in set this to false and a new user account will be created when they log in if there is no user account already in the users table." },
            { "ResetLanguageDefaults", "Reset All Language to Defaults" },
            { "ResetPassword", "Reset Password" },
            { "ResetUserPassword", "Reset User Password" },
            { "SamplePage", "Sample Page" },
            { "Save", "Save" },
            { "Saved", "Saved" },
            { "SavedAt", "Saved at" },
            { "Saving", "Saving" },
            { "SavingWait", "Saving, Please Wait..." },
            { "SelectCulture", "Select a Language Culture" },
            { "SelectTenant", "Select a Tenant" },
            { "SelectTheme", "Select Theme" },
            { "Settings", "Settings" },
            { "ServerOffline", "Server Offline, Attempting to Reconnect..." },
            { "ServerUpdated", "Server Updated" },
            { "ServerUpdatedMessage", "The server has been updated, refreshing the application..." },
            { "ShowColumn", "Show Column" },
            { "ShowColumnInfo", "Indicates if this field should be show when listing records. Any UDF with a value in the Label field will be shown when editing a record. The Show Column is only used to toggle this column on and off in the records view." },
            { "ShowFilter", "Show Filter" },
            { "ShowHelp", "Show Help" },
            { "ShowInFilter", "Show in Filter" },
            { "ShowInFilterInfo", "If this option is selected this field will be shown in the filter options as a select list. Only use this option on fields with limited values, as every distinct value from the records will be used to create this list." },
            { "ShowTenantCodeFieldOnLoginForm", "Show Tenant Code Field on Login Form" },
            { "ShowTenantListingWhenMissingTenantCode", "Show Tenant Listing When Missing Tenant Code" },
            { "SignUp", "Sign Up" },
            { "SignUpInstructions", "Complete all of the following fields to sign up for an account. A confirmation code will be emailed to the address you provide to confirm that it is a valid email address. You will then enter that validation code to finish the sign up process." },
            { "Sort", "Sort" },
            { "SortOrder", "Sort Order" },
            { "Source", "Source" },
            { "StartDate", "Start Date" },
            { "SupportedFileTypes", "Supported File Types" },
            { "SwitchAccountMessage", "You have access to multiple app instances. You can switch to another instance below:" },
            { "Tenant", "Tenant" },
            { "TenantCode", "Code" },
            { "TenantId", "TenantId" },
            { "TenantName", "Name" },
            { "Tenants", "Tenants" },
            { "Theme", "Theme" },
            { "ThemeAuto", "Auto (based on system settings)" },
            { "ThemeDark", "Dark" },
            { "ThemeLight", "Light" },
            { "Title", "Title" },
            { "UDF", "UDF" },
            { "UdfHelpIntro", "For a simple text input just enter the field name in the Label field.<br /><br />Alternatively, you can specify that the field should be a select element or a list of radio buttons by using the following format:<br /><br />Label|select|options,separated,by,commas<br />Label|radio|options,separated,by,commas" },
            { "UdfLabel", "User-Defined Field Label" },
            { "UdfLabels", "User-Defined Field Labels" },
            { "UdfOptions", "Options:" },
            { "UdfShowConflictNote", "NOTE: While it is possible to use both Show in Filter and Include in Search for an item, this is not recommended as it is a duplication of searching. If the item is shown as a filter you can quickly find those items by clicking the appropriate filter option. Adding unneccesary fields to the Include in Search can affect performance." },
            { "UnlockUserAccount", "Unlock User Account" },
            { "UserLockedOut", "User account is locked out at {0} due to too many failed login attempts." },
            { "UpdatePassword", "Update Password" },
            { "UploadFile", "Upload a File" },
            { "User", "User" },
            { "UserGroups", "User Groups" },
            { "Username", "Username" },
            { "Users", "Users" },
            { "UsersInGroup", "Users in Group" },
            { "UseTenantCodeInUrl", "Use Tenant Code in URL" },
            { "ValidateConfirmationCode", "Validate Confirmation Code" },
            { "ValidatingLogin", "Validating Login, Please Wait..." },
            { "ValidationCode", "Validation Code" },
            { "Welcome", "Welcome" }
        };

        foreach (var item in language) {
            output.Phrases.Add(new DataObjects.OptionPair {
                Id = item.Key,
                Value = item.Value
            });
        }

        return output;
    }

    public List<DataObjects.OptionPair> GetLanguageCultureCodes()
    {
        List<DataObjects.OptionPair> output = new List<DataObjects.OptionPair>();

        var ci = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
        if (ci != null && ci.Any()) {
            foreach (var c in ci.Where(x => !String.IsNullOrEmpty(x.Name) && !x.IsNeutralCulture)
                .OrderBy(x => x.DisplayName)) {
                output.Add(new DataObjects.OptionPair {
                    Id = c.Name,
                    Value = c.DisplayName
                });
            }
        }

        return output;
    }

    public async Task<List<string>> GetLanguageCultures(Guid TenantId)
    {
        List<string> output = new List<string>();

        var recs = await data.Settings
            .Where(x => x.TenantId == TenantId && x.SettingName != null && x.SettingName.ToLower().StartsWith("language_"))
            .ToListAsync();

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                string culture = StringValue(rec.SettingName).Substring(9);
                if (!String.IsNullOrWhiteSpace(culture)) {
                    output.Add(culture);
                }
            }
        }

        return output;
    }

    public string GetLanguageItem(string? item, DataObjects.Language? language = null)
    {
        string output = String.Empty;

        if (language == null) {
            language = GetDefaultLanguage();
        }

        if (language.Phrases == null || !language.Phrases.Any()) {
            language = GetDefaultLanguage();
        }

        if (language != null && language.Phrases != null && language.Phrases.Any() && !String.IsNullOrEmpty(item)) {
            var phrase = language.Phrases.FirstOrDefault(x => x.Id != null && x.Id.ToLower() == item.ToLower());
            if (phrase != null) {
                output += phrase.Value;
            }
        }

        if (String.IsNullOrEmpty(output) && !String.IsNullOrEmpty(item)) {
            output = item.ToUpper();
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> SaveLanguage(Guid TenantId, DataObjects.Language language)
    {
        if (String.IsNullOrWhiteSpace(language.Culture)) {
            language.Culture = "en-US";
        }

        var output = SaveSetting("Language_" + language.Culture, DataObjects.SettingType.Object, language.Phrases, TenantId);

        if (output.Result) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = TenantId,
                UpdateType = DataObjects.SignalRUpdateType.Setting,
                Message = "Language",
                Object = language
            });
        }

        return output;
    }
}