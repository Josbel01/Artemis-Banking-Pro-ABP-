using Microsoft.AspNetCore.Mvc;

namespace Artemis_Banking_Pro.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Account");
        }
    }
}
