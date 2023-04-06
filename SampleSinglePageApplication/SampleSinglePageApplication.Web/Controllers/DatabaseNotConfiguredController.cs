using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace SampleSinglePageApplication.Web.Controllers;

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
            cs.MySQL_Password = String.Empty;
            cs.PostgreSql_Password = String.Empty;
            cs.SqlServer_Password = String.Empty;
            return View(cs);
        }
    }

    [HttpPost]
    public IActionResult Index(DataObjects.ConnectionStringConfig csConfig)
    {
        //Try and save the connection string data and redirect to the Index page.
        string cs = String.Empty;

        if (csConfig != null) {
            if (!String.IsNullOrWhiteSpace(csConfig.DatabaseType)) {
                switch (csConfig.DatabaseType.ToUpper()) {
                    case "INMEMORY":
                        cs = "InMemory";
                        break;

                    case "MYSQL":
                        if (!String.IsNullOrWhiteSpace(csConfig.MySQL_Server) &&
                            !String.IsNullOrWhiteSpace(csConfig.MySQL_Database) &&
                            !String.IsNullOrWhiteSpace(csConfig.MySQL_User) &&
                            !String.IsNullOrWhiteSpace(csConfig.MySQL_Password)
                        ) {
                            cs = "Server=" + csConfig.MySQL_Server +
                                ";Database=" + csConfig.MySQL_Database +
                                ";User=" + csConfig.MySQL_User +
                                ";Password=" + csConfig.MySQL_Password;
                        }

                        break;

                    case "POSTGRESQL":
                        if (!String.IsNullOrWhiteSpace(csConfig.PostgreSql_Host) &&
                            !String.IsNullOrWhiteSpace(csConfig.PostgreSql_Database) &&
                            !String.IsNullOrWhiteSpace(csConfig.PostgreSql_Username) &&
                            !String.IsNullOrWhiteSpace(csConfig.PostgreSql_Password)
                        ) {
                            cs = "Host=" + csConfig.PostgreSql_Host +
                                ";Database=" + csConfig.PostgreSql_Database +
                                ";Username=" + csConfig.PostgreSql_Username +
                                ";Password=" + csConfig.PostgreSql_Password;
                        }

                        break;

                    case "SQLSERVER":
                        if (!String.IsNullOrWhiteSpace(csConfig.SqlServer_Server) &&
                            !String.IsNullOrWhiteSpace(csConfig.SqlServer_Database)) {
                            cs = "Data Source=" + csConfig.SqlServer_Server +
                                "; Initial Catalog=" + csConfig.SqlServer_Database;
                            if (csConfig.SqlServer_IntegratedSecurity) {
                                cs += "Integrated Security=true;";
                            }
                            if (csConfig.SqlServer_PersistSecurityInfo) {
                                cs += "Persist Security Info=True;";
                            }
                            if (csConfig.SqlServer_TrustServerCertificate) {
                                cs += "TrustServerCertificate=True;";
                            }
                            if (!String.IsNullOrWhiteSpace(csConfig.SqlServer_UserId)) {
                                cs += "User ID=" + csConfig.SqlServer_UserId + ";";
                            }
                            if (!String.IsNullOrWhiteSpace(csConfig.SqlServer_Password)) {
                                cs += "Password=" + csConfig.SqlServer_Password + ";";
                            }
                            cs += "MultipleActiveResultSets=True;";
                        }
                        break;

                    case "SQLITE":
                        if (!String.IsNullOrWhiteSpace(csConfig.SQLiteDatabase)) {
                            cs = "Data Source=" + csConfig.SQLiteDatabase;
                        }
                        break;
                }
            }

            if (!String.IsNullOrEmpty(cs)) {
                var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
                var json = da.StringValue(System.IO.File.ReadAllText(appSettingsPath));

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new ExpandoObjectConverter());
                jsonSettings.Converters.Add(new StringEnumConverter());

                var config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);
                if (config != null) {
                    dynamic o = (dynamic)config;
                    o.ConnectionStrings.AppData = cs;
                    o.DatabaseType = csConfig.DatabaseType;
                }

                var newJson = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);

                System.IO.File.WriteAllText(appSettingsPath, newJson);
            }
        }

        _hostLifetime.StopApplication();
        return RedirectToAction("Index", "Home");
    }
}