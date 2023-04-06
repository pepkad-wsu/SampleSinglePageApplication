namespace SampleSinglePageApplication;
public partial class DataAccess
{
    /// <summary>
    /// Appends text to a string, adding a comma before the new text if the string already has a value.
    /// </summary>
    /// <param name="Original">The original string.</param>
    /// <param name="New">The new value to add to the string.</param>
    /// <returns></returns>
    public string AppendWithComma(string Original, string New)
    {
        string output = Original;
        if (!String.IsNullOrWhiteSpace(Original)) {
            output += ", ";
        }
        output += New;
        return output;
    }

    public string ApplicationURL { 
        get {
            string output = StringValue(CacheStore.GetCachedItem<string>(Guid.Empty, "ApplicationURL"));
            if (String.IsNullOrWhiteSpace(output)) {
                output += GetSetting<string>("ApplicationURL", DataObjects.SettingType.Text);
                CacheStore.SetCacheItem(Guid.Empty, "ApplicationURL", output);
            }
            return output;
        }
    }

    public string AppName {
        get {
            return _appName;
        }
    }

    public bool BooleanValue(bool? value)
    {
        bool output = value.HasValue ? (bool)value : false;
        return output;
    }

    public string CleanHtml(string html)
    {
        string output = html;
        if (!String.IsNullOrWhiteSpace(output)) {
            // First, if there are body tags only get the text in between the start and end tag.
            int BodyStart = output.ToLower().IndexOf("<body");
            int BodyEnd = output.ToLower().IndexOf("</body");
            if (BodyStart > -1 && BodyEnd > -1) {
                BodyStart = output.IndexOf(">", BodyStart + 1);
                if (BodyStart > -1) {
                    output = output.Substring(BodyStart + 1, (BodyEnd - BodyStart - 1));
                }
            }

            // Next, remove any style tags
            int Safety = 0;
            int StyleStart = output.ToLower().IndexOf("<style");
            while (StyleStart > -1) {
                Safety++;
                if (Safety > 100) {
                    return output;
                }
                int StyleEnd = output.ToLower().IndexOf("</style");
                if (StyleStart > -1 && StyleEnd > -1) {
                    StyleEnd = output.IndexOf(">", StyleEnd + 1);
                    if (StyleEnd > -1) {
                        string style = output.Substring(StyleStart, (StyleEnd - StyleStart + 1));
                        output = output.Replace(style, "");
                    }
                }
                StyleStart = output.ToLower().IndexOf("<style>");
            }
        }
        return output;
    }

    private List<string> ConcatenateErrorMessages(DataObjects.User ReportedBy,
        DataObjects.User AffectedUser, List<DataObjects.User> AdditionalAffectedUsers)
    {
        List<string> output = new List<string>();
        if (!ReportedBy.ActionResponse.Result) {
            if (ReportedBy.ActionResponse.Messages != null && ReportedBy.ActionResponse.Messages.Count() > 0) {
                foreach (var msg in ReportedBy.ActionResponse.Messages) {
                    output.Add(msg);
                }
            }
        }

        if (!AffectedUser.ActionResponse.Result) {
            if (AffectedUser.ActionResponse.Messages != null && AffectedUser.ActionResponse.Messages.Count() > 0) {
                foreach (var msg in AffectedUser.ActionResponse.Messages) {
                    output.Add(msg);
                }
            }
        }

        if (AdditionalAffectedUsers != null && AdditionalAffectedUsers.Count() > 0) {
            foreach (var user in AdditionalAffectedUsers) {
                if (!user.ActionResponse.Result) {
                    if (user.ActionResponse.Messages != null && user.ActionResponse.Messages.Count() > 0) {
                        foreach (var msg in user.ActionResponse.Messages) {
                            output.Add(msg);
                        }
                    }
                }
            }
        }
        return output;
    }

    public string ConnectionString(bool full = false)
    {
        string output = ConnectionStringReport(_connectionString);
        if (full) {
            output = _connectionString;
        }
        return output;
    }

    public string ConnectionStringReport(string input)
    {
        string output = input;

        if (!String.IsNullOrWhiteSpace(output)) {
            List<string> elements = output.Split(';').ToList();
            if (elements != null && elements.Count() > 0) {
                string report = "";
                foreach (var element in elements) {
                    List<string> items = element.Split('=').ToList();
                    if (items != null && items.Count() > 0) {
                        switch (items[0].ToUpper()) {
                            case "DATABASE":
                            case "DATA SOURCE":
                            case "INITIAL CATALOG":
                            case "MULTIPLEACTIVERESULTSETS":
                            case "PROVIDER":
                            case "SERVER":
                            case "USER ID":
                                report += element + ";";
                                break;

                            case "PASSWORD":
                                report += "Password=***;";
                                break;
                        }
                    }
                    output = report;
                }
            } else {
                int loc = output.ToLower().IndexOf("initial catalog=");
                if (loc > -1) {
                    int locEnd = output.IndexOf(";", loc + 1);
                    if (locEnd > -1) {
                        output = output.Substring(0, locEnd);
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
	/// Reads a cookie value.
	/// </summary>
	/// <param name="cookieName">The name of the cookie.</param>
	/// <returns>The cookie value as a string.</returns>
	public string CookieRead(string cookieName)
    {
        string output = String.Empty;

        if (_httpContext != null) {
            if (!String.IsNullOrWhiteSpace(cookieName)) {
                try {
                    string ck = _httpContext.Request.Cookies[cookieName];
                    if (!String.IsNullOrWhiteSpace(ck)) {
                        output = ck;
                    }
                } catch (Exception ex) {
                    if (ex != null) { }
                }
            }
            if (output.ToLower() == "cleared") { output = String.Empty; }
        }

        return System.Web.HttpUtility.HtmlDecode(output);
    }

    /// <summary>
    /// Writes a cookie.
    /// </summary>
    /// <param name="cookieName">The name of the cookie.</param>
    /// <param name="value">The value for the cookie.</param>
    /// <param name="cookieDomain">An optional domain to set for the cookie. Never used when running on localhost.</param>
    public void CookieWrite(string cookieName, string value, string cookieDomain = "")
    {
        if (_httpContext != null) {
            DateTime now = DateTime.UtcNow;
            if (String.IsNullOrEmpty(cookieName)) { return; }

            Microsoft.AspNetCore.Http.CookieOptions option = new Microsoft.AspNetCore.Http.CookieOptions();
            option.Expires = now.AddYears(1);

            string fullUrl = GetFullUrl();

            if (!String.IsNullOrWhiteSpace(cookieDomain) && !String.IsNullOrWhiteSpace(fullUrl) && !fullUrl.ToLower().Contains("localhost")) {
                option.Domain = cookieDomain;
            }

            _httpContext.Response.Cookies.Append(cookieName, value, option);
        }
    }

    public string Copyright {
        get {
            return _copyright;
        }
    }

    public string DatabaseType {
        get {
            return _databaseType;
        }
    }

    private string DefaultReplyToAddress {
        get {
            string output = String.Empty;
            if(CacheStore.ContainsKey(Guid.Empty, "DefaultReplyToAddress")) {
                output += CacheStore.GetCachedItem<string>(Guid.Empty, "DefaultReplyToAddress");
            } else {
                output += GetSetting<string>("DefaultReplyToAddress", DataObjects.SettingType.Text);
                CacheStore.SetCacheItem(Guid.Empty, "DefaultReplyToAddress", output);
            }
            return output;
        }
    }

    public T? DeserializeObject<T>(string? SerializedObject)
    {
        var output = default(T);

        if (!String.IsNullOrWhiteSpace(SerializedObject)) {
            try {
                var d = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(SerializedObject);
                if (d != null) {
                    output = d;
                }
            } catch { }
        }

        return output;
    }

    public T? DeserializeObjectFromXmlOrJson<T>(string? SerializedObject)
    {
        var output = default(T);

        if (!String.IsNullOrWhiteSpace(SerializedObject)) {
            if (SerializedObject.StartsWith("<") || SerializedObject.ToLower().Contains("xmlns:")) {
                var deserializedXML = Serialize_XmlToObject<T>(SerializedObject);
                if (deserializedXML != null) {
                    output = deserializedXML;
                }
            } else {
                var deserializedJson = DeserializeObject<T>(SerializedObject);
                if (deserializedJson != null) {
                    output = deserializedJson;
                }
            }
        }

        return output;
    }

    private List<DataObjects.Dictionary>? DictionaryToListOfDictionary(Dictionary<string, string>? dict)
    {
        List<DataObjects.Dictionary>? output = null;

        if (dict != null && dict.Any()) {
            output = new List<DataObjects.Dictionary>();
            foreach (var item in dict) {
                output.Add(new DataObjects.Dictionary {
                    Key = item.Key,
                    Value = item.Value
                });
            }
        }

        return output;
    }

    private static T? DuplicateObject<T>(object? o)
    {
        T? output = default(T);

        if (o != null) {
            // To make a new copy serialize the object and then deserialize it back to a new object.
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(o);
            if (!String.IsNullOrEmpty(serialized)) {
                try {
                    var duplicate = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(serialized);
                    if (duplicate != null) {
                        output = duplicate;
                    }
                } catch { }
            }
        }

        return output;
    }

    /// <summary>
    /// Generates a random numeric code
    /// </summary>
    /// <param name="Length">The length of the code</param>
    /// <returns>A string with a random code</returns>
    public string GenerateRandomCode(int Length)
    {
        string output = String.Empty;
        char[] Possibilities = "1234567890".ToCharArray();
        Random Randomizer = new Random();

        while (Length > 0) {
            output += Possibilities[Randomizer.Next(0, Possibilities.Length)];
            Length--;
        }

        return output;
    }

    public string GetFullUrl()
    {
        string output = "";

        if (_httpContext != null) {
            try {
                output = string.Concat(
                    _httpContext.Request.Scheme,
                    "://",
                    _httpContext.Request.Host.ToUriComponent(),
                    _httpContext.Request.PathBase.ToUriComponent(),
                    _httpContext.Request.Path.ToUriComponent(),
                    _httpContext.Request.QueryString.ToUriComponent()
                );
            } catch { }
        }

        return output;
    }

    public string GetFullUrlWithoutQuerystring()
    {
        string output = "";

        if (_httpContext != null) {
            try {
                output = string.Concat(
                    _httpContext.Request.Scheme,
                    "://",
                    _httpContext.Request.Host.ToUriComponent(),
                    _httpContext.Request.PathBase.ToUriComponent(),
                    _httpContext.Request.Path.ToUriComponent()
                );
            } catch { }
        }

        return output;
    }

    public DataObjects.BooleanResponse GetNewActionResponse(bool result = false, string? message = null)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse {
            Result = result,
            Messages = new List<string>()
        };
        if (!String.IsNullOrEmpty(message)) {
            output.Messages.Add(message);
        }
        return output;
    }

    public Guid GuidValue(Guid? guid)
    {
        return guid.HasValue ? (Guid)guid : Guid.Empty;
    }

    public string HtmlToPlainText(string html)
    {
        string output = Regex.Replace(html, @"<(.|\n)*?>", "");
        output = System.Web.HttpUtility.HtmlDecode(output);
        return output;
    }

    public int IntValue(int? value)
    {
        int output = value.HasValue ? (int)value : 0;
        return output;
    }

    private string MailServer {
        get {
            string output = String.Empty;
            if (CacheStore.ContainsKey(Guid.Empty, "MailServer")) {
                output += CacheStore.GetCachedItem<string>(Guid.Empty, "MailServer");
            } else {
                output += GetSetting<string>("MailServer", DataObjects.SettingType.Text);
                CacheStore.SetCacheItem(Guid.Empty, "MailServer", output);
            }
            return output;
        }
    }

    private string MailServerPassword {
        get {
            string output = String.Empty;
            if (CacheStore.ContainsKey(Guid.Empty, "MailServerPassword")) {
                output += CacheStore.GetCachedItem<string>(Guid.Empty, "MailServerPassword");
            } else {
                output += GetSetting<string>("MailServerPassword", DataObjects.SettingType.Text);
                CacheStore.SetCacheItem(Guid.Empty, "MailServerPassword", output);
            }
            return output;
        }
    }

    private int MailServerPort {
        get {
            int output = 0;
            if(CacheStore.ContainsKey(Guid.Empty, "MailServerPort")) {
                output = CacheStore.GetCachedItem<int>(Guid.Empty, "MailServerPort");
            } else {
                output = GetSetting<int>("MailServerPort", DataObjects.SettingType.NumberInt);
                if (output < 1) {
                    output = 25;
                }
                CacheStore.SetCacheItem(Guid.Empty, "MailServerPort", output);
            }
            return output;
        }
    }

    private string MailServerUsername {
        get {
            string output = String.Empty;
            if(CacheStore.ContainsKey(Guid.Empty, "MailServerUsername")) {
                output += CacheStore.GetCachedItem<string>(Guid.Empty, "MailServerUsername");
            } else {
                output += GetSetting<string>("MailServerUsername", DataObjects.SettingType.Text);
                CacheStore.SetCacheItem(Guid.Empty, "MailServerUsername", output);
            }
            return output;
        }
    }

    private bool MailServerUsesSSL {
        get {
            bool output = false;
            if(CacheStore.ContainsKey(Guid.Empty, "MailServerUsesSSL")) {
                output = CacheStore.GetCachedItem<bool>(Guid.Empty, "MailServerUsesSSL");
            } else {
                output = GetSetting<bool>("MailServerUseSSL", DataObjects.SettingType.Boolean);
                CacheStore.SetCacheItem(Guid.Empty, "MailServerUsesSSL", output);
            }
            return output;
        }
    }

    private string MaxStringLength(string? value, int maxLength)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(value)) {
            output = value;
            if (output.Length > maxLength) {
                output = output.Substring(0, maxLength);
            }
        }

        return output;
    }

    public double NowFromUnixEpoch()
    {
        return (double)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public List<string> MessageToListOfString(string message)
    {
        return new List<string> { message };
    }

    private string OptionPairValue(List<DataObjects.OptionPair>? Options, string Id)
    {
        string output = String.Empty;
        if (Options != null && Options.Count() > 0) {
            var opt = Options.FirstOrDefault(x => x.Id != null && x.Id.ToLower() == Id.ToLower());
            if (opt != null) {
                output += opt.Value;
            }
        }
        return output;
    }

    public string QueryStringValue(string valueName)
    {
        string output = String.Empty;

        if (_httpContext != null) {
            try {
                output += _httpContext.Request.Query[valueName].ToString();
            } catch { }
        }

        return output;
    }

    public void Redirect(string url)
    {
        if (_httpContext != null) {
            _httpContext.Response.Redirect(url);
        }
    }

    public DateTime Released {
        get {
            return _released;
        }
    }

    public string Replace(string input, string replaceText, string withText)
    {
        string output = input;

        if (!String.IsNullOrWhiteSpace(output) && !String.IsNullOrWhiteSpace(replaceText)) {
            if (String.IsNullOrWhiteSpace(withText)) {
                withText = "";
            }

            output = Regex.Replace(
                input,
                Regex.Escape(replaceText),
                withText.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );
        }

        return output;
    }

    public string Request(string parameter)
    {
        string output = "";

        if (_httpContext != null) {
            // First, try the querystring.
            output = QueryStringValue(parameter);

            if (String.IsNullOrWhiteSpace(output)) {
                // Check form fields
                try {
                    output += _httpContext.Request.Form[parameter].ToString();
                } catch { }
            }
        }

        return output;
    }

    public double RunningSince {
        get {
            return GlobalSettings.RunningSince;
        }
    }

    private DataObjects.BooleanResponse SaveFile(string FullPath, string Contents)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        bool exists = System.IO.File.Exists(FullPath);
        try {
            System.IO.File.WriteAllText(FullPath, Contents);
            output.Result = true;

            if (exists) {
                output.Messages.Add("File Updated");
            } else {
                output.Messages.Add("File Created");
            }
        } catch (Exception ex) {
            output.Messages.Add("Error Saving File '" + FullPath + "' - " + ex.Message);
        }

        return output;
    }

    public DataObjects.BooleanResponse SendEmail(DataObjects.EmailMessage message, DataObjects.MailServerConfig? config = null)
    {
        if (config == null) {
            config = GetMailServerConfig();
        }

        if (!String.IsNullOrWhiteSpace(config.Username)) {
            config.Username = Decrypt(config.Username);
        }

        if (!String.IsNullOrWhiteSpace(config.Password)) {
            config.Password = Decrypt(config.Password);
        }

        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (String.IsNullOrWhiteSpace(message.From)) {
            message.From = DefaultReplyToAddress;
        }

        if (String.IsNullOrWhiteSpace(message.From)) {
            output.Messages.Add("Sending an email requires a valid From address.");
        }
        if (String.IsNullOrWhiteSpace(message.Subject)) {
            output.Messages.Add("Sending an email requires a Subject.");
        }
        if (String.IsNullOrWhiteSpace(message.Body)) {
            output.Messages.Add("Sending an email requires a Body.");
        }
        int Recipients = 0;
        if (message.To != null) {
            Recipients += message.To.Count();
        }
        if (message.Cc != null) {
            Recipients += message.Cc.Count();
        }
        if (message.Bcc != null) {
            Recipients += message.Bcc.Count();
        }
        if (Recipients == 0) {
            output.Messages.Add("Sending an email requires at least one recipient.");
        }
        if (output.Messages.Count() > 0) {
            return output;
        }

        MailMessage m = new MailMessage();

        if (!String.IsNullOrEmpty(message.From)) {
            m.From = String.IsNullOrWhiteSpace(message.FromDisplayName)
                ? new MailAddress(message.From)
                : new MailAddress(message.From, message.FromDisplayName);
        }

        m.SubjectEncoding = System.Text.Encoding.UTF8;
        m.Subject = message.Subject;
        m.IsBodyHtml = true;
        m.Body = message.Body;

        if (message.To != null && message.To.Count() > 0) {
            foreach (var To in message.To) {
                if (!String.IsNullOrWhiteSpace(To)) {
                    if (To.Contains("@")) {
                        m.To.Add(To);
                    }
                }
            }
        }

        if (message.Cc != null && message.Cc.Count() > 0) {
            foreach (var Cc in message.Cc) {
                if (!String.IsNullOrWhiteSpace(Cc)) {
                    if (Cc.Contains("@")) {
                        m.CC.Add(Cc);
                    }
                }
            }
        }

        if (message.Bcc != null && message.Bcc.Count() > 0) {
            foreach (var Bcc in message.Bcc) {
                if (!String.IsNullOrWhiteSpace(Bcc)) {
                    if (Bcc.Contains("@")) {
                        m.Bcc.Add(Bcc);
                    }
                }
            }
        }

        if (message.Files != null && message.Files.Count() > 0) {
            foreach (var file in message.Files) {
                if (file.Value != null) {
                    System.Net.Mail.Attachment att = new System.Net.Mail.Attachment(new MemoryStream(file.Value), file.FileName);
                    m.Attachments.Add(att);
                }
            }
        }

        SmtpClient? s = null;

        try {
            if (String.IsNullOrWhiteSpace(config.Server)) {
                s = new SmtpClient();
            } else {
                s = new SmtpClient(config.Server, config.Port);
                if (config.UseSSL) {
                    s.EnableSsl = true;
                }
            }

            if (!String.IsNullOrWhiteSpace(config.Username) && !String.IsNullOrWhiteSpace(config.Password)) {
                s.UseDefaultCredentials = false;
                s.Credentials = new System.Net.NetworkCredential(config.Username, config.Password);
            }
        } catch (Exception ex) {
            output.Messages.Add("Error Creating SmtpClient - " + ex.Message);
            return output;
        }

        if (s != null) {
            int sendAttempts = 25;
#if DEBUG
            //sendAttempts = 0;
            //output.Result = true;
#endif
            if (sendAttempts > 0) {
                // Sometimes a sendmail fails, so give it multiple attempts to send
                string SendError = String.Empty;
                for (var x = 0; x < sendAttempts; x++) {
                    if (output.Result == false) {
                        try {
                            s.Send(m);
                            output.Result = true;
                        } catch (Exception ex) {
                            SendError = "Error Sending Email - " + ex.Message;
                        }
                    }
                }

                if (output.Result == false) {
                    output.Messages.Add(SendError);
                }
            }
        } else {
            output.Messages.Add("Unable to Connect to Mail Server");
        }
        return output;
    }

    public string Serialize_ObjectToXml(object o, bool OmitXmlDeclaration = true)
    {
        XmlSerializer serializer = new XmlSerializer(o.GetType());

        XmlWriterSettings settings = new XmlWriterSettings();
        //settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
        settings.Indent = true;
        settings.OmitXmlDeclaration = OmitXmlDeclaration;

        string strSerializedXML = String.Empty;

        using (StringWriter textWriter = new StringWriter()) {
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings)) {
                serializer.Serialize(xmlWriter, o);
            }
            return textWriter.ToString();
        }
    }

    public T? Serialize_XmlToObject<T>(string? xml)
    {
        var output = default(T);

        if (!string.IsNullOrEmpty(xml)) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReaderSettings settings = new XmlReaderSettings();

            using (StringReader textReader = new StringReader(xml)) {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings)) {
                    var deserialized = serializer.Deserialize(xmlReader);
                    if (deserialized != null) {
                        output = (T)deserialized;
                    }
                }
            }
        }

        return output;
    }

    public string SerializeObject(object? Object)
    {
        string output = String.Empty;

        if (Object != null) {
            output += Newtonsoft.Json.JsonConvert.SerializeObject(Object);
        }

        return output;
    }

    public void SetHttpContext(Microsoft.AspNetCore.Http.HttpContext context)
    {
        if (context != null) {
            _httpContext = context;
        }
    }

    private bool ShowTenantCodeFieldOnLoginForm {
        get {
            bool output = false;
            if(CacheStore.ContainsKey(Guid.Empty, "ShowTenantCodeFieldOnLoginForm")) {
                output = CacheStore.GetCachedItem<bool>(Guid.Empty, "ShowTenantCodeFieldOnLoginForm");
            } else {
                output = GetSetting<bool>("ShowTenantCodeFieldOnLoginForm", DataObjects.SettingType.Boolean);
                CacheStore.SetCacheItem(Guid.Empty, "ShowTenantCodeFieldOnLoginForm", output);
            }
            return output;
        }
    }

    private bool ShowTenantListingWhenMissingTenantCode {
        get {
            bool output = false;
            if (CacheStore.ContainsKey(Guid.Empty, "ShowTenantListingWhenMissingTenantCode")) {
                output = CacheStore.GetCachedItem<bool>(Guid.Empty, "ShowTenantListingWhenMissingTenantCode");
            } else {
                output = GetSetting<bool>("ShowTenantListingWhenMissingTenantCode", DataObjects.SettingType.Boolean);
                CacheStore.SetCacheItem(Guid.Empty, "ShowTenantListingWhenMissingTenantCode", output);
            }
            return output;
        }
    }

    public string StringValue(string? input)
    {
        return !String.IsNullOrEmpty(input) ? input : String.Empty;
    }

    public string UrlDecode(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(input)) {
            output = System.Net.WebUtility.UrlDecode(input);
        }

        return output;
    }

    public string UrlEncode(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(input)) {
            output = System.Net.WebUtility.UrlEncode(input);
        }

        return output;
    }

    private bool UseTenantCodeInUrl {
        get {
            bool output = false;
            if (CacheStore.ContainsKey(Guid.Empty, "UseTenantCodeInUrl")) {
                output = CacheStore.GetCachedItem<bool>(Guid.Empty, "UseTenantCodeInUrl");
            } else {
                output = GetSetting<bool>("UseTenantCodeInUrl", DataObjects.SettingType.Boolean);
                CacheStore.SetCacheItem(Guid.Empty, "UseTenantCodeInUrl", output);
            }
            return output;
        }
    }

    public string Version {
        get {
            return _version;
        }
    }

    public DataObjects.VersionInfo VersionInfo {
        get {
            return new DataObjects.VersionInfo {
                Released = _released,
                RunningSince = RunningSince,
                Version = _version
            };
        }
    }

    private string WebsiteName(string? url)
    {
        string output = WebsiteRoot(url);

        if (output.ToLower().StartsWith("https://")) {
            output = output.Substring(8);
        } else if (output.ToLower().StartsWith("http://")) {
            output = output.Substring(7);
        }

        if (output.EndsWith("/")) {
            output = output.Substring(0, output.Length - 1);
        }


        return output;
    }

    private string WebsiteRoot(string? url)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(url)) {
            int start = url.IndexOf("://");
            if (start > -1) {
                int end = url.IndexOf("/", start + 3);
                if (end > -1) {
                    output = url.Substring(0, end + 1);
                }
            }
        }

        return output;
    }
}