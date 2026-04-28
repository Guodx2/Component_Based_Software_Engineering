using Microsoft.AspNetCore.Mvc;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller responsible for serving the main HTML file of the React application. The Index action method returns the index.html file located in the wwwroot directory, which is the entry point for the React frontend. This allows the React application to be loaded and rendered in the client's browser when they access the root URL of the server. The PhysicalFile method is used to serve the static HTML file with the appropriate content type.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Serves the index.html file from the wwwroot directory when the root URL is accessed. This file is the main entry point for the React application, allowing it to be loaded and rendered in the client's browser. The PhysicalFile method is used to specify the path to the index.html file and set the content type to "text/html" for proper rendering.
        /// </summary>
        /// <returns>The physical file result</returns>
        public IActionResult Index()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            return PhysicalFile(filePath, "text/html");
        }
    }
}