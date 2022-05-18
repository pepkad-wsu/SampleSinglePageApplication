using HelloWorld;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HelloWorld.Web.Models;

namespace HelloWorld.Web.Controllers
{
    public partial class DataController : ControllerBase
    {

        [HttpGet]
        [Route("~/api/Data/GetSources/")]
        public async Task<ActionResult<DataObjects.GetSourcesResult>> GetSources()
        {
            DataObjects.GetSourcesResult output = new DataObjects.GetSourcesResult();
      
            bool validatedCredentials = true;
            if (validatedCredentials)
            {
                output = await da.GetSources();

                return Ok(output);
            }
            else
            {
                return Unauthorized("Access Denied");
            }
        }

        [HttpGet]
        [Route("~/api/Data/GetSource/{id}")]
        public async Task<ActionResult<DataObjects.Source>> GetSource(Guid id)
        {
            DataObjects.Source output = new DataObjects.Source();
            var sourceId = id;
            bool validatedCredentials = true;
            if (validatedCredentials)
            {
                output = await da.GetSource(sourceId); 

                return Ok(output);
            }
            else
            {
                return Unauthorized("Access Denied");
            }
        }

        [HttpPost]
        [Route("~/api/Data/SaveSource/")]
        public async Task<ActionResult<DataObjects.Source>> SaveSource(DataObjects.Source source)
        {
            DataObjects.Source output = new DataObjects.Source();
            bool validatedCredentials = true;
            if (validatedCredentials)
            {
                output = await da.SaveSource(source);
                return Ok(output);
            }
            else
            {
                return Unauthorized("Access Denied");
            }
        }

        [HttpPost]
        [Route("~/api/Data/DeleteSource/{id}")]
        public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteSource(Guid sourceId)
        {
            DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
            bool validatedCredentials = true;
            if (validatedCredentials)
            {
                output = await da.DeleteSource(sourceId);
                return Ok(output);
            }
            else
            {
                return Unauthorized("Access Denied");
            }
        }
    }
}