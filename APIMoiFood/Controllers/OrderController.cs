using Microsoft.AspNetCore.Mvc;

namespace APIMoiFood.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
