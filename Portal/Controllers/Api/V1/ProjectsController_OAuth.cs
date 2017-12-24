using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Portal.Extensions;
using Portal.Settings.ArtifactStorage;
using Portal.Utils;

namespace Portal.Controllers.Api.V1
{
    public partial class ProjectsController
    {
        // GET /api/v1/projects
        [HttpGet("")]
        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Projects([FromQuery] string minecraft)
        {
            var user = _userManager.GetUserId(HttpContext.User);
            // TODO* Pagination
            object[] projects = await _context.Projects
                .AsNoTracking()
                .Where(p => p.AccessRights.Any(a => a.User.Id == user))
                .Where(p => string.IsNullOrEmpty(minecraft) || p.MinecraftVersion.Version == minecraft)
                .Select(p => new JObject
                    {
                        {"id", new JValue(p.Id)},
                        {"name", new JValue(p.Name)},
                        {"minecraftVersion", new JValue(p.MinecraftVersion.Version)},
                        {"forgeVersion", new JValue(p.ForgeVersion.Version)}
                    }
                )
                .ToArrayAsync();
            var root = new JObject
            {
                {"success", new JValue(true)},
                {"data", new JArray(projects)}
            };
            return new OkObjectResult(root);
        }

        [HttpGet("{uuid}/artifact")]
        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Artifact(string uuid)
        {
            if (uuid == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var userId = _userManager.GetUserId(HttpContext.User);
            var result = await _context.CanAccessToProjectWithProjectAsync(userId, uuid);
            if (!result.canAccess)
            {
                return NotFound();
            }
            switch (_storageSetting.GetArtifactStorageSetting().GetArtifactProvideMethod())
            {
                case ArtifactProvideMethod.Stream:
                    var stream = await _storageSetting.GetArtifactStorageSetting().GetArtifactStreamAsync(uuid, result.project.BuildId);
                    return File(stream, "application/octet-stream");
                case ArtifactProvideMethod.Redirect:
                    var uri = await _storageSetting.GetArtifactStorageSetting().GetArtifactRedirectUriAsync(uuid, result.project.BuildId);
                    return Redirect(uri);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError, "ArtifactProvideMethod is out of range.");
            }
        }
    }
}