using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace SampleSinglePageApplication;

/// <summary>
/// Client used to look up user information by WSUID or NID
/// </summary>
public class UserLookupClient
{
    private string _endpoint;
    private string _username;
    private string _password;

    /// <summary>
    /// Creates a new instance of the user lookup client.
    /// </summary>
    /// <param name="endpoint">The endpoint to the RESTful web service.</param>
    /// <param name="username">The username for accessing the web service.</param>
    /// <param name="password">The password for accessing the web service.</param>
    public UserLookupClient(string endpoint, string username, string password)
    {
        if (!String.IsNullOrWhiteSpace(endpoint)) {
            _endpoint = endpoint;
            if (!_endpoint.EndsWith("/")) {
                _endpoint += "/";
            }
        } else {
            throw new NullReferenceException("Endpoint is Required");
        }

        if (!String.IsNullOrWhiteSpace(username)) {
            _username = username;
        } else {
            throw new NullReferenceException("Username is Required");
        }

        if (!String.IsNullOrWhiteSpace(password)) {
            _password = password;
        } else {
            throw new NullReferenceException("Password is Required");
        }
    }

    /// <summary>
    /// Looks up user information by NID.
    /// </summary>
    /// <param name="NID">The NID of the account to look up.</param>
    /// <returns>A UserLookupResponse object.</returns>
    public UserLookupResponse? UserLookupByNID(string NID)
    {
        if (String.IsNullOrWhiteSpace(NID)) {
            throw new NullReferenceException("UserLookupByNID Requires a Valid NID to be Passed");
        }
        return PerformLookup(_endpoint + "UserInfoLookupByNID?NID=" + UrlEncode(NID));
    }

    /// <summary>
    /// Looks up user information by WSUID.
    /// </summary>
    /// <param name="WSUID">The WSUID of the account to look up.</param>
    /// <returns>A UserLookupResponse object.</returns>
    public UserLookupResponse? UserLookupByWSUID(int WSUID)
    {
        if (WSUID < 1) {
            throw new NullReferenceException("UserLookupByWSUID Requires a Valid WSUID");
        }
        return PerformLookup(_endpoint + "UserInfoLookupByWsuId?WSUID=" + UrlEncode(WSUID.ToString()));
    }

    private UserLookupResponse? PerformLookup(string url)
    {
        UserLookupResponse? output = null;

        var client = Utilities.GetHttpClient(url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("ApplicationUsername", _username);
        client.DefaultRequestHeaders.Add("ApplicationPassword", _password);

        System.Net.Http.HttpResponseMessage response;

        try {
            response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode) {
                var content = response.Content.ReadAsStringAsync().Result;
                if (!String.IsNullOrWhiteSpace(content)) {
                    output = JsonConvert.DeserializeObject<UserLookupResponse>(content);
                }
            }
        } catch { }

        return output;
    }

    private string UrlEncode(string text)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(text)) {
            output = System.Net.WebUtility.UrlEncode(text);
        }

        return output;
    }

    /// <summary>
    /// The user lookup response.
    /// </summary>
    public class UserLookupResponse
    {
        /// <summary>
        /// WSUID
        /// </summary>
        public int WsuId { get; set; }
        /// <summary>
        /// External System ID
        /// </summary>
        public string ExtSysId { get; set; } = "";
        /// <summary>
        /// Network ID
        /// </summary>
        public string NID { get; set; } = "";
        /// <summary>
        /// Last Name
        /// </summary>
        public string LastName { get; set; } = "";
        /// <summary>
        /// Middle Name
        /// </summary>
        public string MiddleName { get; set; } = "";
        /// <summary>
        /// First Name
        /// </summary>
        public string FirstName { get; set; } = "";
        /// <summary>
        /// Name Suffix
        /// </summary>
        public string Suffix { get; set; } = "";
        /// <summary>
        /// Display Namein First Last format
        /// </summary>
        public string DisplayName { get; set; } = "";
        /// <summary>
        /// Display Name in Last, First Middle format
        /// </summary>
        public string DisplayNameLFM { get; set; } = "";
        /// <summary>
        /// Birth Date
        /// </summary>
        public DateTime? BirthDate { get; set; }
        /// <summary>
        /// Birth Date as String
        /// </summary>
        public string BirthDateString { get; set; } = "";
        /// <summary>
        /// Sex
        /// </summary>
        public string Sex { get; set; } = "";
        /// <summary>
        /// Mail Country
        /// </summary>
        public string MailCountry { get; set; } = "";
        /// <summary>
        /// Mail Country Name
        /// </summary>
        public string MailCountryName { get; set; } = "";
        /// <summary>
        /// Address Line 1
        /// </summary>
        public string MailAddress1 { get; set; } = "";
        /// <summary>
        /// Address Line 2
        /// </summary>
        public string MailAddress2 { get; set; } = "";
        /// <summary>
        /// Address Line 3
        /// </summary>
        public string MailAddress3 { get; set; } = "";
        /// <summary>
        /// Address Line 4
        /// </summary>
        public string MailAddress4 { get; set; } = "";
        /// <summary>
        /// Mail City
        /// </summary>
        public string MailCity { get; set; } = "";
        /// <summary>
        /// Mail State
        /// </summary>
        public string MailState { get; set; } = "";
        /// <summary>
        /// Mail Postal Code
        /// </summary>
        public string MailPostalCode { get; set; } = "";
        /// <summary>
        /// Local Phone
        /// </summary>
        public string LocalPhone { get; set; } = "";
        /// <summary>
        /// Home Country
        /// </summary>
        public string HomeCountry { get; set; } = "";
        /// <summary>
        /// Home Country Name
        /// </summary>
        public string HomeCountryName { get; set; } = "";
        /// <summary>
        /// Home Address Line 1
        /// </summary>
        public string HomeAddress1 { get; set; } = "";
        /// <summary>
        /// Home Address Line 2
        /// </summary>
        public string HomeAddress2 { get; set; } = "";
        /// <summary>
        /// Home Address Line 3
        /// </summary>
        public string HomeAddress3 { get; set; } = "";
        /// <summary>
        /// Home Address Line 4
        /// </summary>
        public string HomeAddress4 { get; set; } = "";
        /// <summary>
        /// Home City
        /// </summary>
        public string HomeCity { get; set; } = "";
        /// <summary>
        /// Home State
        /// </summary>
        public string HomeState { get; set; } = "";
        /// <summary>
        /// Home Postal Code
        /// </summary>
        public string HomePostalCode { get; set; } = "";
        /// <summary>
        /// Campus Address Line 1
        /// </summary>
        public string CampusAddress1 { get; set; } = "";
        /// <summary>
        /// Campus Address Line 2
        /// </summary>
        public string CampusAddress2 { get; set; } = "";
        /// <summary>
        /// Campus City
        /// </summary>
        public string CampusCity { get; set; } = "";
        /// <summary>
        /// Campus State
        /// </summary>
        public string CampusState { get; set; } = "";
        /// <summary>
        /// Campus Postal Code
        /// </summary>
        public string CampusPostalCode { get; set; } = "";
        /// <summary>
        /// Home Phone
        /// </summary>
        public string HomePhone { get; set; } = "";
        /// <summary>
        /// Mobile Phone
        /// </summary>
        public string MobilePhone { get; set; } = "";
        /// <summary>
        /// Preferred Phone
        /// </summary>
        public string PreferredPhone { get; set; } = "";
        /// <summary>
        /// Office Email
        /// </summary>
        public string OfficeEmail { get; set; } = "";
        /// <summary>
        /// Personal Email
        /// </summary>
        public string PersonalEmail { get; set; } = "";
        /// <summary>
        /// Preferred Email
        /// </summary>
        public string PreferredEmail { get; set; } = "";
        /// <summary>
        /// FERPA
        /// </summary>
        public bool FERPA { get; set; }
        /// <summary>
        /// Visa Type
        /// </summary>
        public string VisaType { get; set; } = "";
        /// <summary>
        /// Visa Status
        /// </summary>
        public string VisaStatus { get; set; } = "";
        /// <summary>
        /// Visa Status Description
        /// </summary>
        public string VisaStatusDescription { get; set; } = "";
        /// <summary>
        /// Citizenship Country
        /// </summary>
        public string CitizenshipCountry { get; set; } = "";
        /// <summary>
        /// Citizenship Status
        /// </summary>
        public string CitizenshipStatus { get; set; } = "";
        /// <summary>
        /// Deceased Individual
        /// </summary>
        public bool DeceasedIndividual { get; set; }
        /// <summary>
        /// Date of Death
        /// </summary>
        public DateTime? DateOfDeath { get; set; }
        /// <summary>
        /// Date of Death String
        /// </summary>
        public string DateOfDeathString { get; set; } = "";
        /// <summary>
        /// Return Code
        /// </summary>
        public int ReturnCode { get; set; }
        /// <summary>
        /// Return Message
        /// </summary>
        public string ReturnMessage { get; set; } = "";
    }
}