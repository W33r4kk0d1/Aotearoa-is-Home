using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    public class AccommodationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}