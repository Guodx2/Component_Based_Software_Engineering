using Microsoft.AspNetCore.Mvc;

namespace NKDUPVS_React.Server.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            return PhysicalFile(filePath, "text/html");
        }
    }
}