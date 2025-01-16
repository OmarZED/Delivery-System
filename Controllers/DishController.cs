using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
    public class DishController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
