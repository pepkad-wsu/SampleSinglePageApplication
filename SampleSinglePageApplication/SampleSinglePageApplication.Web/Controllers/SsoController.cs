using Microsoft.AspNetCore.Mvc;

namespace SampleSinglePageApplication.Web.Controllers;

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
}
