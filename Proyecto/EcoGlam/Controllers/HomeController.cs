using Microsoft.AspNetCore.Mvc;

namespace EcoGlam.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
