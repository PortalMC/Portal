using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Portal.Data;
using Portal.Extensions;
using Portal.Models;
using Portal.Models.ApiV1ProjectsViewModels;
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
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            StorageSetting storageSetting,
            IBuildService buildService,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _context = context;
            _storageSetting = storageSetting;
            _buildService = buildService;
            _logger = loggerFactory.CreateLogger<ProjectsController>();
        }

        // POST /api/v1/projects
        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> NewProject([FromBody] NewProjectViewModel newPorject)
        {
            if (newPorject == null)
            {
                return BadRequest();
            }
            // Validation
            var errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(newPorject.Name))
            {
                errors.AppendLine("Project name must not be empty.");
            }
            else if (newPorject.Name.Length > 30)
            {
                errors.AppendLine("Project name is 30 characters limit.");
            }
            if (newPorject.Description != null && newPorject.Description.Length > 200)
            {
                errors.AppendLine("Description is 200 characters limit.");
            }
            if (string.IsNullOrWhiteSpace(newPorject.MinecraftVersionId))
            {
                errors.AppendLine("Minecraft version must not be empty.");
            }
            if (string.IsNullOrWhiteSpace(newPorject.ForgeVersionId))
            {
                errors.AppendLine("Forge version must not be empty.");
            }

            var version = _context.MinecraftVersions
                .Include(v => v.ForgeVersions)
                .Where(v => v.Id == newPorject.MinecraftVersionId)
                .SelectMany(v => v.ForgeVersions, (mv, fv) => CreateVersionTuple(mv, fv))
                .FirstOrDefault(v => v.forgeVersion.Id == newPorject.ForgeVersionId);
            if (version.minecraftVersion == null || version.forgeVersion == null)
            {
                errors.AppendLine("Specified Minecraft or Forge version is not found.");
            }
            // Check valid zip is exists
            var forgeZipFile = _storageSetting.GetForgeStorageSetting().GetForgeZipFile(version.minecraftVersion, version.forgeVersion);
            if (!forgeZipFile.Exists)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Specified template of Minecraft and Forge version is not found.");
            }
            if (errors.Length != 0)
            {
                var root = new JObject
                {
                    {"success", new JValue(false)},
                    {"message", new JValue(errors.ToString())}
                };
                return new BadRequestObjectResult(root);
            }
            try
            {
                var safeUser = await _context.SafeUsers
                    .SingleOrDefaultAsync(m => m.Id == _userManager.GetUserId(HttpContext.User));
                var now = DateTime.UtcNow;
                var accessRight = await _context.AccessRights.AddAsync(new AccessRight
                {
                    User = safeUser,
                    Level = AccessRightLevel.Owner.Level
                });
                var createdProject = await _context.Projects.AddAsync(new Project
                {
                    Name = newPorject.Name,
                    Description = newPorject.Description,
                    MinecraftVersion = version.minecraftVersion,
                    ForgeVersion = version.forgeVersion,
                    CreatedAt = now,
                    UpdatedAt = now,
                    AccessRights = new List<AccessRight> {accessRight.Entity}
                });
                await _context.SaveChangesAsync();
                // Expand zip file
                var projectId = createdProject.Entity.Id;
                var targetDir = _storageSetting.GetProjectStorageSetting().GetRootDirectory().ResolveDir(projectId);
                ZipFile.ExtractToDirectory(forgeZipFile.FullName, targetDir.FullName);
                var root = new JObject
                {
                    {"success", new JValue(true)},
                    {
                        "data", new JObject
                        {
                            {"id", new JValue(projectId)}
                        }
                    }
                };
                return new OkObjectResult(root);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred in creating new project.");
                var root = new JObject
                {
                    {"success", new JValue(false)},
                    {"message", new JValue("An internal error occurred.")}
                };
                return new OkObjectResult(root);
            }
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

        private static (MinecraftVersion minecraftVersion, ForgeVersion forgeVersion) CreateVersionTuple(MinecraftVersion minecraftVersion, ForgeVersion forgeVersion)
        {
            return (minecraftVersion, forgeVersion);
        }

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