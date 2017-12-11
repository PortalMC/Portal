using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Extensions;
using Portal.Models;
using Portal.Models.ProjectsViewModel;
using Portal.Utils;

namespace Portal.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProjectsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /**
         * Projects/{uuid}
         * Projects/New
         * Projects
         */
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
                return View("New");
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
         * Projects/{uuid}/Editor
         */
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

        /**
         * Projects/{uuid}/Builds
         */
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

        /**
         * Projects/{uuid}/Settings
         */
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
    }
}