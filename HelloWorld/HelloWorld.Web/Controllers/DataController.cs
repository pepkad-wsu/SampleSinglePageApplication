using HelloWorld;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HelloWorld.Web.Models;

namespace HelloWorld.Web.Controllers
{
    [ApiController]
    [Route("~/api/[controller]/[action]")]
    [Produces("application/json")]
    public partial class DataController : ControllerBase
    {
        private DataAccess da;

        public DataController()
        {
            da = new DataAccess("Server=localhost;Database=HelloWorld;Trusted_Connection=True;MultipleActiveResultSets=true;");
        }
    }
}