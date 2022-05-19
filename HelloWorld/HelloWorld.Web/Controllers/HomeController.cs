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
            // da.EnsureDeleted();
            da.EnsureCreated();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Record()
        {
            HelloWorld.DataAccess da = new DataAccess("Server=localhost;Database=HelloWorld;Trusted_Connection=True;MultipleActiveResultSets=true;");
            da.EnsureCreated();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}