using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace SampleSinglePageApplication.Web.Controllers;

public class LogoutController : Controller
{
    private HttpContext? context;
    private IDataAccess da;
    private ICustomAuthentication auth;

    public LogoutController(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor, ICustomAuthentication customAuthentication)
    {
        da = daInjection;
        auth = customAuthentication;

        if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            da.SetHttpContext(httpContextAccessor.HttpContext);
            context = httpContextAccessor.HttpContext;
            da.SetHttpContext(context);
        }
    }

    [Route("~/Logout/{id}")]
    public async Task<IActionResult> Index(Guid id, string redirect)
    {
        // Remove any possible user tokens.
        var baseURL = await da.AppSetting<string>("ApplicationURL");

        bool useSSO = false;
        string eitSsoUrl = String.Empty;
        string cookieDomain = String.Empty;

        var tenants = await da.GetTenants();
        if (tenants != null && tenants.Any()) {
            foreach (var tenant in tenants) {
                var eitSSO = tenant.TenantSettings.LoginOptions.FirstOrDefault(x => x.ToLower() == "eitsso");
                if (eitSSO != null) {
                    if (tenant.TenantId == id) {
                        if (String.IsNullOrEmpty(cookieDomain)) {
                            cookieDomain += tenant.TenantSettings.CookieDomain;
                        }

                        if (String.IsNullOrEmpty(eitSsoUrl)) {
                            eitSsoUrl += tenant.TenantSettings.EitSsoUrl;
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(eitSsoUrl)) {
                        useSSO = true;
                    }
                }

                var facebook = tenant.TenantSettings.LoginOptions.FirstOrDefault(x => x.ToLower() == "facebook");
                var google = tenant.TenantSettings.LoginOptions.FirstOrDefault(x => x.ToLower() == "google");
                var microsoft = tenant.TenantSettings.LoginOptions.FirstOrDefault(x => x.ToLower() == "microsoft");
                var openId = tenant.TenantSettings.LoginOptions.FirstOrDefault(x => x.ToLower() == "openid");

                string authType = String.Empty;

                if (context != null) {
                    if (context.User != null && context.User.Identity != null) {
                        if (context.User.Identity.IsAuthenticated) {
                            authType += context.User.Identity.AuthenticationType;
                        }
                    }

                    if (auth.Enabled) {
                        await context.SignOutAsync();

                        if (auth.UseFacebook || auth.UseGoogle || auth.UseMicrosoftAccount || auth.UseOpenId) {
                            await context.SignOutAsync("Cookies");
                        }

                        if (auth.UseOpenId) {
                            await context.SignOutAsync("OpenId");
                        }
                    }
                }

                if (String.IsNullOrWhiteSpace(authType)) {
                    authType = da.CookieRead("Login-Method");
                }

                if (!String.IsNullOrWhiteSpace(authType)) {
                    switch (authType.ToLower()) {
                        case "custom":
                            redirect = baseURL + "Authorization/CustomLogout/" + id.ToString();
                            break;

                        case "facebook":
                            // TODO: See if there is a Facebook logout URL with redirect support
                            break;

                        case "microsoftaccount":
                            redirect = "https://login.microsoftonline.com/common/oauth2/v2.0/logout?post_logout_redirect_uri=" +
                                da.UrlEncode(da.StringValue(baseURL));
                            break;
                    }
                }

                da.CookieWrite("Token-" + tenant.TenantId.ToString(), "");
            }
        }

        da.CookieWrite(da.AppName + "-tenant-code", "");

        if (useSSO) {
            // Only need to redirect to SSO logout if we have a value for the sso-token cookie.
            string ssoToken = da.CookieRead("sso-token");
            if (!String.IsNullOrWhiteSpace(ssoToken)) {
                // Remove any SSO login
                da.CookieWrite("sso-token", "", cookieDomain);

                string ssoLogout = eitSsoUrl + "Logout?Redirect=" + da.UrlEncode(redirect);
                return Redirect(ssoLogout);
            } else {
                return Redirect(redirect);
            }

        } else {
            return Redirect(redirect);
        }
    }
}