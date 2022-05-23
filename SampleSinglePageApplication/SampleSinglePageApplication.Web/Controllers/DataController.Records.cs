using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using SampleSinglePageApplication.Web.Hubs;

namespace SampleSinglePageApplication.Web.Controllers
{
    public partial class DataController : ControllerBase
    {
        [HttpGet]
        [Route("~/api/Data/GetRecords")]
        public async Task<ActionResult<List<DataObjects.Record>?>> GetRecords()
        {
            List<DataObjects.Record>? output = new List<DataObjects.Record>();

            if (CurrentUser.AppAdmin)
            {
                output = await da.GetRecords();
                return Ok(output);
            }
            else
            {
                return Unauthorized("Access Denied");
            }
        }
    }
}
