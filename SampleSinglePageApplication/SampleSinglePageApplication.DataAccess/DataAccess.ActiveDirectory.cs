namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfo(Guid UserId, string ldapRoot, string ldapQueryUsername = "", string ldapQueryPassword = "", string ldapLocationAttribute = "")
    {
        var output = GetActiveDirectoryInfo(UserId.ToString(), DataObjects.UserLookupType.Guid, ldapRoot, ldapQueryUsername, ldapQueryPassword, ldapLocationAttribute);
        return output;
    }

    public DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfo(string Lookup, DataObjects.UserLookupType Type, string ldapRoot, string ldapQueryUsername = "", string ldapQueryPassword = "", string ldapLocationAttribute = "")
    {
#pragma warning disable CA1416 // Validate platform compatibility
        if (String.IsNullOrWhiteSpace(ldapLocationAttribute)) {
            ldapLocationAttribute = "physicalDeliveryOfficeName";
        }

        DataObjects.ActiveDirectoryUserInfo? output = null;
        System.DirectoryServices.DirectoryEntry? entry = null;
        if (String.IsNullOrWhiteSpace(ldapQueryUsername) || String.IsNullOrWhiteSpace(ldapQueryPassword)) {
            entry = new System.DirectoryServices.DirectoryEntry("LDAP://" + ldapRoot);
        } else {
            entry = new System.DirectoryServices.DirectoryEntry("LDAP://" + ldapRoot, ldapQueryUsername, ldapQueryPassword);
        }

        string searchFilter = String.Empty;
        switch (Type) {
            case DataObjects.UserLookupType.Email:
                searchFilter = "mail=" + Lookup;
                break;

            case DataObjects.UserLookupType.EmployeeId:
                searchFilter = "(&(objectClass=user)(objectCategory=person)(employeeid=" + Lookup + "))";
                break;

            case DataObjects.UserLookupType.Guid:
                searchFilter = "objectGUID=" + Utilities.GetBinaryStringFromGuid(new Guid(Lookup));
                break;

            case DataObjects.UserLookupType.Username:
                searchFilter = "SAMAccountName=" + Lookup;
                break;
        }

        if (entry != null) {
            try {
                object obj = entry.NativeObject;
                System.DirectoryServices.DirectorySearcher search = new System.DirectoryServices.DirectorySearcher(entry);
                search.Filter = searchFilter;
                search.PropertiesToLoad.Add("givenName");
                search.PropertiesToLoad.Add("sn");
                search.PropertiesToLoad.Add("mail");
                search.PropertiesToLoad.Add("department");
                search.PropertiesToLoad.Add("SAMAccountName");
                search.PropertiesToLoad.Add("telephoneNumber");
                search.PropertiesToLoad.Add("employeeID");
                search.PropertiesToLoad.Add("title");
                search.PropertiesToLoad.Add(ldapLocationAttribute);
                search.PropertiesToLoad.Add("objectGUID");
                var result = search.FindOne();
                if (result != null) {
                    output = new DataObjects.ActiveDirectoryUserInfo();
                    output.UserId = Guid.Empty;

                    try { output.Department = result.Properties["Department"][0].ToString(); } catch { }
                    try { output.Username = result.Properties["SAMAccountName"][0].ToString(); } catch { }
                    try { output.FirstName = result.Properties["givenName"][0].ToString(); } catch { }
                    try { output.LastName = result.Properties["sn"][0].ToString(); } catch { }
                    try { output.Email = result.Properties["mail"][0].ToString(); } catch { }
                    try { output.Phone = result.Properties["telephoneNumber"][0].ToString(); } catch { }
                    try { output.EmployeeId = result.Properties["employeeID"][0].ToString(); } catch { }
                    try { output.Title = result.Properties["title"][0].ToString(); } catch { }
                    try { output.Location = result.Properties[ldapLocationAttribute][0].ToString(); } catch { }
                    try { output.UserId = new Guid((byte[])result.Properties["objectGUID"][0]); } catch { }
                }
            } catch { }
        }

#pragma warning restore CA1416 // Validate platform compatibility
        return output;
    }

    public DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfo(string Username, string ldapRoot, string ldapQueryUsername = "", string ldapQueryPassword = "", string ldapLocationAttribute = "")
    {
        var output = GetActiveDirectoryInfo(Username, DataObjects.UserLookupType.Username, ldapRoot, ldapQueryUsername, ldapQueryPassword, ldapLocationAttribute);
        return output;
    }

    public DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfoFromEmailAddress(string EmailAddress, string ldapRoot, string ldapQueryUsername = "", string ldapQueryPassword = "", string ldapLocationAttribute = "")
    {
        var output = GetActiveDirectoryInfo(EmailAddress, DataObjects.UserLookupType.Email, ldapRoot, ldapQueryUsername, ldapQueryPassword, ldapLocationAttribute);
        return output;
    }

    public DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfoFromEmployeeId(string EmployeeId, string ldapRoot, string ldapQueryUsername = "", string ldapQueryPassword = "", string ldapLocationAttribute = "", string bioDemoUsername = "", string bioDemoPassword = "", string bioDemoUrl = "")
    {
        DataObjects.ActiveDirectoryUserInfo? output = null;

        // Try the BioDemo client before going to Active Directory.
        if (!String.IsNullOrWhiteSpace(bioDemoUsername) && !String.IsNullOrWhiteSpace(bioDemoPassword) && !String.IsNullOrWhiteSpace(bioDemoUrl)) {
            BioDemoClient client = new BioDemoClient(bioDemoUsername, bioDemoPassword, bioDemoUrl);

            int wsuId = 0;
            try {
                wsuId = Convert.ToInt32(EmployeeId);
            } catch {
                wsuId = 0;
            }
            if (wsuId > 0) {
                var bioDemoUser = client.GetBioDemoByWsuId(wsuId);
                if (bioDemoUser != null && bioDemoUser.WsuId > 0) {
                    // A user was returned.
                    output = new DataObjects.ActiveDirectoryUserInfo {
                        Email = !String.IsNullOrWhiteSpace(bioDemoUser.OfficeEmail) ? bioDemoUser.OfficeEmail : bioDemoUser.PreferredEmail,
                        EmployeeId = bioDemoUser.WsuId.ToString().PadLeft(9, '0'),
                        FirstName = bioDemoUser.FirstName,
                        LastName = bioDemoUser.LastName,
                        Phone = !String.IsNullOrWhiteSpace(bioDemoUser.PreferredPhone) ? bioDemoUser.PreferredPhone : "",
                        Username = bioDemoUser.NID
                    };
                    return output;
                }
            }
        }

        output = GetActiveDirectoryInfo(EmployeeId, DataObjects.UserLookupType.EmployeeId, ldapRoot, ldapQueryUsername, ldapQueryPassword, ldapLocationAttribute);
        return output;
    }

    public List<DataObjects.ActiveDirectorySearchResults>? GetActiveDirectorySearchResults(string SearchText, int MaxResults, List<string> excludeEmails, string ldapRoot, string ldapQueryUsername = "", string ldapQueryPassword = "", string ldapLocationAttribute = "")
    {
#pragma warning disable CA1416 // Validate platform compatibility
        if (String.IsNullOrWhiteSpace(ldapLocationAttribute)) {
            ldapLocationAttribute = "physicalDeliveryOfficeName";
        }

        List<DataObjects.ActiveDirectorySearchResults>? output = null;
        System.DirectoryServices.DirectoryEntry? entry = null;
        if (String.IsNullOrWhiteSpace(ldapQueryUsername) || String.IsNullOrWhiteSpace(ldapQueryPassword)) {
            entry = new System.DirectoryServices.DirectoryEntry("LDAP://" + ldapRoot);
        } else {
            entry = new System.DirectoryServices.DirectoryEntry("LDAP://" + ldapRoot, ldapQueryUsername, ldapQueryPassword);
        }
        try {
            object obj = entry.NativeObject;
            System.DirectoryServices.DirectorySearcher search = new System.DirectoryServices.DirectorySearcher(entry);
            search.Filter = "(&(objectClass=user)(objectCategory=person)(anr=" + SearchText + "))";
            search.PropertiesToLoad.Add("givenName");
            search.PropertiesToLoad.Add("sn");
            search.PropertiesToLoad.Add("mail");
            search.PropertiesToLoad.Add("objectGUID");
            search.PropertiesToLoad.Add("Department");
            search.PropertiesToLoad.Add(ldapLocationAttribute);
            var results = search.FindAll();
            if (results != null) {
                output = new List<DataObjects.ActiveDirectorySearchResults>();
                foreach (System.DirectoryServices.SearchResult result in results) {
                    if (output.Count() <= MaxResults) {
                        var u = new DataObjects.ActiveDirectorySearchResults();

                        try { u.LastName = result.Properties["sn"][0].ToString(); } catch { }
                        try { u.FirstName = result.Properties["givenName"][0].ToString(); } catch { }
                        try { u.Email = result.Properties["mail"][0].ToString(); } catch { }

                        if (!String.IsNullOrWhiteSpace(u.LastName) && !String.IsNullOrWhiteSpace(u.FirstName) && !String.IsNullOrWhiteSpace(u.Email) && !excludeEmails.Contains(u.Email.ToLower())) {
                            try { u.Department = result.Properties["Department"][0].ToString(); } catch { }
                            try { u.Location = result.Properties[ldapLocationAttribute][0].ToString(); } catch { }
                            try { u.UserId = new Guid((byte[])result.Properties["objectGUID"][0]); } catch { }
                            output.Add(u);
                        }
                    }
                }
                if (output.Count() > 0) {
                    output = output.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
                }
            }
        } catch (Exception ex) {
            if (!String.IsNullOrWhiteSpace(ex.Message)) {

            }
        }
#pragma warning restore CA1416 // Validate platform compatibility
        return output;
    }

    public string GetLdapOptionalLocationAttribute()
    {
        string output = _ldapOptionalLocationAttribute;

        if (String.IsNullOrWhiteSpace(output)) {
            var setting = GetSetting<string>("ldapLocationAttribute", DataObjects.SettingType.Text);
            if (!String.IsNullOrEmpty(setting)) {
                output = setting;
            }

            if (String.IsNullOrWhiteSpace(output)) {
                output = "physicalDeliveryOfficeName";
            }

            _ldapOptionalLocationAttribute = output;
        }

        return output;
    }


}