namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public DataObjects.AjaxLookup AjaxUserSearch(DataObjects.AjaxLookup Lookup, bool LocalOnly = false)
    {
        // A combination of results from the local users table and from Active Directory
        List<DataObjects.AjaxResults> results = new List<DataObjects.AjaxResults>();

        // First, find local accounts (limit to 25)
        int Local = 0;
        var recs = data.Users.Where(x => x.TenantId == Lookup.TenantId && x.Enabled == true);

        string search = StringOrEmpty(Lookup.Search);

        string LastName = String.Empty;
        string FirstName = String.Empty;
        string[] Names;

        if (search.Contains(",")) {
            // Check "Last, First"
            Names = search.Split(',');
            try {
                LastName += Names[0];
                FirstName += Names[1];
            } catch { }
            LastName = LastName.Trim();
            FirstName = FirstName.Trim();

            if (!String.IsNullOrWhiteSpace(LastName) && !String.IsNullOrWhiteSpace(FirstName)) {
                recs = recs.Where(x => x.LastName != null && x.LastName.StartsWith(LastName) && x.FirstName != null && x.FirstName.StartsWith(FirstName));
            } else {
                recs = recs.Where(x => x.LastName != null && x.LastName.StartsWith(LastName));
            }
        } else if (search.Contains(" ")) {
            // Check "First Last"
            Names = search.Split(' ');
            try {
                FirstName += Names[0];
                LastName += Names[1];
            } catch { }
            LastName = LastName.Trim();
            FirstName = FirstName.Trim();

            if (!String.IsNullOrWhiteSpace(LastName) && !String.IsNullOrWhiteSpace(FirstName)) {
                recs = recs.Where(x => x.LastName != null && x.LastName.StartsWith(LastName) && x.FirstName != null && x.FirstName.StartsWith(FirstName));
            } else {
                recs = recs.Where(x => x.FirstName != null && x.FirstName.StartsWith(FirstName));
            }
        } else {
            recs = recs.Where(
                x => (x.FirstName != null && x.FirstName.Contains(search)) ||
                (x.LastName != null && x.LastName.Contains(search)) ||
                (x.Email != null && x.Email.Contains(search))
                );
        }

        recs = recs.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);

        if (recs != null && recs.Count() > 0) {
            results.Add(new DataObjects.AjaxResults {
                value = Guid.Empty.ToString(),
                label = "--- Local Help Desk Users ---"
            });
            foreach (var rec in recs) {
                Local += 1;
                if (Local < 26) {
                    string deptName = rec.Department != null && !String.IsNullOrEmpty(rec.Department.DepartmentName) ? rec.Department.DepartmentName : String.Empty;
                    string location = rec.Location != null ? rec.Location : String.Empty;

                    results.Add(new DataObjects.AjaxResults {
                        value = rec.UserId.ToString(),
                        label = DisplayNameFromLastAndFirst(rec.LastName, rec.FirstName, rec.Email, deptName, location) +
                            (!String.IsNullOrWhiteSpace(rec.Email) ? " (" + rec.Email + ")" : ""),
                        email = !String.IsNullOrWhiteSpace(rec.Email) ? rec.Email : "",
                        username = rec.Username,
                        extra1 = rec.Phone,
                        extra2 = rec.Location,
                        extra3 = rec.DepartmentId.ToString()
                    });
                }
            }
        }

        // Next, see if there are any ad results to add
        int AD = 0;

        if (_localDb) {
            LocalOnly = true;
        }

        if (!LocalOnly) {
            var ldapRoot = GetSetting<string>("activedirectoryroot", DataObjects.SettingType.Text);

            if (!String.IsNullOrWhiteSpace(ldapRoot)) {
                var ldapQueryUsername = GetSetting<string>("LdapUsername", DataObjects.SettingType.EncryptedText);
                var ldapQueryPassword = GetSetting<string>("LdapPassword", DataObjects.SettingType.EncryptedText);

                if (String.IsNullOrWhiteSpace(ldapQueryUsername) || String.IsNullOrWhiteSpace(ldapQueryPassword)) {
                    ldapQueryUsername = "";
                    ldapQueryPassword = "";
                }

                List<string> excludeEmails = new List<string>();
                if (results.Count() > 0) {
                    foreach (var item in results.Where(x => !String.IsNullOrWhiteSpace(x.email))) {
                        if (!String.IsNullOrEmpty(item.email)) {
                            excludeEmails.Add(item.email.ToLower());
                        }
                    }
                }

                string ldapOptionalLocationAttribute = GetLdapOptionalLocationAttribute();
                List<DataObjects.ActiveDirectorySearchResults>? adResults = GetActiveDirectorySearchResults(StringOrEmpty(Lookup.Search), 25, excludeEmails, ldapRoot, ldapQueryUsername, ldapQueryPassword, ldapOptionalLocationAttribute);
                if (adResults != null && adResults.Count() > 0) {
                    results.Add(new DataObjects.AjaxResults {
                        value = Guid.Empty.ToString(),
                        label = "--- Active Directory Results ---"
                    });

                    foreach (var adUser in adResults) {
                        // Only add if we don't already have this user
                        var match = results.FirstOrDefault(x => x.email == adUser.Email);
                        if (match == null) {
                            AD += 1;
                            if (AD < 26) {
                                results.Add(new DataObjects.AjaxResults {
                                    value = adUser.UserId.HasValue ? ((Guid)adUser.UserId).ToString() : Guid.Empty.ToString(),
                                    label = DisplayNameFromLastAndFirst(adUser.LastName, adUser.FirstName, adUser.Email, adUser.Department, adUser.Location) + " [AD]" +
                                        (!String.IsNullOrWhiteSpace(adUser.Email) ? " (" + adUser.Email + ")" : ""),
                                    email = adUser.Email
                                });
                            }
                        }
                    }
                    //results = results.OrderBy(x => x.label).ToList();
                }
            }
        }

        if (Local > 25 || AD > 25) {
            // We limited the results
            results.Insert(0, new DataObjects.AjaxResults {
                value = Guid.Empty.ToString(),
                label = "--- Too many results found, please narrow your search ---"
            });
        }

        Lookup.Results = results;
        return Lookup;
    }
}