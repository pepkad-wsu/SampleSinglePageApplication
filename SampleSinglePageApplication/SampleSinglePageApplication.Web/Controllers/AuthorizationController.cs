using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Security.Claims;

namespace SampleSinglePageApplication.Web.Controllers;

public class AuthorizationController : Controller
{
    private HttpContext? context;
    private IDataAccess da;

    public AuthorizationController(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor)
    {
        da = daInjection;

        if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            da.SetHttpContext(httpContextAccessor.HttpContext);
            context = httpContextAccessor.HttpContext;
            da.SetHttpContext(context);
        }
    }

    [Route("~/Authorization/AccessDenied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Route("~/Authorization/Custom/{id}")]
    public IActionResult Custom(Guid id)
    {
        return View();
    }

    [Route("~/Authorization/CustomLogout/{id}")]
    public IActionResult CustomLogout(Guid id) 
    {
        return View();
    }

    [Authorize(AuthenticationSchemes = "Facebook")]
    [Route("~/Authorization/Facebook")]
    public IActionResult Facebook()
    {
        string tenantId = da.Request("TenantId");
        return RedirectToAction("FacebookAuthorized", new { TenantId = tenantId });
    }

    public async Task<IActionResult> FacebookAuthorized()
    {
        var result = await ProcessClaims("Facebook");
        if (result.Result) {
            return RedirectToAction("Index", "Home");
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return RedirectToAction(result.Message);
            } else {
                return RedirectToAction("InvalidUser", new { authMethod="Facebook" });
            }
        }
    }

    [Authorize(AuthenticationSchemes = "Google")]
    [Route("~/Authorization/Google")]
    public IActionResult Google()
    {
        string tenantId = da.Request("TenantId");
        return RedirectToAction("GoogleAuthorized", new { TenantId = tenantId });
    }

    public async Task<IActionResult> GoogleAuthorized()
    {
        var result = await ProcessClaims("Google");
        if (result.Result) {
            return RedirectToAction("Index", "Home");
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return RedirectToAction(result.Message);
            } else {
                return RedirectToAction("InvalidUser", new { authMethod = "Google" });
            }
        }
    }

    [Authorize(AuthenticationSchemes = "MicrosoftAccount")]
    [Route("~/Authorization/MicrosoftAccount")]
    public IActionResult MicrosoftAccount()
    {
        string tenantId = da.Request("TenantId");
        return RedirectToAction("MicrosoftAccountAuthorized", new { TenantId = tenantId });
    }

    public async Task<IActionResult> MicrosoftAccountAuthorized()
    {
        var result = await ProcessClaims("MicrosoftAccount");
        if (result.Result) {
            return RedirectToAction("Index", "Home");
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return RedirectToAction(result.Message);
            } else {
                return RedirectToAction("InvalidUser", new { authMethod = "MicrosoftAccount" });
            }
        }
    }

    [Route("~/Authorization/InvalidUser")]
    public IActionResult InvalidUser()
    {
        return View();
    }

    [Route("~/Authorization/NoLocalAccount")]
    public IActionResult NoLocalAccount()
    {
        return View();
    }

    [Authorize(AuthenticationSchemes="OpenId")]
    [Route("~/Authorization/OpenId")]
    public IActionResult OpenId()
    {
        string tenantId = da.Request("TenantId");
        return RedirectToAction("OpenIdAuthorized", new { TenantId=tenantId });
    }

    [Route("~/Authorization/OpenIdAuthorized")]
    public async Task<IActionResult> OpenIdAuthorized()
    {
        var result = await ProcessClaims("OpenId");
        if (result.Result) {
            return RedirectToAction("Index", "Home");
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return RedirectToAction(result.Message);
            } else {
                return RedirectToAction("InvalidUser", new { authMethod = "OpenId" });
            }
        }
    }

    private async Task<DataObjects.SimpleResponse> ProcessClaims(string Source)
    {
        DataObjects.SimpleResponse output = new DataObjects.SimpleResponse();

        string qsTenantId = da.Request("TenantId");

        bool validUser = false;
        bool noLocalAccount = false;

        if (context != null) {
            if (context.User != null) {
                if (context.User.Identity != null) {
                    if (context.User.Identity.IsAuthenticated) {
                        validUser = true;
                        var claims = (System.Security.Claims.ClaimsIdentity)context.User.Identity;

                        if (claims != null && claims.Claims != null && claims.Claims.Any()) {
                            Dictionary<string, string> allClaims = new Dictionary<string, string>();

                            string name = String.Empty;
                            string preferredUsername = String.Empty;
                            string givenName = String.Empty;
                            string familyName = String.Empty;

                            foreach (var claim in claims.Claims) {
                                var claimType = GetClaimType(claim.Type).ToLower();

                                allClaims.Add(claim.Type, claim.Value);

                                switch (claimType) {
                                    case "name":
                                        name += claim.Value;
                                        break;

                                    case "emailaddress":
                                    case "preferred_username":
                                        preferredUsername += claim.Value;
                                        break;

                                    case "givenname":
                                    case "given_name":
                                        givenName += claim.Value;
                                        break;

                                    case "surname":
                                    case "family_name":
                                        familyName += claim.Value;
                                        break;
                                }
                            }

                            if (!String.IsNullOrWhiteSpace(preferredUsername)) {
                                noLocalAccount = true;

                                if (qsTenantId.IsGuid()) {
                                    DataObjects.User user = new DataObjects.User();

                                    Guid tenantId = new Guid(qsTenantId);
                                    var tenant = da.GetTenant(tenantId);

                                    user = await da.GetUserByUsernameOrEmail(tenantId, preferredUsername);
                                    if (user == null || !user.ActionResponse.Result) {
                                        // See if this tenant allows for creating new accounts automatically.
                                        var settings = da.GetTenantSettings(tenantId);
                                        if (!settings.RequirePreExistingAccountToLogIn) {
                                            // Create the new account
                                            DataObjects.User addUser = new DataObjects.User {
                                                UserId = Guid.Empty,
                                                TenantId = tenantId,
                                                FirstName = givenName,
                                                LastName = familyName,
                                                Email = preferredUsername,
                                                Username = preferredUsername,
                                                Admin = false,
                                                Enabled = true,
                                                Source = Source
                                            };

                                            user = await da.SaveUser(addUser);
                                        }
                                    }

                                    if (user != null && user.ActionResponse.Result && user.Enabled) {
                                        output.Result = true;
                                        noLocalAccount = false;

                                        // Write out the user token
                                        da.CookieWrite("Token-" + qsTenantId, da.GetUserToken(tenantId, user.UserId));
                                        da.CookieWrite("Login-Method", Source);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (validUser && noLocalAccount) {
            output.Message = "NoLocalAccount";
        }

        return output;
    }

    private string GetClaimType(string claimType)
    {
        string output = claimType;

        if (!String.IsNullOrWhiteSpace(claimType)) {
            if (claimType.Contains(@"\")) {
                claimType = claimType.Replace(@"\", "/");
            }

            if (claimType.Contains("/")) {
                int pos = claimType.LastIndexOf("/");
                output = claimType.Substring(pos + 1);
            }
        }

        return output;
    }
}