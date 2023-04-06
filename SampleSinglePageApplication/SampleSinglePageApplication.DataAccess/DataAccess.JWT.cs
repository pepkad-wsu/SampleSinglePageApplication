namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public string GetSourceJWT(Guid TenantId, string Source)
    {
        string output = String.Empty;
        Dictionary<string, object> Payload = new Dictionary<string, object> {
                {"Source", Source }
            };
        output = JwtEncode(TenantId, Payload);
        return output;
    }

    public string JsonWebTokenKey(Guid TenantId)
    {
        string output = StringValue(CacheStore.GetCachedItem<string>(TenantId, "JasonWebTokenKey"));

        if (String.IsNullOrEmpty(output)) {
            var settings = GetTenantSettings(TenantId);
            if (settings != null) {
                if (!String.IsNullOrEmpty(settings.JasonWebTokenKey)) {
                    output = settings.JasonWebTokenKey;
                    CacheStore.SetCacheItem(TenantId, "JasonWebTokenKey", output);
                }
            }
        }

        if (String.IsNullOrEmpty(output)) {
            // Use a default token if no value is set.
            output = "ccb377729940478db0ff6f6aedbf75a1";
        }

        return output;
    }

    public Dictionary<string, object> JwtDecode(Guid TenantId, string Encrypted)
    {
        Dictionary<string, object> output = new Dictionary<string, object>();
        try {
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();

            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            string key = JsonWebTokenKey(TenantId);
            output = decoder.DecodeToObject<Dictionary<string, object>>(Encrypted, key, true);
        } catch (Exception ex) {
            if (ex != null) {

            }
        }

        return output;
    }

    public string JwtEncode(Guid TenantId, Dictionary<string, object> Payload)
    {
        Dictionary<string, object> payload = new Dictionary<string, object>();

        // Add in the payload options that are missing
        if (!Payload.ContainsKey("iss")) {
            payload.Add("iss", _appName);
        }
        if (!Payload.ContainsKey("iat")) {
            payload.Add("iat", NowFromUnixEpoch());
        }
        if (!Payload.ContainsKey("exp")) {
            payload.Add("exp", NowFromUnixEpoch() + (86400 * 14)); // Default to 14 days
        }

        // Now, add the remaining items from the payload
        foreach (var item in Payload) {
            payload.Add(item.Key, item.Value);
        }

        IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        IJsonSerializer serializer = new JsonNetSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

        string key = JsonWebTokenKey(TenantId);
        string output = encoder.Encode(payload, key);
        return output;
    }

    public bool ValidateSourceJWT(Guid TenantId, string Source, string JWT)
    {
        bool output = false;

        string SourceCheck = String.Empty;
        Dictionary<string, object> decrypted = JwtDecode(TenantId, JWT);
        try {
            SourceCheck = decrypted["Source"] + String.Empty;
            if (SourceCheck == Source) {
                output = true;
            }
        } catch { }

        return output;
    }
}