using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Portal.Data;
using Portal.Extensions;
using Portal.Models;
using Portal.Services;
using Portal.Settings;
using Portal.Settings.ArtifactStorage;
using Portal.Utils;

namespace Portal.Controllers.Api.V1
{
    [Produces("application/json")]
    [Route("api/v1/projects")]
    public class ProjectsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMinecraftVersionProvider _minecraftVersionProvider;
        private readonly StorageSetting _storageSetting;
        private readonly IBuildService _buildService;

        public ProjectsController(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IMinecraftVersionProvider minecraftVersionProvider,
            StorageSetting storageSetting,
            IBuildService buildService)
        {
            _userManager = userManager;
            _context = context;
            _minecraftVersionProvider = minecraftVersionProvider;
            _storageSetting = storageSetting;
            _buildService = buildService;
        }

        [HttpGet("")]
        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> NewProject([FromQuery] string minecraft)
        {
            // TODO* Pagination
            object[] projects = await _context.AccessRights
                .AsNoTracking()
                .Where(a => a.User.Id == _userManager.GetUserId(HttpContext.User))
                .Select(a => a.Project)
                .Where(p => string.IsNullOrEmpty(minecraft) || p.MinecraftVersion == minecraft)
                .Select(p => new JObject
                    {
                        {"id", new JValue(p.Id)},
                        {"name", new JValue(p.Name)},
                        {"minecraftVersion", new JValue(p.MinecraftVersion)},
                        {"forgeVersion", new JValue(p.ForgeVersion)}
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

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> NewProject([FromBody] Project project)
        {
            if (project == null)
            {
                return BadRequest();
            }
            // Validation
            var errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(project.Name))
            {
                errors.AppendLine("Project name must not be empty.");
            }
            else if (project.Name.Length > 30)
            {
                errors.AppendLine("Project name is 30 characters limit.");
            }
            if (project.Description != null && project.Description.Length > 200)
            {
                errors.AppendLine("Description is 200 characters limit.");
            }
            if (string.IsNullOrWhiteSpace(project.MinecraftVersion))
            {
                errors.AppendLine("Minecraft version must not be empty.");
            }
            if (string.IsNullOrWhiteSpace(project.ForgeVersion))
            {
                errors.AppendLine("Forge version must not be empty.");
            }
            var minecraftVersion = _minecraftVersionProvider.GetMinecraftVersions()
                .FirstOrDefault(v => v.Version == project.MinecraftVersion);
            ForgeVersion forgeVersion = null;
            if (minecraftVersion == default(MinecraftVersion))
            {
                errors.AppendLine("Specified Minecraft version is not found.");
            }
            else
            {
                forgeVersion =
                    minecraftVersion.ForgeVersions.FirstOrDefault(v => v.Version == project.ForgeVersion);
                if (forgeVersion == default(ForgeVersion))
                {
                    errors.AppendLine("Specified Forge version is not found.");
                }
            }
            // Check valid zip is exists
            var forgeZipFile = _minecraftVersionProvider.GetForgeZipFile(minecraftVersion, forgeVersion);
            if (!forgeZipFile.Exists)
            {
                errors.AppendLine("Specified template of Minecraft and Forge version is not found.");
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
                project.UpdatedAt = project.CreatedAt = DateTime.UtcNow;
                var createdProject = await _context.Projects.AddAsync(project);
                var ownerAccessRight = new AccessRight
                {
                    User = safeUser,
                    Project = createdProject.Entity,
                    Level = AccessRightLevel.Owner.Level
                };
                await _context.AccessRights.AddAsync(ownerAccessRight);
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
                var root = new JObject
                {
                    {"success", new JValue(false)},
                    {"message", new JValue(e.Message)}
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
            var result = await _context.CanAccessToProjectWithProjectAsync(userId, uuid);
            if (!result.canAccess)
            {
                return NotFound();
            }
            _buildService.StartBuild(result.project, userId);
            return new NoContentResult();
        }

        [HttpPost("{uuid}/file/get")]
        [Authorize]
        public async Task<IActionResult> Get(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var canAccess = await _context.CanAccessToProjectAsync(_userManager.GetUserId(HttpContext.User), uuid);
            if (!canAccess)
            {
                return NotFound();
            }
            var projectRoot = Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid);
            var rawFile = new FileInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
            if (!rawFile.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                // Maybe directory traversal
                return NotFound();
            }
            if (!rawFile.Exists)
            {
                return NotFound();
            }
            var relativePath = rawFile.FullName.Substring(projectRoot.Length + 1).Replace("\\", "/");
            var extension = rawFile.Extension;
            if (extension.Length != 0)
            {
                extension = extension.Substring(1);
            }

            var retFile = new FileObject
            {
                Path = relativePath,
                Name = rawFile.Name,
                Extension = extension,
                Content = System.IO.File.ReadAllText(rawFile.FullName, Encoding.UTF8)
            };
            return new ObjectResult(retFile);
        }

        [HttpPost("{uuid}/file/edit")]
        [Authorize]
        public async Task<IActionResult> Edit(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var canAccess = await _context.CanAccessToProjectAsync(_userManager.GetUserId(HttpContext.User), uuid);
            if (!canAccess)
            {
                return NotFound();
            }
            var projectRoot = Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid);
            var rawFile = new FileInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
            if (!rawFile.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                // Maybe directory traversal
                return NotFound();
            }
            if (!rawFile.Exists)
            {
                return NotFound();
            }

            try
            {
                await System.IO.File.WriteAllTextAsync(rawFile.FullName, file.Content, Encoding.UTF8);
                await _context.UpdateProjectUpdatedAtAsync(uuid);
                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet("{uuid}/file/list")]
        [Authorize]
        public async Task<IActionResult> FileList(string uuid)
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
            var projectRoot = new DirectoryInfo(Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid));
            if (!projectRoot.Exists)
            {
                return BadRequest();
            }
            var projectRootObj = new JArray();
            var id = 0;
            AddDirectories(projectRoot, projectRootObj, ref id);
            CheckBlacklistElemenets(projectRootObj);
            var root = new JArray
            {
                new JObject
                {
                    {"title", new JValue(result.project.Name)},
                    {"folder", new JValue(true)},
                    {"expanded", new JValue(true)},
                    {"children", projectRootObj}
                }
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
                    var stream = await _storageSetting.GetArtifactStorageSetting().GetArtifactStreamAsync(uuid, "1");
                    return File(stream, "application/octet-stream");
                case ArtifactProvideMethod.Redirect:
                    var uri = await _storageSetting.GetArtifactStorageSetting().GetArtifactRedirectUriAsync(uuid, "1");
                    return Redirect(uri);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError, "ArtifactProvideMethod is out of range.");
            }
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