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
    public void CookieWrite(string cookieName, string value)
    {
        if (_httpContext != null) {
            DateTime now = DateTime.Now;
            if (String.IsNullOrEmpty(cookieName)) { return; }

            Microsoft.AspNetCore.Http.CookieOptions option = new Microsoft.AspNetCore.Http.CookieOptions();
            option.Expires = now.AddYears(1);

            string fullUrl = GetFullUrl();

            if (!String.IsNullOrWhiteSpace(fullUrl) && !fullUrl.ToLower().Contains("localhost")) {
                option.Domain = ".wsu.edu";
            }

            _httpContext.Response.Cookies.Append(cookieName, value, option);
        }
    }

    public T? DeserializeObject<T>(string SerializedObject)
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

    public Guid GuidOrEmpty(Guid? guid)
    {
        return guid.HasValue ? (Guid)guid : Guid.Empty;
    }

    public string HtmlToPlainText(string html)
    {
        string output = Regex.Replace(html, @"<(.|\n)*?>", "");
        output = System.Web.HttpUtility.HtmlDecode(output);
        return output;
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
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
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

    public async Task<DataObjects.BooleanResponse> SendEmail(DataObjects.EmailMessage message, DataObjects.MailServerConfig? config = null)
    {
        if (config == null) {
            config = await GetMailServerConfig();
        }

        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

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

    public string Serialize_ObjectToXml(object o, bool OmitXmlDeclaration)
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

    public string SerializeObject(object Object)
    {
        string output = Newtonsoft.Json.JsonConvert.SerializeObject(Object);
        return output;
    }

    public void SetHttpContext(Microsoft.AspNetCore.Http.HttpContext context)
    {
        if (context != null) {
            _httpContext = context;
        }
    }

    public string StringOrEmpty(string? input)
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

    public string Version {
        get {
            return _version;
        }
    }

}