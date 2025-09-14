using Microsoft.AspNetCore.Mvc;

namespace APIMoiFood.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
