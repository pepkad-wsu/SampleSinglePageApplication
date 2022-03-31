using Microsoft.AspNetCore.Mvc;

namespace SampleSinglePageApplication.Web.Controllers
{
    public class SsoController : Controller
    {
        private HttpContext? context;
        private IDataAccess da;

        public SsoController(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor)
        {
            da = daInjection;

            if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
                da.SetHttpContext(httpContextAccessor.HttpContext);
                context = httpContextAccessor.HttpContext;
                da.SetHttpContext(context);
            }
        }

        public async Task<RedirectResult> Authenticate()
        {
            string redirect = da.QueryStringValue("redirect");

            if (context != null) {
                var auth = new SSO.Auth.SingleSignOn(context);
                auth.Authenticate();
            }

            if (!String.IsNullOrEmpty(redirect)) {
                return Redirect(redirect);
            } else {
                return Redirect("/");
            }
        }

        public async Task<RedirectResult> Logout()
        {
            string returnUrl = da.QueryStringValue("ReturnUrl");

            if (context != null) {
                var auth = new SSO.Auth.SingleSignOn(context);
                auth.Logout();

                if (String.IsNullOrEmpty(returnUrl)) {
                    string? appUrl = da.GetSetting<string>("ApplicationURL", DataObjects.SettingType.Text);
                    if (!String.IsNullOrEmpty(appUrl)) {
                        returnUrl += appUrl;
                    }
                }
            }

            return Redirect("https://sso.em.wsu.edu/Logout?Redirect=" + da.UrlEncode(returnUrl));
        }
    }
}
