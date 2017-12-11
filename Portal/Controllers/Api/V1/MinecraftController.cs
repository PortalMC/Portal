using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Portal.Data;
using Portal.Models;
using Portal.Utils;

namespace Portal.Controllers.Api.V1
{
    [Produces("application/json")]
    [Route("api/v1/minecraft")]
    public class MinecraftController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MinecraftController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/v1/minecraft/versions/{versionId}
        [HttpGet("versions/{versionId}")]
        public IActionResult Versions(string versionId)
        {
            if (string.IsNullOrWhiteSpace(versionId) || !Util.IsCorrectUuid(versionId))
            {
                return BadRequest();
            }
            var minecraftVersion = _context.MinecraftVersions
                .AsNoTracking()
                .Include(v => v.ForgeVersions)
                .FirstOrDefault(v => v.Id == versionId);
            if (minecraftVersion == default(MinecraftVersion))
            {
                return BadRequest();
            }
            var versions = new JArray();
            foreach (var forgeVersion in minecraftVersion.ForgeVersions)
            {
                versions.Add(new JObject
                {
                    {"id", new JValue(forgeVersion.Id)},
                    {"name", new JValue(forgeVersion.Version)},
                    {"is_recommend", new JValue(forgeVersion.IsRecommend)}
                });
            }
            var root = new JObject
            {
                {"minecraft", new JValue(minecraftVersion.Version)},
                {"forge_versions", versions}
            };
            return new OkObjectResult(root);
        }
    }
}