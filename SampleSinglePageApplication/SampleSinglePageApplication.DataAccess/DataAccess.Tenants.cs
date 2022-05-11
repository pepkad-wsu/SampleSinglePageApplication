namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public string DefaultTenantCode {
        get {
            string output = String.Empty;

            if (_open) {
                var defaultTenantCode = GetSetting<string>("DefaultTenantCode", DataObjects.SettingType.Text);
                if (!String.IsNullOrEmpty(defaultTenantCode)) {
                    output = defaultTenantCode;
                }

                if (String.IsNullOrEmpty(defaultTenantCode)) {
                    var rec = data.Tenants.FirstOrDefault(x => x.TenantId == _guid2);
                    if (rec != null) {
                        output = rec.TenantCode;
                    }
                }
            }

            return output;
        }
    }

    public async Task<DataObjects.BooleanResponse> DeleteTenant(Guid TenantId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (TenantId == _guid1) {
            output.Messages.Add("Cannot delete the built-in Admin account");
            return output;
        } else if (TenantId == _guid2) {
            output.Messages.Add("Cannot delete the built-in initial customer account.");
            return output;
        }

        try {
            if (_inMemoryDatabase) {
                data.FileStorages.RemoveRange(data.FileStorages.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                data.Settings.RemoveRange(data.Settings.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                data.DepartmentGroups.RemoveRange(data.DepartmentGroups.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                data.Departments.RemoveRange(data.Departments.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                data.Users.RemoveRange(data.Users.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                data.UDFLabels.RemoveRange(data.UDFLabels.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                data.Tenants.RemoveRange(data.Tenants.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
            } else {
                // Delete from all tables
                await data.Database.ExecuteSqlRawAsync("DELETE FROM FileStorage WHERE TenantId={0}", TenantId);
                await data.Database.ExecuteSqlRawAsync("DELETE FROM Settings WHERE TenantId={0}", TenantId);
                await data.Database.ExecuteSqlRawAsync("DELETE FROM DepartmentGroups WHERE TenantId={0}", TenantId);
                await data.Database.ExecuteSqlRawAsync("DELETE FROM Departments WHERE TenantId={0}", TenantId);
                await data.Database.ExecuteSqlRawAsync("DELETE FROM Users WHERE TenantId={0}", TenantId);
                await data.Database.ExecuteSqlRawAsync("DELETE FROM UDFLabels WHERE TenantId={0}", TenantId);
                await data.Database.ExecuteSqlRawAsync("DELETE FROM Tenants WHERE TenantId={0}", TenantId);
            }
        } catch (Exception ex) {
            output.Messages.Add("An error occurred attempting to delete the tenant '" + TenantId.ToString() + "' - " + ex.Message);
        }

        output.Result = output.Messages.Count() == 0;

        return output;
    }

    public DataObjects.Tenant? GetTenant(Guid TenantId)
    {
        DataObjects.Tenant? output = null;

        var rec = data.Tenants.FirstOrDefault(x => x.TenantId == TenantId);
        if (rec != null) {
            output = new DataObjects.Tenant {
                ActionResponse = GetNewActionResponse(true),
                TenantId = rec.TenantId,
                TenantCode = rec.TenantCode,
                Name = rec.Name,
                Enabled = rec.Enabled
            };

            var settings = GetTenantSettings(TenantId);
            if (settings != null) {
                output.TenantSettings = settings;
            }
        }

        return output;
    }

    public async Task<DataObjects.Tenant> GetTenantFull(Guid TenantId)
    {
        DataObjects.Tenant output = new DataObjects.Tenant();

        var cached = CacheStore.GetCachedItem<DataObjects.Tenant>(TenantId, "FullTenant");
        if (cached != null) {
            output = cached;
        } else {
            var tenant = GetTenant(TenantId);
            if (tenant != null) {
                output = tenant;
                output.Departments = await GetDepartments(TenantId);
                output.DepartmentGroups = await GetDepartmentGroups(TenantId);
                output.udfLabels = await GetUDFLabels(TenantId);
            }
            CacheStore.SetCacheItem(TenantId, "FullTenant", output);
        }

        return output;
    }

    public async Task<DataObjects.Tenant> GetTenantFromCode(string tenantCode)
    {
        DataObjects.Tenant output = new DataObjects.Tenant();

        Guid tenantId = Guid.Empty;
        var rec = await data.Tenants.FirstOrDefaultAsync(x => x.TenantCode == tenantCode);
        if (rec != null) {
            tenantId = rec.TenantId;
        }

        if(tenantId != Guid.Empty) {
            output = await GetTenantFull(tenantId);
        } else {
            output.ActionResponse.Messages.Add("Invalid Tenant Code '" + tenantCode + "'");
        }

        return output;
    }

    public Guid GetTenantIdFromCode(string tenantCode)
    {
        Guid output = Guid.Empty;

        if (!String.IsNullOrEmpty(tenantCode)) {
            var rec = data.Tenants.FirstOrDefault(x => x.TenantCode == tenantCode);
            if (rec != null) {
                output = rec.TenantId;
            }
        }

        return output;
    }

    public List<DataObjects.OptionPair> GetTenantLanguage(Guid TenantId)
    {
        var output = GetDefaultLanguage();

        // During development just use the default language as changes are being made too often.
        // return output;

        bool updated = false;

        // See if there is a saved language object for this tenant.
        var language = GetSetting<List<DataObjects.OptionPair>>("Language", DataObjects.SettingType.Object, TenantId);
        if (language != null) {

            // Go through each item in the defaults.
            // If any items in the Tenant's language are different than the defaults then update the output
            // and flag that it's been updated so we can save.
            List<DataObjects.OptionPair> missing = new List<DataObjects.OptionPair>();

            foreach (var item in output) {
                var tenantItem = language.FirstOrDefault(x => StringOrEmpty(x.Id).ToLower() == StringOrEmpty(item.Id).ToLower());
                if (tenantItem != null) {
                    string value = StringOrEmpty(tenantItem.Value);
                    if (item.Value != value) {
                        item.Value = value;
                        updated = true;
                    }
                } else {
                    // Item does not exist in the user's saved language, so add it
                    missing.Add(item);
                }
            }

            if(missing.Count() > 0) {
                foreach(var item in missing) {
                    output.Add(item);
                }
                output = output.OrderBy(x => x.Id).ToList();
            }
        } else {
            // Need to save for this Tenant with the defaults since they didn't have a value.
            updated = true;
        }

        if (updated) {
            SaveSetting("Language", DataObjects.SettingType.Object, output, TenantId);
        }

        return output;
    }

    public async Task<List<DataObjects.Tenant>> GetTenants()
    {
        List<DataObjects.Tenant> output = new List<DataObjects.Tenant>();

        var recs = await data.Tenants.ToListAsync();
        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                if (rec != null) {
                    var tenant = new DataObjects.Tenant {
                        ActionResponse = GetNewActionResponse(true),
                        TenantId = rec.TenantId,
                        TenantCode = rec.TenantCode,
                        Name = rec.Name,
                        Enabled = rec.Enabled
                    };

                    var settings = GetTenantSettings(tenant.TenantId);
                    if (settings != null) {
                        tenant.TenantSettings = settings;
                    }
                    output.Add(tenant);
                }
            }
        }

        return output;
    }

    public DataObjects.TenantSettings GetTenantSettings(Guid TenantId)
    {
        var defaultWorkSchedule = new DataObjects.WorkSchedule {
            Sunday = false,
            SundayAllDay = false,
            SundayStart = "",
            SundayEnd = "",

            Monday = true,
            MondayAllDay = false,
            MondayStart = "8:00 am",
            MondayEnd = "5:00 pm",

            Tuesday = true,
            TuesdayAllDay = false,
            TuesdayStart = "8:00 am",
            TuesdayEnd = "5:00 pm",

            Wednesday = true,
            WednesdayAllDay = false,
            WednesdayStart = "8:00 am",
            WednesdayEnd = "5:00 pm",

            Thursday = true,
            ThursdayAllDay = false,
            ThursdayStart = "8:00 am",
            ThursdayEnd = "5:00 pm",

            Friday = true,
            FridayAllDay = false,
            FridayStart = "8:00 am",
            FridayEnd = "5:00 pm",

            Saturday = false,
            SaturdayAllDay = false,
            SaturdayStart = "",
            SaturdayEnd = ""
        };

        DataObjects.TenantSettings output = new DataObjects.TenantSettings();

        var settings = GetSetting<DataObjects.TenantSettings>("Settings", DataObjects.SettingType.Object, TenantId);
        if (settings != null) {
            output = settings;
            if (settings.WorkSchedule == null) {
                settings.WorkSchedule = defaultWorkSchedule;
                SaveTenantSettings(TenantId, output);
            } else if (!settings.WorkSchedule.Sunday && !settings.WorkSchedule.Monday && !settings.WorkSchedule.Tuesday
                 && !settings.WorkSchedule.Wednesday && !settings.WorkSchedule.Thursday && !settings.WorkSchedule.Friday && !settings.WorkSchedule.Saturday) {
                settings.WorkSchedule = defaultWorkSchedule;
                SaveTenantSettings(TenantId, output);
            }
        } else {
            // Create default settings for this tenant.
            output = new DataObjects.TenantSettings {
                JasonWebTokenKey = Guid.NewGuid().ToString().Replace("-", ""),
                LoginOptions = new List<string>() { "local", "eitsso" },
                WorkSchedule = defaultWorkSchedule
            };

            SaveTenantSettings(TenantId, output);
        }

        if(!String.IsNullOrWhiteSpace(output.EitSsoUrl) && !output.EitSsoUrl.EndsWith("/")) {
            output.EitSsoUrl += "/";
        }

        return output;
    }

    public async Task<DataObjects.Tenant> SaveTenant(DataObjects.Tenant tenant)
    {
        DataObjects.Tenant output = tenant;
        output.ActionResponse = GetNewActionResponse();

        if (tenant.TenantId != _guid1 && !String.IsNullOrEmpty(tenant.TenantCode) && tenant.TenantCode.ToLower() == "admin") {
            output.ActionResponse.Messages.Add("Invalid TenantCode. 'Admin' is a Reserved Code.");
            return output;
        }

        bool newRecord = false;
        var rec = await data.Tenants.FirstOrDefaultAsync(x => x.TenantId == tenant.TenantId);
        if (rec == null) {
            if (output.TenantId == Guid.Empty) {
                newRecord = true;
                output.TenantId = Guid.NewGuid();
                rec = new Tenant();
                rec.TenantId = output.TenantId;
            } else {
                output.ActionResponse.Messages.Add("Tenant '" + tenant.TenantId.ToString() + "' Not Found");
                return output;
            }
        }

        output.Name = MaxStringLength(output.Name, 200);
        output.TenantCode = MaxStringLength(output.TenantCode, 50);

        rec.Name = output.Name;
        rec.TenantCode = output.TenantCode;
        rec.Enabled = output.Enabled;

        try {
            if (newRecord) {
                await data.Tenants.AddAsync(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;
            output.ActionResponse.Messages.Add(newRecord ? "New Tenant Added" : "Tenant Updated");

            if (newRecord) {
                SeedTestData_CreateDefaultTenantData(output.TenantId);
            } else {
                SaveTenantSettings(output.TenantId, output.TenantSettings);
            }

            ClearTenantCache(output.TenantId);

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = output.TenantId,
                UpdateType = DataObjects.SignalRUpdateType.Setting,
                Message = "TenantSaved",
                Object = output
            });
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Tenant '" + output.TenantId.ToString() + "' - " + ex.Message);
        }

        return output;
    }

    public void SaveTenantSettings(Guid TenantId, DataObjects.TenantSettings settings)
    {
        if (!String.IsNullOrWhiteSpace(settings.EitSsoUrl) && !settings.EitSsoUrl.EndsWith("/")) {
            settings.EitSsoUrl += "/";
        }

        SaveSetting("Settings", DataObjects.SettingType.Object, settings, TenantId);
    }
}