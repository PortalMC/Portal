using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Services;

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
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                var user = await _context.SafeUsers
                    .Include(s => s.Projects)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(m => m.UserId == currentUser.Id);
                return View(user);
            }
            if (uuid == "New")
            {
                // Create new project page
                return View("New", _minecraftVersionProvider.GetMinecraftVersions());
            }
            if (!Utils.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            ViewData["Uuid"] = uuid;
            return View("ProjectIndex");
        }

        public IActionResult Editor(string uuid)
        {
            if (uuid == null)
            {
                return RedirectToAction("Index");
            }
            if (!Utils.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            ViewData["Uuid"] = uuid;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}