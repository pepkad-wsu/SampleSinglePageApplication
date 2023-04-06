using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace SampleSinglePageApplication;
public interface ICustomAuthentication
{
    bool Enabled { get; }
    bool UseFacebook { get; }
    bool UseMicrosoftAccount { get; }
    bool UseOpenId { get; }
    bool UseGoogle { get; }
}

public class CustomAuthentication : ICustomAuthentication
{
    private CustomAuthenticationConfiguration _config;

    public CustomAuthentication(CustomAuthenticationConfiguration config)
    {
        _config = config;
    }

    public bool Enabled {
        get {
            return _config.Enabled;
        }
    }

    public bool UseFacebook {
        get {
            return _config.UseFacebook;
        }
    }

    public bool UseMicrosoftAccount {
        get {
            return _config.UseMicrosoftAccount;
        }
    }

    public bool UseOpenId {
        get {
            return _config.UseOpenId;
        }
    }

    public bool UseGoogle {
        get {
            return _config.UseGoogle;
        }
    }
}

public class CustomAuthenticationConfiguration
{
    public bool Enabled { get; set; }
    public bool UseFacebook { get; set; }
    public bool UseMicrosoftAccount { get; set; }
    public bool UseOpenId { get; set; }
    public bool UseGoogle { get; set; }
}


public static class CustomAuthenticationProviders
{
    public static CustomAuthenticationConfiguration UseAuthorization(WebApplicationBuilder applicationBuilder)
    {
        CustomAuthenticationConfiguration output = new CustomAuthenticationConfiguration();

        string facebookAppId = String.Empty;
        string facebookAppSecret = String.Empty;
        try { facebookAppId += applicationBuilder.Configuration["AuthenticationProviders:Facebook:AppId"]; } catch { }
        try { facebookAppSecret += applicationBuilder.Configuration["AuthenticationProviders:Facebook:AppSecret"]; } catch { }
        if (!String.IsNullOrWhiteSpace(facebookAppId) && !String.IsNullOrWhiteSpace(facebookAppSecret)) {
            output.Enabled = true;
            output.UseFacebook = true;
        }

        string microsoftAccountClientId = String.Empty;
        string microsoftAccountClientSecret = String.Empty;
        try { microsoftAccountClientId += applicationBuilder.Configuration["AuthenticationProviders:MicrosoftAccount:ClientId"]; } catch { }
        try { microsoftAccountClientSecret += applicationBuilder.Configuration["AuthenticationProviders:MicrosoftAccount:ClientSecret"]; } catch { }
        if (!String.IsNullOrEmpty(microsoftAccountClientId) && !String.IsNullOrEmpty(microsoftAccountClientSecret)) {
            output.Enabled = true;
            output.UseMicrosoftAccount = true;
        }

        string openIdClientId = String.Empty;
        string openIdClientSecret = String.Empty;
        string openIdAuthority = String.Empty;
        try { openIdClientId += applicationBuilder.Configuration["AuthenticationProviders:OpenId:ClientId"]; } catch { }
        try { openIdClientSecret += applicationBuilder.Configuration["AuthenticationProviders:OpenId:ClientSecret"]; } catch { }
        try { openIdAuthority += applicationBuilder.Configuration["AuthenticationProviders:OpenId:Authority"]; } catch { }
        if (!String.IsNullOrEmpty(openIdClientId) && !String.IsNullOrEmpty(openIdClientSecret) && !String.IsNullOrEmpty(openIdAuthority)) {
            output.Enabled = true;
            output.UseOpenId = true;
        }

        string googleClientId = String.Empty;
        string googleClientSecret = String.Empty;
        try { googleClientId += applicationBuilder.Configuration["AuthenticationProviders:Google:ClientId"]; } catch { }
        try { googleClientSecret += applicationBuilder.Configuration["AuthenticationProviders:Google:ClientSecret"]; } catch { }
        if (!String.IsNullOrEmpty(googleClientId) && !String.IsNullOrEmpty(googleClientSecret)) {
            output.Enabled = true;
            output.UseGoogle = true;
        }

        if (output.Enabled) {
            var auth = applicationBuilder.Services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            });
            auth.AddCookie();

            if (output.UseFacebook) {
                auth.AddFacebook("Facebook", o => {
                    o.AppId = facebookAppId;
                    o.AppSecret = facebookAppSecret;
                    o.AccessDeniedPath = "/Authorization/AccessDenied";
                });
            }

            if (output.UseGoogle) {
                auth.AddGoogle("Google", o => {
                    o.ClientId = googleClientId;
                    o.ClientSecret = googleClientSecret;
                    o.AccessDeniedPath = "/Authorization/AccessDenied";
                });
            }

            if (output.UseMicrosoftAccount) {
                auth.AddMicrosoftAccount("MicrosoftAccount", o => {
                    o.ClientId = microsoftAccountClientId;
                    o.ClientSecret = microsoftAccountClientSecret;
                    o.AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
                    o.AccessDeniedPath = "/Authorization/AccessDenied";
                });
            }

            if (output.UseOpenId) {
                auth.AddOpenIdConnect("OpenId", o => {
                    o.ClientId = openIdClientId;
                    o.ClientSecret = openIdClientSecret;
                    o.Authority = openIdAuthority;
                    o.ResponseType = "code";
                    o.GetClaimsFromUserInfoEndpoint = true;
                    o.AccessDeniedPath = "/Authorization/AccessDenied";
                });
            }
        }

        return output;
    }
}