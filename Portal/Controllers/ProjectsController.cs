using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Data;
using Portal.Extensions;
using Portal.Models;
using Portal.Models.ProjectsViewModel;
using Portal.Settings;
using Portal.Utils;

namespace Portal.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly StorageSetting _storageSetting;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            StorageSetting storageSetting,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _context = context;
            _storageSetting = storageSetting;
            _logger = loggerFactory.CreateLogger<ProjectsController>();
        }

        // Projects/{uuid}
        // Projects/New
        // Projects
        [HttpGet]
        public async Task<IActionResult> Index(string uuid)
        {
            if (uuid == null)
            {
                var user = _userManager.GetUserId(HttpContext.User);
                // Projects Listing
                var projects = _context.Projects
                    .Include(p => p.MinecraftVersion)
                    .Include(p => p.ForgeVersion)
                    .Where(p => p.AccessRights.Any(a => a.User.Id == user))
                    .OrderByDescending(p => p.UpdatedAt);
                return View(new IndexViewModel
                {
                    Projects = projects
                });
            }
            if (uuid == "New")
            {
                // Create new project page
                return View("New", new NewProjectViewModel());
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
            ViewBag.Uuid = uuid;
            ViewBag.Name = result.project.Name;
            ViewBag.Description = result.project.Description;
            return View("ProjectIndex");
        }

        /**
         * Projects/New
         */
        [HttpPost]
        public async Task<IActionResult> Index(string uuid, NewProjectViewModel newPorject)
        {
            if (uuid == "New")
            {
                if (newPorject == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return View("New", newPorject);
                }

                var version = _context.MinecraftVersions
                    .Include(v => v.ForgeVersions)
                    .Where(v => v.Id == newPorject.MinecraftVersionId)
                    .SelectMany(v => v.ForgeVersions, (mv, fv) => CreateVersionTuple(mv, fv))
                    .FirstOrDefault(v => v.forgeVersion.Id == newPorject.ForgeVersionId);
                if (version.minecraftVersion == null || version.forgeVersion == null)
                {
                    return BadRequest();
                }
                // Check valid zip is exists
                var forgeZipFile = _storageSetting.GetForgeStorageSetting().GetForgeZipFile(version.minecraftVersion, version.forgeVersion);
                if (!forgeZipFile.Exists)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Specified template of Minecraft and Forge version is not found.");
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
                    return RedirectToAction(nameof(Index), "Projects", new RouteValueDictionary
                    {
                        {"uuid", projectId}
                    });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred in creating new project.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
                }
            }
            return NotFound();
        }

        // Projects/{uuid}/Editor
        [HttpGet]
        public async Task<IActionResult> Editor(string uuid)
        {
            if (uuid == null)
            {
                return RedirectToAction("Index");
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
            ViewBag.Uuid = uuid;
            ViewBag.Name = result.project.Name;
            return View();
        }

        // Projects/{uuid}/Builds
        [HttpGet]
        public async Task<IActionResult> Builds(string uuid)
        {
            if (uuid == null)
            {
                return RedirectToAction("Index");
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
            ViewBag.Uuid = uuid;
            ViewBag.Name = result.project.Name;
            return View();
        }

        // Projects/{uuid}/Settings
        [HttpGet]
        public async Task<IActionResult> Settings(string uuid)
        {
            if (uuid == null)
            {
                return RedirectToAction("Index");
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
            ViewBag.Uuid = uuid;
            ViewBag.Name = result.project.Name;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        private static (MinecraftVersion minecraftVersion, ForgeVersion forgeVersion) CreateVersionTuple(MinecraftVersion minecraftVersion, ForgeVersion forgeVersion)
        {
            return (minecraftVersion, forgeVersion);
        }
    }
}