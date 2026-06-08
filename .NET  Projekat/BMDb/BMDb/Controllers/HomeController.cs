using System.Diagnostics;
using BMDb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BMDb.Controllers
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
            return View(); // Ovo otvara Views/Home/Index.cshtml
        }

        public IActionResult Glavna()
        {
            return View(); // Ovo otvara Views/Home/Glavna.cshtml
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AdminDashboard() { return View(); }
        public IActionResult Finansije() { return View(); }
        public IActionResult AdminLista() { return View(); }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
