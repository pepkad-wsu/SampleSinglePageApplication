using Microsoft.AspNetCore.Mvc;

namespace SampleSinglePageApplication.Web.Controllers
{
    public class LogoutController : Controller
    {
        private HttpContext? context;
        private IDataAccess da;

        public LogoutController(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor)
        {
            da = daInjection;

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
            bool useSSO = false;
            string eitSsoUrl = String.Empty;
            string cookieDomain = String.Empty;
            
            var tenants = await da.GetTenants();
            if (tenants != null && tenants.Any()) {
                foreach (var tenant in tenants) {
                    var eitSSO = tenant.TenantSettings.LoginOptions.FirstOrDefault(x => x.ToLower() == "eitsso");
                    if (eitSSO != null) {
                        if(tenant.TenantId == id) {
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
                    da.CookieWrite("Token-" + tenant.TenantId.ToString(), "");
                }
            }

            if (useSSO) {
                // Only need to redirect to SSO logout if we have a value for the sso-auth-user cookie.
                string authUser = da.CookieRead("sso-auth-user");
                if (!String.IsNullOrWhiteSpace(authUser)) {
                    // Remove any SSO login
                    da.CookieWrite("user-groups", "", cookieDomain);
                    da.CookieWrite("user-groups-checksum", "", cookieDomain);
                    da.CookieWrite("sso-auth-user", "", cookieDomain);
                    da.CookieWrite("sso-auth-object", "", cookieDomain);

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
}
