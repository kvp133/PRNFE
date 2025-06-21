using Microsoft.AspNetCore.Mvc;

namespace PRNFE.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 