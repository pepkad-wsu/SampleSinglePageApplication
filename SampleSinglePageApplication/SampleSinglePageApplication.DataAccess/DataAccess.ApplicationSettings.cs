namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public DataObjects.ApplicationSettings GetApplicationSettings()
    {
        DataObjects.ApplicationSettings output = new DataObjects.ApplicationSettings {
            ActionResponse = GetNewActionResponse(true),
            ApplicationURL = ApplicationURL,
            DefaultTenantCode = DefaultTenantCode,
            EncryptionKey = ConvertByteArrayToString(GetEncryptionKey),
            MailServer = MailServer,
            MailServerPassword = MailServerPassword,
            MailServerPort = MailServerPort,
            MailServerUsername = MailServerUsername,
            MailServerUseSSL = MailServerUsesSSL,
            DefaultReplyToAddress = DefaultReplyToAddress,
            UseTenantCodeInUrl = UseTenantCodeInUrl,
            ShowTenantCodeFieldOnLoginForm = ShowTenantCodeFieldOnLoginForm,
            ShowTenantListingWhenMissingTenantCode = ShowTenantListingWhenMissingTenantCode
        };

        return output;
    }

    public async Task<DataObjects.ApplicationSettings> SaveApplicationSettings(DataObjects.ApplicationSettings settings, DataObjects.User CurrentUser)
    {
        DataObjects.ApplicationSettings output = settings;
        output.ActionResponse = GetNewActionResponse();

        if (!CurrentUser.AppAdmin) {
            output.ActionResponse.Messages.Add("Access Denied");
            return output;
        }

        // Save each individual settings item.
        var saved1 = SaveSetting("ApplicationURL", DataObjects.SettingType.Text, output.ApplicationURL);
        var saved2 = SaveSetting("DefaultTenantCode", DataObjects.SettingType.Text, output.DefaultTenantCode);
        var saved3 = SaveSetting("MailServer", DataObjects.SettingType.Text, output.MailServer);
        var saved4 = SaveSetting("MailServerPassword", DataObjects.SettingType.EncryptedText, output.MailServerPassword);
        var saved5 = SaveSetting("MailServerPort", DataObjects.SettingType.NumberInt, output.MailServerPort);
        var saved6 = SaveSetting("MailServerUsername", DataObjects.SettingType.EncryptedText, output.MailServerUsername);
        var saved7 = SaveSetting("MailServerUseSSL", DataObjects.SettingType.Boolean, output.MailServerUseSSL);
        var saved8 = SaveSetting("DefaultReplyToAddress", DataObjects.SettingType.Text, output.DefaultReplyToAddress);
        var saved9 = SaveSetting("UseTenantCodeInUrl", DataObjects.SettingType.Boolean, output.UseTenantCodeInUrl);
        var saved10 = SaveSetting("ShowTenantCodeFieldOnLoginForm", DataObjects.SettingType.Boolean, output.ShowTenantCodeFieldOnLoginForm);
        var saved11 = SaveSetting("ShowTenantListingWhenMissingTenantCode", DataObjects.SettingType.Boolean, output.ShowTenantListingWhenMissingTenantCode);

        List<string>? errors = null;
        if (!saved1.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved1.Messages); }
        if (!saved2.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved2.Messages); }
        if (!saved3.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved3.Messages); }
        if (!saved4.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved4.Messages); }
        if (!saved5.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved5.Messages); }
        if (!saved6.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved6.Messages); }
        if (!saved7.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved7.Messages); }
        if (!saved8.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved8.Messages); }
        if (!saved9.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved9.Messages); }
        if (!saved10.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved10.Messages); }
        if (!saved11.Result) { errors = Utilities.ConcatenateListsOfStrings(errors, saved11.Messages); }

        if (errors != null && errors.Count() > 0) {
            output.ActionResponse.Messages = errors;
            return output;
        } else {
            output.ActionResponse.Result = true;
        }

        CacheStore.SetCacheItem(Guid.Empty, "ApplicationURL", output.ApplicationURL);
        CacheStore.SetCacheItem(Guid.Empty, "DefaultReplyToAddress", output.DefaultReplyToAddress);
        CacheStore.SetCacheItem(Guid.Empty, "DefaultTenantCode", output.DefaultTenantCode);
        CacheStore.SetCacheItem(Guid.Empty, "MailServer", output.MailServer);
        CacheStore.SetCacheItem(Guid.Empty, "MailServerPassword", output.MailServerPassword);
        CacheStore.SetCacheItem(Guid.Empty, "MailServerPort", output.MailServerPort);
        CacheStore.SetCacheItem(Guid.Empty, "MailServerUsername", output.MailServerUsername);
        CacheStore.SetCacheItem(Guid.Empty, "MailServerUseSSL", output.MailServerUseSSL);
        CacheStore.SetCacheItem(Guid.Empty, "ShowTenantCodeFieldOnLoginForm", output.ShowTenantCodeFieldOnLoginForm);
        CacheStore.SetCacheItem(Guid.Empty, "ShowTenantListingWhenMissingTenantCode", output.ShowTenantListingWhenMissingTenantCode);
        CacheStore.SetCacheItem(Guid.Empty, "UseTenantCodeInUrl", output.UseTenantCodeInUrl);

        // See if the EncryptionKey value has changed.
        var encryptionKey = GetSetting<string>("EncryptionKey", DataObjects.SettingType.Text);
        if (output.EncryptionKey != encryptionKey) {
            var keyChanged = UpdateApplicationEncryptionKey(encryptionKey, output.EncryptionKey);
            if (!keyChanged.Result) {
                output.ActionResponse.Result = false;
                Utilities.ConcatenateListsOfStrings(output.ActionResponse.Messages, keyChanged.Messages);
            }
        }

        if (output.ActionResponse.Result) {
            // Get a fresh copy of the settings to return
            output = GetApplicationSettings();
        }

        // Send out a SignalR update to every tenant
        var tenants = await GetTenants();
        foreach (var tenant in tenants) {
            if (tenant.Enabled) {
                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = tenant.TenantId,
                    UpdateType = DataObjects.SignalRUpdateType.Setting,
                    Message = "UseTenantCodeInUrl",
                    ItemId = output.UseTenantCodeInUrl ? "1" : "0"
                });
            }
        }

        return output;
    }
}