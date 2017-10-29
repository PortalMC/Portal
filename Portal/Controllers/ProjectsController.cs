using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Services;
using Portal.Utils;

namespace Portal.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMinecraftVersionProvider _minecraftVersionProvider;

        public ProjectsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IMinecraftVersionProvider minecraftVersionProvider)
        {
            _userManager = userManager;
            _context = context;
            _minecraftVersionProvider = minecraftVersionProvider;
        }

        public async Task<IActionResult> Index(string uuid)
        {
            if (uuid == null)
            {
                // Projects Listing
                var projects = await _context.AccessRights
                    .AsNoTracking()
                    .Where(a => a.User.Id == _userManager.GetUserId(HttpContext.User))
                    .Select(a => a.Project)
                    .OrderByDescending(p => p.UpdatedAt)
                    .ToListAsync();
                return View(projects);
            }
            if (uuid == "New")
            {
                // Create new project page
                return View("New", _minecraftVersionProvider.GetMinecraftVersions());
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var accessRight = await _context.AccessRights
                .AsNoTracking()
                .Where(a => a.User.Id == _userManager.GetUserId(HttpContext.User))
                .Include(a => a.Project)
                .SingleOrDefaultAsync(a => a.Project.Id == uuid);
            if (accessRight == default(AccessRight))
            {
                return NotFound();
            }
            ViewData["Uuid"] = uuid;
            ViewData["Name"] = accessRight.Project.Name;
            return View("ProjectIndex");
        }

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
            var accessRight = await _context.AccessRights
                .AsNoTracking()
                .Where(a => a.User.Id == _userManager.GetUserId(HttpContext.User))
                .Include(a => a.Project)
                .SingleOrDefaultAsync(a => a.Project.Id == uuid);
            if (accessRight == default(AccessRight))
            {
                return NotFound();
            }
            ViewData["Uuid"] = uuid;
            ViewData["Name"] = accessRight.Project.Name;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}