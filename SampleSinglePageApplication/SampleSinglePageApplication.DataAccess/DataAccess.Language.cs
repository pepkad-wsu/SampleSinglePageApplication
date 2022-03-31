﻿namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public List<DataObjects.OptionPair> GetDefaultLanguage()
    {
        List<DataObjects.OptionPair> output = new List<DataObjects.OptionPair>();

        Dictionary<string, string> language = new Dictionary<string, string> {
            { "Action", "Action" },
            { "Active", "Active" },
            { "ActiveDirectoryNames", "Active Directory Names" },
            { "ActiveDirectoryNamesInfo", "Bracket-separated names of AD groups to match for this department (eg: {Enrollment IT]{Admissions}, etc.)" },
            { "Added", "Added" },
            { "AddNewDepartment", "Add a New Department" },
            { "AddNewDepartmentGroup", "Add a New Department Group" },
            { "AddNewUser", "Add a New User" },
            { "AddTenant", "Add a New Tenant" },
            { "Admin", "Admin" },
            { "AppTitle", "App Title" },
            { "AutoCompleteUserLookup", "User Lookup" },
            { "Back", "Back" },
            { "Cancel", "Cancel" },
            { "ChangePassword", "Change Password" },
            { "ChangePasswordInstructions", "To change your password enter your current password, your new password, and confirm your new password." },
            { "Clear", "Clear" },
            { "ConfirmDelete", "Confirm Delete" },
            { "ConfirmDeleteTenant", "Confirm Delete Tenant" },
            { "ConfirmPassword", "Confirm Password" },
            { "Created", "Created" },
            { "CurrentPassword", "Current Password" },
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
            { "EditTenant", "Edit Tenant" },
            { "EditUser", "Edit User" },
            { "Email", "Email" },
            { "EmailAddress", "Email Address" },
            { "EmployeeId", "Employee ID" },
            { "Enabled", "Enabled" },
            { "EndDate", "End Date" },
            { "FirstName", "First Name" },
            { "Help", "Help" },
            { "HideHelp", "Hide Help" },
            { "HideInStats", "Hide in Statistics" },
            { "HideFilter", "Hide Filter" },
            { "HomeMenuIcon", "<i class=\"fa-solid fa-house\"></i>" },
            { "HomeMenuText", "Home" },
            { "HomePage", "Home Page" },
            { "InvalidImageFileType", "Invalid Image File Type" },
            { "InvalidUsernameOrPassword", "Invalid Username or Password" },
            { "Language", "Language" },
            { "LastName", "Last Name" },
            { "Loading", "Loading" },
            { "LoadingWait", "Loading, Please Wait..." },
            { "Location", "Location" },
            { "Login", "Login" },
            { "LoginIntro", "Select a login option below:" },
            { "LoginTitle", "Login Required" },
            { "LoginSuccessMessage", "Logged In, Please Wait..." },
            { "LoginWithEITSSO", "Login with OKTA Single Sign-On" },
            { "LoginWithLocalAccount", "Log in with a Local Account" },
            { "Log-in", "Log In" },
            { "Logout", "Logout" },
            { "Log-out", "Log Out" },
            { "ManageAvatar", "Manage Your Avatar" },
            { "ManageAvatarInstructions", "Drag and drop an image here to upload a user image. Images are limited to a maximum of 5MB." },
            { "ManageAvatarInstructionsAdmin", "Drag and drop an image here to upload a user image. Images are limited to a maximum of 5MB. There is no need to save after uploading the user photo, as the photo is saved automatically." },
            { "ManageProfile", "Manage Profile" },
            { "ManageProfileInfo", "Manage Basic Profile Info" },
            { "ManageProfileInfoInstructions", "You can make some basic profile updates below." },
            { "MostRecentFirst", "most-recent items listed first" },
            { "MenuAdmin", "Admin" },
            { "MenuAdminDepartments", "Departments" },
            { "MenuAdminSettings", "Settings" },
            { "MenuAdminUsers", "Users" },
            { "Modified", "Modified" },
            { "New", "New" },
            { "NewPassword", "New Password" },
            { "NewPasswordAndConfirmDontMatch", "New Password and Confirm Password Don't Match" },
            { "NewTenant", "New Tenant" },
            { "NoItemsToShow", "There are no items to show." },
            { "Ok", "Ok" },
            { "Option", "Option" },
            { "Password", "Password" },
            { "PasswordChanged", "Password Changed" },
            { "PasswordReset", "Password Reset" },
            { "PhoneNumber", "Phone Number" },
            { "RecordNavigationFirst", "Go to First Page of Records" },
            { "RecordNavigationLast", "Go to Last Page of Records" },
            { "RecordNavigationNext", "Go to Next Page of Records" },
            { "RecordNavigationPrevious", "Go to Previous Page of Records" },
            { "Refresh", "Refresh" },
            { "RequiredMissing", "{0} is Required" },
            { "ResetUserPassword", "Reset User Password" },
            { "Save", "Save" },
            { "Saved", "Saved" },
            { "SavedAt", "Saved at" },
            { "Saving", "Saving" },
            { "SavingWait", "Saving, Please Wait..." },
            { "Settings", "Settings" },
            { "ShowFilter", "Show Filter" },
            { "ShowHelp", "Show Help" },
            { "Sort", "Sort" },
            { "SortOrder", "Sort Order" },
            { "StartDate", "Start Date" },
            { "SupportedFileTypes", "Supported File Types" },
            { "SwitchAccountMessage", "You have access to multiple app instances. You can switch to another instance below:" },
            { "Tenant", "Tenant" },
            { "TenantCode", "Code" },
            { "TenantId", "TenantId" },
            { "TenantName", "Name" },
            { "Tenants", "Tenants" },
            { "Theme", "Theme" },
            { "Title", "Title" },
            { "UpdatePassword", "Update Password" },
            { "UploadFile", "Upload a File" },
            { "User", "User" },
            { "Username", "Username" },
            { "Users", "Users" },
            { "ValidatingLogin", "Validating Login, Please Wait..." },
            { "Welcome", "Welcome" }
        };

        foreach (var item in language) {
            output.Add(new DataObjects.OptionPair {
                Id = item.Key,
                Value = item.Value
            });
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> SaveLanguage(Guid TenantId, List<DataObjects.OptionPair> language)
    {
        var output = SaveSetting("Language", DataObjects.SettingType.Object, language, TenantId);

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