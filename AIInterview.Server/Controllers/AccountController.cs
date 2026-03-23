using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
