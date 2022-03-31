using Microsoft.AspNetCore.Mvc;

namespace SampleSinglePageApplication.Web.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            string username = da.Request("local-username");
            string password = da.Request("local-password");
            string tenantId = da.Request("local-login-tenantid");
            string url = da.Request("local-login-url");
            string token = String.Empty;
            string error = String.Empty;

            Guid TenantId = Guid.Empty;
            if (!String.IsNullOrWhiteSpace(tenantId)) {
                try {
                    TenantId = new Guid(tenantId);
                } catch { }
            }

            if(!String.IsNullOrWhiteSpace(username) && !String.IsNullOrEmpty(password) && TenantId != Guid.Empty) {
                DataObjects.User? auth = await da.Authenticate(TenantId, username, password, true);
                if(auth != null && auth.ActionResponse.Result) {
                    token += auth.AuthToken;
                }else if(auth != null && auth.ActionResponse.Messages != null && auth.ActionResponse.Messages.Any()) {
                    error = auth.ActionResponse.Messages[0];
                }
            }

            if (!String.IsNullOrEmpty(token)) {
                // Valid Login, write out user token.
                da.CookieWrite("Token-" + TenantId.ToString(), token);
                da.CookieWrite("login-error", "");
                return RedirectToAction("Index", "Home");
            } else {
                if (String.IsNullOrEmpty(error)) {
                    error = "Invalid Username or Password";
                }
                da.CookieWrite("login-error", error);

                return Redirect(url + "LocalLoginError/");
            }

        }
    }
}
