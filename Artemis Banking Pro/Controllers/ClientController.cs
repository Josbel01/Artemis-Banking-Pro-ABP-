using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Artemis_Banking_Pro.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
