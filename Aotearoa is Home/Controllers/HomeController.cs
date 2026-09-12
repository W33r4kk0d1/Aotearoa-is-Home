using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}