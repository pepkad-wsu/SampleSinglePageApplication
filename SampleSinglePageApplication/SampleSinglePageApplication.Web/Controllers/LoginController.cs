using Microsoft.AspNetCore.Mvc;

namespace SampleSinglePageApplication.Web.Controllers;

public class LoginController : Controller
{
    private HttpContext? context;
    private IDataAccess da;

    public LoginController(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor)
    {
        da = daInjection;

        if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            da.SetHttpContext(httpContextAccessor.HttpContext);
            context = httpContextAccessor.HttpContext;
            da.SetHttpContext(context);
        }
    }

    [Route("~/LoginLocal")]
    public async Task<IActionResult> LoginLocal()
    {
        string tenantCode = da.Request("local-tenantCode");
        string username = da.Request("local-username");
        string password = da.Request("local-password");
        string tenantId = da.Request("local-login-tenantid");
        string url = da.Request("local-login-url");
        string token = String.Empty;
        string error = String.Empty;
        string baseUrl = da.ApplicationURL;
        if (!baseUrl.EndsWith("/")) { baseUrl += "/"; }

        // If the login page is refreshed it will post, so if no values were received just
        // return to the page
        if (String.IsNullOrWhiteSpace(tenantCode + username + password + tenantId + url)) {
            return Redirect(baseUrl);
        }

        Guid TenantId = Guid.Empty;

        if (!String.IsNullOrWhiteSpace(tenantCode)) {
            // A tenant code was passed, so try and get the tenant from the code
            var tenant = await da.GetTenantFromCode(tenantCode);
            if (tenant != null && tenant.ActionResponse.Result) {
                if (tenant.Enabled) {
                    TenantId = tenant.TenantId;
                } else {
                    return Redirect(url + "?Error=" + da.UrlEncode("Tenant '" + tenantCode + "' Disabled"));
                }
            } else {
                // An invalid tenant code was entered
                return Redirect(url + "?Error=" + da.UrlEncode("Invalid Tenant Code '" + tenantCode + "'"));
            }
        }

        if (TenantId == Guid.Empty) {
            // Tenant not found from tenant code, so try the Guid
            if (!String.IsNullOrWhiteSpace(tenantId)) {
                try {
                    TenantId = new Guid(tenantId);
                } catch { }
            }
        }

        if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrEmpty(password)) {
            DataObjects.User? auth = null;

            if (TenantId != Guid.Empty) {
                auth = await da.Authenticate(username, password, TenantId);
            } else {
                auth = await da.Authenticate(username, password, null);
                if (auth != null && auth.ActionResponse.Result) {
                    TenantId = auth.TenantId;

                    // We need to get the tenant code for this user.
                    if (auth.UserTenants != null) {
                        foreach (var item in auth.UserTenants) {
                            if (String.IsNullOrEmpty(tenantCode) && !String.IsNullOrEmpty(item.TenantCode)) {
                                if (item.TenantId == auth.TenantId) {
                                    tenantCode = item.TenantCode;
                                }
                            }
                        }
                    }
                }
            }

            if (auth != null && auth.ActionResponse.Result) {
                token += auth.AuthToken;
            } else if (auth != null && auth.ActionResponse.Messages != null && auth.ActionResponse.Messages.Any()) {
                error = auth.ActionResponse.Messages[0];
            }
        }

        if (!String.IsNullOrEmpty(token)) {
            // Valid Login, write out user token.
            da.CookieWrite("Token-" + TenantId.ToString(), token);

            // See if we are using tenant code in the URL
            var appSettings = da.GetApplicationSettings();
            if (appSettings.UseTenantCodeInUrl) {
                // Get the info for this tenant.
                var tenant = da.GetTenant(TenantId);
                if (tenant != null && tenant.ActionResponse.Result) {
                    baseUrl += tenant.TenantCode;
                }
                da.CookieWrite(da.AppName + "-tenant-code", "");
            } else {
                da.CookieWrite(da.AppName + "-tenant-code", tenantCode);
            }

            return Redirect(baseUrl);
        } else {
            if (String.IsNullOrEmpty(error)) {
                error = "Invalid Username or Password";
            }

            return Redirect(url + "?Error=" + da.UrlEncode(error));
        }
    }
}