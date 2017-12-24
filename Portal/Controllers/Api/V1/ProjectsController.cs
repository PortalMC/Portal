using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Portal.Data;
using Portal.Extensions;
using Portal.Models;
using Portal.Services;
using Portal.Settings;
using Portal.Utils;

namespace Portal.Controllers.Api.V1
{
    [Produces("application/json")]
    [Route("api/v1/projects")]
    public partial class ProjectsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly StorageSetting _storageSetting;
        private readonly IBuildService _buildService;

        public ProjectsController(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            StorageSetting storageSetting,
            IBuildService buildService)
        {
            _userManager = userManager;
            _context = context;
            _storageSetting = storageSetting;
            _buildService = buildService;
        }

        [HttpPatch("{uuid}")]
        [Authorize]
        public async Task<IActionResult> PatchProject(string uuid, [FromBody] Project project)
        {
            if (uuid == null || project == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var result = await _context.CanAccessToProjectWithProjectAsync(_userManager.GetUserId(HttpContext.User), uuid, false);
            if (!result.canAccess)
            {
                return NotFound();
            }
            try
            {
                result.project.UpdatedAt = DateTime.UtcNow;
                result.project.Description = project.Description;
                await _context.SaveChangesAsync();
                return new NoContentResult();
            }
            catch (Exception e)
            {
                var root = new JObject
                {
                    {"success", new JValue(false)},
                    {"message", new JValue(e.Message)}
                };
                return new OkObjectResult(root);
            }
        }

        [HttpPost("{uuid}/build")]
        [Authorize]
        public async Task<IActionResult> Build(string uuid)
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
            var result = await _context.CanAccessToProjectWithProjectAsync(userId, uuid, false, true);
            if (!result.canAccess)
            {
                return NotFound();
            }
            var buildId = result.project.BuildId + 1;
            result.project.BuildId = buildId;
            await _context.SaveChangesAsync();
            _buildService.StartBuild(result.project, userId, buildId);
            return new NoContentResult();
        }

        private static readonly string[] BlacklistElements =
        {
            ".gradle", "build", "eclipse", "gradle"
        };

        private static void CheckBlacklistElemenets(JArray array)
        {
            var deleting = array.Where(token => BlacklistElements.Contains(token["title"].Value<string>())).ToList();
            foreach (var token in deleting)
            {
                array.Remove(token);
            }
        }

        private static void AddDirectories(DirectoryInfo directory, JArray store, ref int id)
        {
            var subDirectories = directory.GetDirectories().OrderByDescending(a => a.Name);
            foreach (var subDirectory in subDirectories)
            {
                var subDirectoryStore = new JArray();
                AddDirectories(subDirectory, subDirectoryStore, ref id);
                var directoryObject = new JObject
                {
                    {"title", new JValue(subDirectory.Name)},
                    {"key", new JValue($"key_{++id}")},
                    {"folder", new JValue(true)},
                    {"children", subDirectoryStore}
                };
                store.Add(directoryObject);
            }
            var files = directory.GetFiles().OrderByDescending(a => a.Name);
            foreach (var file in files)
            {
                var fileObject = new JObject
                {
                    {"title", new JValue(file.Name)},
                    {"key", new JValue($"key_{++id}")}
                };
                store.Add(fileObject);
            }
        }
    }
}