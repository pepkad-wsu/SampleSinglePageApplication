using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace SampleSinglePageApplication.Web.Controllers
{
    public class DatabaseNotConfiguredController : Controller
    {
        private IConfiguration _config;
        private IDataAccess da;
        private IHostApplicationLifetime _hostLifetime;

        public DatabaseNotConfiguredController(IConfiguration config, IDataAccess daInjection, IHostApplicationLifetime hostLifetime)
        {
            _config = config;
            da = daInjection;
            _hostLifetime = hostLifetime;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Only allow access to this page if we are unable to open the data connection
            if (da.DatabaseOpen) {
                return RedirectToAction("Index", "Home");
            } else {
                var cs = da.GetConnectionStringConfig();

                // Clear the password value
                cs.Password = "";
                return View(cs);
            }
        }

        [HttpPost]
        public IActionResult Index(DataObjects.ConnectionStringConfig csConfig)
        {
            //Try and save the connection string data and redirect to the Index page.
            if (csConfig != null) {
                string cs =
                    "Data Source=" + csConfig.Server + ";" +
                    "Initial Catalog=" + csConfig.Database + ";" +
                    "Persist Security Info=True;" +
                    "User ID=" + csConfig.UserId + ";" +
                    "Password=" + csConfig.Password + ";" +
                    "MultipleActiveResultSets=True";

                var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
                var json = System.IO.File.ReadAllText(appSettingsPath);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new ExpandoObjectConverter());
                jsonSettings.Converters.Add(new StringEnumConverter());

                dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);
                config.ConnectionStrings.AppData = cs;

                var newJson = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);

                System.IO.File.WriteAllText(appSettingsPath, newJson);
                _hostLifetime.StopApplication();
            }

            return RedirectToAction("Index", "Home");
        }
    }

}
