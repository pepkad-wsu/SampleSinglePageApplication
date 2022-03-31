using Microsoft.AspNetCore.Mvc;

namespace SampleSinglePageApplication.Web.Controllers
{
    public class FileController : Controller
    {
        private HttpContext? context;
        private IDataAccess da;

        public FileController(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor)
        {
            da = daInjection;

            if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
                da.SetHttpContext(httpContextAccessor.HttpContext);
                context = httpContextAccessor.HttpContext;
                da.SetHttpContext(context);
            }
        }

        public async Task<IActionResult> View(Guid id)
        {
            string filename = String.Empty;
            byte[]? fileContent = null;
            string mimeType = "";

            if (id != Guid.Empty && context != null) {
                DataObjects.FileStorage file = await da.GetFileStorage(id);
                if (file != null && file.ActionResponse != null && file.ActionResponse.Result) {
                    string extension = da.StringOrEmpty(file.Extension).Replace(".", "").ToLower();
                    mimeType = Utilities.GetMimeType(extension);
                    filename = da.StringOrEmpty(file.FileName);
                    fileContent = file.Value;
                }
            }

            return new FileStreamResult(new MemoryStream(fileContent), mimeType);
        }
    }
}
