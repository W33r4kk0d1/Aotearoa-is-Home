using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    public class EventController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
