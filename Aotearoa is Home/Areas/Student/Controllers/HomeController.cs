using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}