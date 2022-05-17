using HelloWorld;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HelloWorld.Web.Models;

namespace HelloWorld.Web.Controllers
{
    [ApiController]
    [Route("~/api/[controller]/[action]")]
    [Produces("application/json")]
    public class DataController : Controller
    {
        private DataAccess da;

        public DataController()
        {
            da = new DataAccess("Server=localhost;Database=HelloWorld;Trusted_Connection=True;MultipleActiveResultSets=true;");
        }

        [HttpGet]
        [Route("~api/Data/GetSources/")]
        public ActionResult<List<DataObjects.Source>> GetSources()
        {
            List<DataObjects.Source> output = new List<DataObjects.Source>();
            bool validatedCredentials = true;
            if (validatedCredentials)
            {
                output = da.GetSources();
                return Ok(output);
            }
            else
            {
                return Unauthorized("Access Denied");
            }

            return output;
        }

        [HttpGet]
        [Route("~api/Data/GetSource/{id}")]
        public ActionResult<DataObjects.Source> GetSource(Guid id)
        {
           DataObjects.Source output = new DataObjects.Source();
            var sourceId = id;
            bool validatedCredentials = true;
            if (validatedCredentials)
            {
                //output = da.GetSource(); TODO: Write GetSource()
                return Ok(output);
            }
            else
            {
                return Unauthorized("Access Denied");
            }

            return output;
        }

        //[HttpPost]
        //[Route("~api/Data/SaveSource/{id}")]
        //public ActionResult<DataObjects.Source> SaveSource(DataObjects.Source source)
        //{
        //    DataObjects.Source output = new DataObjects.Source();
        //    var sourceId = id;
        //    bool validatedCredentials = true;
        //    if (validatedCredentials)
        //    {
        //        //output = da.GetSource(); TODO: Write GetSource()
        //        return Ok(output);
        //    }
        //    else
        //    {
        //        return Unauthorized("Access Denied");
        //    }

        //    return output;
        //}
    }
}