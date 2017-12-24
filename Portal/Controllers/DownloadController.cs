using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Portal.Data;
using Portal.Settings;
using Portal.Utils;

namespace Portal.Controllers
{
    public class DownloadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StorageSetting _storageSetting;

        public DownloadController(
            ApplicationDbContext context,
            StorageSetting storageSetting)
        {
            _context = context;
            _storageSetting = storageSetting;
        }

        public async Task<IActionResult> Index(string version, string type)
        {
            if (version != null)
            {
                if (!Util.IsCorrectUuid(version))
                {
                    return NotFound();
                }
                var minecraftVersion = await _context.MinecraftVersions.FindAsync(version);
                switch (type)
                {
                    case "coremod":
                    {
                        var path = _storageSetting.GetCoremodStorageSetting().GetCoremodFile(minecraftVersion).FullName;
                        return PhysicalFile(path, "application/java-archive", $"portal-core-{minecraftVersion.Version}.jar");
                    }
                    case "property":
                    {
                        var path = _storageSetting.GetCoremodStorageSetting().GetPropertyFile(minecraftVersion).FullName;
                        return PhysicalFile(path, "text/plain", "portal.properties");
                    }
                }
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