using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
