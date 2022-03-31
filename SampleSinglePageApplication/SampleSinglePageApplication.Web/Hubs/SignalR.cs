using Microsoft.AspNetCore.SignalR;

namespace SampleSinglePageApplication.Web.Hubs;
public class SignalRHub : Hub
{
    private List<string> tenants = new List<string>();
    //private HttpContext? context;
    //private IDataAccess da;

    //public SignalRHub(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor)
    //{
    //    da = daInjection;

    //    if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
    //        da.SetHttpContext(httpContextAccessor.HttpContext);
    //        context = httpContextAccessor.HttpContext;
    //        da.SetHttpContext(context);

    //    }
    //}

    //public override Task OnConnectedAsync()
    //{
    //    return base.OnConnectedAsync();
    //}

    public async Task JoinTenantId(string TenantId)
    {
        if (!tenants.Contains(TenantId)) {
            tenants.Add(TenantId);
        }

        // Before adding a user to a Tenant group remove them from any groups they were in before.
        if (tenants != null && tenants.Count() > 0) {
            foreach(var tenant in tenants) {
                try {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenant);
                } catch { }
            }
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, TenantId);
    }

    //public async Task SignalRUpdate(DataObjects.SignalRUpdate update)
    //{
    //    if (update.TenantId != null) {
    //        string tenantId = ((Guid)update.TenantId).ToString();

    //        await Clients.Group(tenantId).SendAsync("SignalRUpdate", update);
    //    }

    //    //await Clients.All.SendAsync("SignalRUpdate", update);
    //}

    //private string HeaderValue(String ValueName)
    //{
    //    string output = String.Empty;
    //    try {
    //        if (context != null) {
    //            output = context.Request.Headers[ValueName];
    //        }
    //    } catch { }

    //    return output;
    //}
}

