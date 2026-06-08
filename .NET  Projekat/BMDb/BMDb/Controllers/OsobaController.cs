using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BMDb.Data; 
using BMDb.Models; 

namespace BMDb.Controllers
{
    public class OsobaController : Controller
    {
        private readonly UserManager<Osoba> _userManager;
        private readonly ApplicationDbContext _context;


        public OsobaController(UserManager<Osoba> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Details()
        {
  
            return View();
        }


        public IActionResult Index()
        {

            return View();
        }
    }
}