using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Portal.Models;
using Portal.Services;

namespace Portal.Controllers.Api.V1
{
    [Produces("application/json")]
    [Route("api/v1/minecraft")]
    public class MinecraftController : Controller
    {
        private readonly IMinecraftVersionProvider _minecraftVersionProvider;

        public MinecraftController(IMinecraftVersionProvider minecraftVersionProvider)
        {
            _minecraftVersionProvider = minecraftVersionProvider;
        }

        [HttpGet("versions/{version}")]
        public IActionResult Versions(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return BadRequest();
            }
            var minecraftVersion = _minecraftVersionProvider.GetMinecraftVersions()
                .FirstOrDefault(v => v.Version == version);
            if (minecraftVersion == default(MinecraftVersion))
            {
                return BadRequest();
            }
            var versions = new JArray();
            foreach (var forgeVersion in minecraftVersion.ForgeVersions)
            {
                versions.Add(new JObject
                {
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