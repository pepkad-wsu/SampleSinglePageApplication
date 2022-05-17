using HelloWorld;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HelloWorld.Web.Models;

namespace HelloWorld.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            HelloWorld.DataAccess da = new DataAccess("Server=localhost;Database=HelloWorld;Trusted_Connection=True;MultipleActiveResultSets=true;");

            var results = da.GetSources();

            ViewBag.Sources = results;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}