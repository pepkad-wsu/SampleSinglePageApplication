using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using SampleSinglePageApplication.Web.Hubs;

namespace SampleSinglePageApplication.Web.Controllers;

[ApiController]
[Route("~/api/[controller]/[action]")]
[Produces("application/json")]
public class DataController : ControllerBase
{
	private HttpContext? context;
	private IDataAccess da;
	private DataObjects.User CurrentUser;
    private string TenantCode;
    private Guid TenantId = Guid.Empty;
    private string SourceToken;
    private IHostApplicationLifetime _hostLifetime;

    private readonly IHubContext<SignalRHub>? _signalR;

	#region Internal Utilities
	public DataController(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor, IHubContext<SignalRHub> hubContext, IHostApplicationLifetime hostLifetime)
	{
		da = daInjection;

        _hostLifetime = hostLifetime;

        if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
				da.SetHttpContext(httpContextAccessor.HttpContext);
				context = httpContextAccessor.HttpContext;
				da.SetHttpContext(context);
			}

        if(hubContext != null) {
            _signalR = hubContext;
        }

        // See if a TenantId is included in the header or querystring.
		string tenantId = HeaderValue("TenantId");
		if (String.IsNullOrEmpty(tenantId)) {
			tenantId = QueryStringValue("TenantId");
		}
		if (!String.IsNullOrEmpty(tenantId)) {
			try {
				TenantId = new Guid(tenantId);
			} catch { }
		}

        // See if a TenantCode is included in the header or querystring
        TenantCode = HeaderValue("TenantCode");
        if(String.IsNullOrEmpty(TenantCode)) {
            TenantCode = QueryStringValue("TenantCode");
        }

        // If we still don't have a TenantCode and TenantId, get the default TenantCode.
        if (String.IsNullOrEmpty(TenantCode) && TenantId == Guid.Empty) {
            TenantCode = da.DefaultTenantCode;
            // If a code exists get the TenantId for that code.
            if (!String.IsNullOrEmpty(TenantCode)) {
                TenantId = da.GetTenantIdFromCode(TenantCode);
            }
        }


        // See if a Token is included in the header or querystring.
        string Token = HeaderValue("Token");
			if (String.IsNullOrWhiteSpace(Token)) {
				Token = QueryStringValue("Token");
			}

        // Set the CurrentUser to a new User object and if we have a valid Token
        // use that to get set the CurrentUser.
        CurrentUser = new DataObjects.User();
        if (!String.IsNullOrWhiteSpace(Token)) {
				CurrentUser = da.GetUserFromToken(TenantId, Token).Result;
			}

        // See if the Source is included in the header or querystring.
        SourceToken = String.Empty;
        string Source = HeaderValue("Source");
        if (String.IsNullOrEmpty(Source)) {
            Source = QueryStringValue("Source");
        }

        // If there is a Source value, make sure that it is valid.
        if (!String.IsNullOrEmpty(Source)) {
            if (da.ValidateSourceJWT(TenantId, Source, Token)) {
                SourceToken = Source;
            }
        }
    }

	private string HeaderValue(String ValueName)
	{
		string output = String.Empty;
		try {
        if(Request != null) {
            output = Request.Headers[ValueName];
        }else if(context != null && context.Request != null) {
            output = context.Request.Headers[ValueName];
        }
		} catch { }

		return output;
	}

	private string QueryStringValue(String ValueName)
	{
		string output = String.Empty;
		try {
        if(Request != null) {
            output = Request.Query[ValueName].ToString();
        }else if(context != null && context.Request != null) {
            output = context.Request.Query[ValueName].ToString();
        }
		} catch { }
		return output;
	}
    #endregion

    [HttpPost]
    [Route("~/api/Data/AddModule")]
    public ActionResult<DataObjects.BooleanResponse> AddModule(DataObjects.AddModule module)
    {
        if (CurrentUser.AppAdmin) {
            var output = da.AddModule(module);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/AjaxUserSearch/")]
    public ActionResult<DataObjects.AjaxLookup> AjaxUserSearch(DataObjects.AjaxLookup Lookup)
    {
        DataObjects.AjaxLookup output = new DataObjects.AjaxLookup();

        if (CurrentUser.Enabled) {
            output = da.AjaxUserSearch(Lookup);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/AjaxUserSearchLocalOnly/")]
    public ActionResult<DataObjects.AjaxLookup> AjaxUserSearchLocalOnly(DataObjects.AjaxLookup Lookup)
    {
        DataObjects.AjaxLookup output = new DataObjects.AjaxLookup();

        if (CurrentUser.Enabled) {
            output = da.AjaxUserSearch(Lookup, true);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/Authenticate/{id}")]
    public async Task<ActionResult<DataObjects.User>> Authenticate(Guid id, DataObjects.Authenticate authenticate)
    {
        DataObjects.User output = new DataObjects.User();

        if (authenticate != null && !String.IsNullOrEmpty(authenticate.Username) && !String.IsNullOrEmpty(authenticate.Password)) {
            output = await da.Authenticate(authenticate.Username, authenticate.Password, id);
        }

        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/Decrypt/")]
    public ActionResult<DataObjects.BooleanResponse> Decrypt(DataObjects.SimplePost post)
    {
        var output = new DataObjects.BooleanResponse();

        if (CurrentUser.Admin) {
            string DecryptedText = da.Decrypt(post.SingleItem);
            if (String.IsNullOrWhiteSpace(DecryptedText)) {
                output.Messages.Add("Error Decrypting Text");
            } else {
                output.Messages.Add(DecryptedText);
                output.Result = true;
            }
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/DeleteDepartment/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteDepartment(Guid id)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (CurrentUser.Admin) {
            output = await da.DeleteDepartment(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/DeleteDepartmentGroup/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteDepartmentGroup(Guid id)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (CurrentUser.Admin) {
            output = await da.DeleteDepartmentGroup(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/DeleteFile/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteFile(Guid id)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (CurrentUser.Admin) {
            DataObjects.FileStorage file = await da.GetFileStorage(id);
            if (file.ActionResponse != null && file.ActionResponse.Result == true) {
                output = await da.DeleteFileStorage(id);
            } else {
                output.Messages.Add("File No Longer Exists");
            }
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/DeleteTenant/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteTenant(Guid id)
    {
        if (CurrentUser.AppAdmin) {
            var output = await da.DeleteTenant(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }


    [HttpGet]
    [Route("~/api/Data/DeleteUser/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteUser(Guid id)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        
        if (CurrentUser.Admin) {
            output = await da.DeleteUser(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/DeleteUserGroup/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteUserGroup(Guid id)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (CurrentUser.Admin) {
            output = await da.DeleteUserGroup(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/DeleteUserPhoto/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteUserPhoto(Guid id)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        
        if (CurrentUser.Admin) {
            output = await da.DeleteUserPhoto(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetApplicationSettings")]
    public ActionResult<DataObjects.ApplicationSettings> GetApplicationSettings()
    {
        if (CurrentUser.AppAdmin) {
            var output = da.GetApplicationSettings();
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetDepartment/{id}")]
    public async Task<ActionResult<DataObjects.Department>> GetDepartment(Guid id) 
    {
        DataObjects.Department output = new DataObjects.Department();

        if (CurrentUser.Enabled) {
            output = await da.GetDepartment(id);
            //output.ActionResponse.JWT = user.ActionResponse.JWT;
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetDepartments/{id}")]
    public async Task<ActionResult<List<DataObjects.Department>?>> GetDepartments(Guid id) 
    {
        List<DataObjects.Department>? output = null;
        
        if (CurrentUser.Enabled) {
            output = await da.GetDepartments(id);
        }
        
        return output;
    }

    [HttpGet]
    [Route("~/api/Data/GetDepartmentGroup/{id}")]
    public async Task<ActionResult<DataObjects.DepartmentGroup>> GetDepartmentGroup(Guid id)
    {
        if (CurrentUser.Admin) {
            var output = await da.GetDepartmentGroup(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetDepartmentGroups/{id}")]
    public async Task<ActionResult<List<DataObjects.DepartmentGroup>?>> GetDepartmentGroups(Guid id) 
    {
        List<DataObjects.DepartmentGroup>? output = null;

        if (CurrentUser.Enabled) {
            output = await da.GetDepartmentGroups(id);
        }

        return Ok(output);
    }


    [HttpGet]
    [Route("~/api/Data/GetFileStorage/{id}")]
    public async Task<ActionResult<DataObjects.FileStorage>> GetFileStorage(Guid id) 
    {
        if (CurrentUser.Admin) {
            var output = await da.GetFileStorage(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetFileStorageItems/{id}")]
    public async Task<ActionResult<List<DataObjects.FileStorage>?>> GetFileStorageItems(Guid id) 
    {
        List<DataObjects.FileStorage>? output = null;

        if (CurrentUser.Admin) {
            output = await da.GetFileStorageItems(id, true, true);
        }

        return Ok(output);
    }

    [HttpGet]
    [Route("~/api/Data/GetLanguage/{id}")]
    public ActionResult<DataObjects.Language> GetLanguage(string id)
    {
        if (CurrentUser.Admin) {
            var output = da.GetTenantLanguage(CurrentUser.TenantId, id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetNewEncryptionKey")]
    public ActionResult<DataObjects.BooleanResponse> GetNewEncryptionKey()
    {
        if (CurrentUser.AppAdmin) {
            var output = new DataObjects.BooleanResponse {
                Result = true,
                Messages = new List<string> { da.GetNewEncryptionKey() }
            };
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetSetting/")]
    public async Task<ActionResult<DataObjects.Setting>> GetSetting(string SettingName) 
    {
        DataObjects.Setting output = new DataObjects.Setting();
        
        if (CurrentUser.Admin) {
            output = await da.GetSetting(SettingName);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetSetting/{id}")]
    public ActionResult<DataObjects.TenantSettings> GetSettings(Guid id) 
    {
        DataObjects.TenantSettings output = new DataObjects.TenantSettings();

        if (CurrentUser.Enabled) {
            output = da.GetSettings(id);
        }

        return Ok(output);
    }

    [HttpGet]
    [Route("~/api/Data/GetTenant/{id}")]
    public async Task<ActionResult<DataObjects.Tenant>> GetTenant(Guid id)
    {
        DataObjects.Tenant output = new DataObjects.Tenant();

        if (CurrentUser.AppAdmin) {
            var tenant = await da.GetTenantFull(id);
            if(tenant != null) {
                output = tenant;
            }
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetTenants")]
    public async Task<ActionResult<List<DataObjects.Tenant>?>> GetTenants()
    {
        List<DataObjects.Tenant>? output = null;

        if (CurrentUser.AppAdmin) {
            output = await da.GetTenants();
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetUDFLabels/{id}")]
    public async Task<ActionResult<List<DataObjects.udfLabel>?>> GetUDFLabels(Guid id)
    {
        List<DataObjects.udfLabel>? output = null;

        if (CurrentUser.Enabled) {
            output = await da.GetUDFLabels(id);
        }

        return Ok(output);
    }

    [HttpGet]
    [Route("~/api/Data/GetUser/{id}")]
    public async Task<ActionResult<DataObjects.User>> GetUser(Guid id)
    {
        DataObjects.User output = new DataObjects.User();

        if (await da.UserCanViewUser(CurrentUser.UserId, id)) {
            output = await da.GetUser(id);
            //output.ActionResponse.JWT = user.ActionResponse.JWT;
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetUser/")]
    public async Task<ActionResult<DataObjects.User>> GetUser(string UserName)
    {
        DataObjects.User output = new DataObjects.User();
        DataObjects.User u = await da.GetUser(CurrentUser.TenantId, UserName);

        if (await da.UserCanViewUser(CurrentUser.UserId, u.UserId)) {
            output = u;
            //output.ActionResponse.JWT = user.ActionResponse.JWT;
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/GetUserByEmployeeId/")]
    public async Task<ActionResult<DataObjects.User>> GetUserByEmployeeId(DataObjects.SimplePost post)
    {
        DataObjects.User output = new DataObjects.User();

        if (!String.IsNullOrWhiteSpace(SourceToken)) {
            if (post != null && !String.IsNullOrWhiteSpace(post.SingleItem)) {
                output = await da.GetUserByEmployeeId(TenantId, post.SingleItem, false);
            } else {
                output.ActionResponse.Messages.Add("No value entered");
            }
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetUserGroup/{id}")]
    public async Task<ActionResult<DataObjects.UserGroup>> GetUserGroup(Guid id)
    {
        if (CurrentUser.Admin) {
            var output = await da.GetUserGroup(id, true);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetUserGroups")]
    public async Task<ActionResult<List<DataObjects.UserGroup>>> GetUserGroups()
    {
        if (CurrentUser.Admin) {
            var output = await da.GetUserGroups(CurrentUser.TenantId, true);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetUsers/{id}")]
    public async Task<ActionResult<List<DataObjects.User>?>> GetUsers(Guid id)
    {
        List<DataObjects.User>? output = null;
        
        if (CurrentUser.Admin) {
            output = await da.GetUsers(id);
        }

        return Ok(output);
    }

    [HttpGet]
    [Route("~/api/Data/GetUsersInDepartment/{id}")]
    public async Task<ActionResult<List<DataObjects.User>>> GetUsersInDepartment(Guid id)
    {
        List<DataObjects.User> output = new List<DataObjects.User>();

        if (CurrentUser.Admin) {
            output = await da.GetUsersInDepartment(CurrentUser.TenantId, id);
        }

        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/GetUsersFiltered/")]
    public async Task<ActionResult<DataObjects.FilterUsers>> GetUsersFiltered(DataObjects.FilterUsers filter)
    {
        if (CurrentUser.Admin) {
            var output = await da.GetUsersFiltered(filter);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpGet]
    [Route("~/api/Data/GetVersionInfo")]
    public ActionResult<DataObjects.VersionInfo> GetVersionInfo()
    {
        var output = da.VersionInfo;
        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/Encrypt/")]
    public ActionResult<DataObjects.BooleanResponse> Encrypt(DataObjects.SimplePost post)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (CurrentUser.Admin) {
            string Encrypted = da.Encrypt(post.SingleItem);
            if (String.IsNullOrWhiteSpace(Encrypted)) {
                output.Messages.Add("Error Encrypting Text");
            } else {
                output.Messages.Add(Encrypted);
                output.Result = true;
            }
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/ForgotPassword")]
    public async Task<ActionResult<DataObjects.User>> ForgotPassword(DataObjects.User user)
    {
        var output = await da.ForgotPassword(user);
        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/ForgotPasswordConfirm")]
    public async Task<ActionResult<DataObjects.User>> ForgotPasswordConfirm(DataObjects.User user)
    {
        var output = await da.ForgotPasswordConfirm(user);
        return Ok(output);
    }

    [HttpGet]
    [Route("~/api/Data/GetUserPhoto/{id}")]
    public async Task<ActionResult<DataObjects.FileStorage>> GetUserPhoto(Guid id)
    {
        DataObjects.FileStorage output = new DataObjects.FileStorage();

        if (CurrentUser.Enabled) {
            Guid? FileId = await da.GetUserPhoto(id);
            if (FileId.HasValue) {
                output = await da.GetFileStorage((Guid)FileId);
            }
        }

        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/ResetUserPassword/")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> ResetUserPassword(DataObjects.UserPasswordReset reset)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (CurrentUser.Admin || (CurrentUser.UserId == reset.UserId && CurrentUser.PreventPasswordChange == false)) {
            output = await da.ResetUserPassword(reset, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveApplicationSettings")]
    public async Task<ActionResult<DataObjects.ApplicationSettings>> SaveApplicationSettings(DataObjects.ApplicationSettings settings)
    {
        if (CurrentUser.AppAdmin) {
            var output = await da.SaveApplicationSettings(settings, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveDepartment/")]
    public async Task<ActionResult<DataObjects.Department>> SaveDepartment(DataObjects.Department department)
    {
        DataObjects.Department output = new DataObjects.Department();
        
        if (CurrentUser.Admin) {
            output = await da.SaveDepartment(department);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveDepartmentGroup/")]
    public async Task<ActionResult<DataObjects.DepartmentGroup>> SaveDepartmentGroup(DataObjects.DepartmentGroup departmentGroup)
    {
        DataObjects.DepartmentGroup output = new DataObjects.DepartmentGroup();
        
        if (CurrentUser.Admin) {
            output = await da.SaveDepartmentGroup(departmentGroup);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveDepartments/")]
    public async Task<ActionResult<List<DataObjects.Department>?>> SaveDepartments(List<DataObjects.Department> departments)
    {
        List<DataObjects.Department>? output = null;
        
        if (CurrentUser.Admin) {
            output = await da.SaveDepartments(departments);
        }

        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/SaveLanguage/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> SaveLanguage(Guid id, DataObjects.Language language)
    {
        if (CurrentUser.Admin) {
            var output = await da.SaveLanguage(id, language);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveSetting/")]
    public async Task<ActionResult<DataObjects.Setting>> SaveSetting(DataObjects.Setting setting)
    {
        DataObjects.Setting output = new DataObjects.Setting();

        if (CurrentUser.Admin) {
            output = await da.SaveSetting(setting);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveTenant")]
    public async Task<ActionResult<DataObjects.Tenant>> SaveTenant(DataObjects.Tenant tenant)
    {
        if (CurrentUser.AppAdmin) {
            var output = await da.SaveTenant(tenant);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveTenantSettings")]
    public ActionResult<DataObjects.BooleanResponse> SaveTenantSettings(DataObjects.Tenant tenant)
    {
        if (CurrentUser.Admin) {
            da.SaveTenantSettings(tenant.TenantId, tenant.TenantSettings);
            return Ok(new DataObjects.BooleanResponse { Result = true });
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveUDFLabels/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> SaveUDFLabels(Guid id, List<DataObjects.udfLabel> labels)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (CurrentUser.Admin) {
            output = await da.SaveUDFLabels(id, labels);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveUser/")]
    public async Task<ActionResult<DataObjects.User>> SaveUser(DataObjects.User user)
    {
        DataObjects.User output = new DataObjects.User();
        
        if (await da.UserCanEditUser(CurrentUser.UserId, user.UserId)) {
            output = await da.SaveUser(user);
            //output.ActionResponse.JWT = CurrentUser.ActionResponse.JWT;
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SaveUserGroup")]
    public async Task<ActionResult<DataObjects.UserGroup>> SaveUserGroup(DataObjects.UserGroup group)
    {
        if (CurrentUser.Admin) {
            var output = await da.SaveUserGroup(group);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/SignalRUpdate")]
    public async Task SignalRUpdate(DataObjects.SignalRUpdate update)
    {
        string tenantId = update.TenantId.HasValue ? ((Guid)update.TenantId).ToString() : "";

        if (!String.IsNullOrEmpty(tenantId) && _signalR != null) {
            await _signalR.Clients.Group(tenantId).SendAsync("SignalRUpdate", update);
        }
    }

    [HttpGet]
    [Route("~/api/Data/UnlockUserAccount/{id}")]
    public async Task<ActionResult<DataObjects.User>> UnlockUserAccount(Guid id)
    {
        if (CurrentUser.Admin) {
            var output = await da.UnlockUserAccount(id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }

    [HttpPost]
    [Route("~/api/Data/UploadFiles")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> UploadFiles()
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        Guid ItemId = Guid.Empty;
        string strToken = da.Request("upload-token");
        string strTenantId = da.Request("tenant-id");
        string strItemId = da.Request("upload-item-id");

        Guid TenantId = Guid.Empty;
        if (!String.IsNullOrEmpty(strTenantId)) {
            try {
                TenantId = new Guid(strTenantId);
            } catch { }
        }
        if(TenantId == Guid.Empty) {
            output.Messages.Add("Invalid TenantId");
            return Ok(output);
        }

        if (String.IsNullOrEmpty(strItemId)) {
            output.Messages.Add("No ItemId Specified");
            return Ok(output);
        }

        try {
            ItemId = new Guid(strItemId);
        } catch { }

        if (ItemId == Guid.Empty) {
            output.Messages.Add("No ItemId Specified");
            return Ok(output);
        }

        if (String.IsNullOrWhiteSpace(strToken)) {
            output.Messages.Add("Invalid Upload Token");
            return Ok(output);
        }

        DataObjects.User user = await da.GetUserFromToken(TenantId, strToken);
        if (user.Enabled == false) {
            output.Messages.Add("User Not Enabled");
            return Ok(output);
        }

        // At this point we are ready to upload the files.
        //System.Web.HttpFileCollection files = ctx.Request.Files;
        IFormFileCollection? files = null;
        if (context != null && context.Request != null && context.Request.Form != null && context.Request.Form.Files != null) {
            files = context.Request.Form.Files;
        }

        if(files != null && files.Count() > 0) {
            foreach(var file in files) {
                if(file.Length > 0) {
                    using (var inputStream = new MemoryStream()) {
                        await file.CopyToAsync(inputStream);

                        byte[] array = new byte[inputStream.Length];
                        inputStream.Seek(0, SeekOrigin.Begin);
                        inputStream.Read(array, 0, array.Length);
                        // get file name
                        string fName = file.FileName;

                        DataObjects.FileStorage fileStorage = new DataObjects.FileStorage { 
                            ActionResponse = da.GetNewActionResponse(true),
                            Bytes = array.Length,
                            Extension = System.IO.Path.GetExtension(fName),
                            FileId = Guid.Empty,
                            FileName = fName,
                            ItemId = ItemId,
                            UploadDate = DateTime.Now,
                            UserId = user.UserId,
                            Value = array
                        };

                        var resp = await da.SaveFileStorage(fileStorage);
                        if (resp.ActionResponse != null && resp.ActionResponse.Result) {
                        } else {
                            output.Messages.Add("Error Saving File '" + fName + "'");
                            if(resp.ActionResponse != null && resp.ActionResponse.Messages != null && resp.ActionResponse.Messages.Count() > 0) {
                                foreach(var msg in resp.ActionResponse.Messages) {
                                    output.Messages.Add(msg);
                                }
                            }
                        }
                    }
                }
            }
        }


        output.Result = output.Messages.Count() == 0;

        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/UploadUserPhoto")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> UploadUserPhoto()
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        string strToken = da.Request("upload-photo-token");
        string strTenantId = da.Request("upload-photo-tenantid");
        string strUserId = da.Request("upload-photo-userid");

        if (String.IsNullOrEmpty(strToken)) {
            strToken = da.Request("admin-user-photo-token");
            strTenantId = da.Request("admin-user-photo-tenantid");
            strUserId = da.Request("admin-user-photo-userid");
        }

        Guid TenantId = Guid.Empty;
        if (!String.IsNullOrEmpty(strTenantId)) {
            try {
                TenantId = new Guid(strTenantId);
            } catch { }
        }
        if (TenantId == Guid.Empty) {
            output.Messages.Add("Invalid TenantId");
            return Ok(output);
        }

        Guid UserId = Guid.Empty;
        if (!String.IsNullOrWhiteSpace(strUserId)) {
            try {
                UserId = new Guid(strUserId);
            } catch { }
        }

        if (UserId == Guid.Empty) {
            output.Messages.Add("No UserId Specified");
            return Ok(output);
        }

        if (String.IsNullOrWhiteSpace(strToken)) {
            output.Messages.Add("Invalid Upload Token");
            return Ok(output);
        }

        DataObjects.User user = await da.GetUserFromToken(TenantId, strToken);
        if (user.Enabled == false) {
            output.Messages.Add("User Not Enabled");
            return Ok(output);
        }

        if (user.Admin || user.UserId == UserId) {
            // Good user, OK to upload
        } else {
            output.Messages.Add("Access Denied");
            return Ok(output);
        }

        // At this point we are ready to upload the files.
        IFormFileCollection? files = null;
        if (context != null && context.Request != null && context.Request.Form != null && context.Request.Form.Files != null) {
            files = context.Request.Form.Files;
        }

        if (files != null && files.Count() > 0) {
            foreach (var file in files) {
                if (file.Length > 0) {
                    using (var inputStream = new MemoryStream()) {
                        await file.CopyToAsync(inputStream);

                        byte[] array = new byte[inputStream.Length];

                        if (array.Length > 5242880) {
                            output.Messages.Add("Profile Images Limited to 5MB");
                            return Ok(output);
                        }

                        inputStream.Seek(0, SeekOrigin.Begin);
                        inputStream.Read(array, 0, array.Length);
                        // get file name
                        string fName = file.FileName;

                        // Delete any existing user photo
                        await da.DeleteUserPhoto(UserId);

                        DataObjects.FileStorage fileStorage = new DataObjects.FileStorage {
                            ActionResponse = da.GetNewActionResponse(true),
                            Bytes = array.Length,
                            Extension = System.IO.Path.GetExtension(fName),
                            FileId = Guid.Empty,
                            FileName = fName,
                            ItemId = (Guid?)null,
                            UploadDate = DateTime.Now,
                            UserId = UserId,
                            Value = array
                        };

                        var resp = await da.SaveFileStorage(fileStorage);
                        if (resp.ActionResponse != null && resp.ActionResponse.Result) {
                            // Success
                        } else {
                            output.Messages.Add("Error Saving File '" + fName + "'");
                            if (resp.ActionResponse != null && resp.ActionResponse.Messages != null && resp.ActionResponse.Messages.Count() > 0) {
                                foreach (var msg in resp.ActionResponse.Messages) {
                                    output.Messages.Add(msg);
                                }
                            }
                        }
                    }
                }
            }
        }

        output.Result = output.Messages.Count() == 0;
        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/UserSignup")]
    public async Task<ActionResult<DataObjects.User>> UserSignup(DataObjects.User user)
    {
        var output = await da.UserSignup(user);
        return Ok(output);
    }

    [HttpPost]
    [Route("~/api/Data/UserSignupConfirm")]
    public async Task<ActionResult<DataObjects.User>> UserSignupConfirm(DataObjects.User user)
    {
        var output = await da.UserSignupConfirm(user);
        return Ok(output);
    }

    [HttpGet]
    [Route("~/api/Data/ValidateSelectedUserAccount/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> ValidateSelectedUserAccount(Guid id)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        
        if (CurrentUser.Enabled) {
            output = await da.ValidateSelectedUserAccount(CurrentUser.TenantId, id);
            return Ok(output);
        } else {
            return Unauthorized("Access Denied");
        }
    }
}