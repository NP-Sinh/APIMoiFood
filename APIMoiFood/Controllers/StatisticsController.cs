using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsController : Controller
    {
        
    }
}
