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

        public async Task<IActionResult> Index(Guid id, string redirect)
        {
            // Remove any possible user tokens.
            var tenants = await da.GetTenants();
            if(tenants != null && tenants.Any()) {
                foreach(var tenant in tenants) {
                    da.CookieWrite("Token-" + tenant.TenantId.ToString(), "");
                }
            }

            // Remove any SSO login
            da.CookieWrite("user-groups", "");
            da.CookieWrite("user-groups-checksum", "");
            da.CookieWrite("sso-auth-user", "");
            da.CookieWrite("sso-auth-object", "");

            string ssoLogout = "https://sso.em.wsu.edu/Logout?Redirect=" + da.UrlEncode(redirect);

            return Redirect(ssoLogout);
        }
    }
}
