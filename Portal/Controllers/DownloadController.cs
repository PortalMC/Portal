using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Portal.Controllers
{
    public class DownloadController : Controller
    {
        public IActionResult Index(string version)
        {
            if (version != null)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "coremods", $"{version}.jar");
                return PhysicalFile(path, "application/java-archive", $"portal-core-{version}.jar");
            }
            return View();
        }

        public IActionResult Installation()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}