using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Portal.Data;
using Portal.Extensions;
using Portal.Models;
using Portal.Models.AdminViewModels;
using Portal.Services;
using Portal.Settings;
using Portal.Utils;

namespace Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly StorageSetting _storageSetting;
        private readonly ILogger<AccountController> _logger;

        public AdminController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            StorageSetting storageSetting,
            ILoggerFactory loggerFactory)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _storageSetting = storageSetting;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        // GET /Admin
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        // GET /Admin/Users
        // GET /Admin/Users/{uuid}
        // GET /Admin/Users/{uuid}/New
        // GET /Admin/Users/{uuid}/Login
        // GET /Admin/Users/{uuid}/ResetPassword
        // GET /Admin/Users/{uuid}/Delete
        [HttpGet("Users/{id?}/{subaction?}")]
        public async Task<IActionResult> Users(string id, string subaction, string message = null)
        {
            ViewBag.Message = message;
            if (id == "New")
            {
                return View("NewUser");
            }
            if (Util.IsCorrectUuid(id))
            {
                var user = await _userManager.FindByIdAsync(id);
                switch (subaction)
                {
                    case null:
                        var safeUser = await _context.SafeUsers.AsNoTracking().Where(u => u.Id == id).FirstAsync();
                        var roles = await _userManager.GetRolesAsync(user);
                        return View("UserDetail", new UserDetailViewModel
                        {
                            User = user,
                            SafeUser = safeUser,
                            Roles = roles
                        });
                    case "Login":
                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    case "ResetPassword":
                        return View("UserDetailResetPassword", new UserDetailPostViewModel
                        {
                            User = user,
                            ResetPasswordViewModel = new UserDetailResetPasswordViewModel()
                        });
                    case "Delete":
                        return View("UserDetailDelete", new UserDetailPostViewModel
                        {
                            User = user,
                            DeleteViewModel = new UserDetailDeleteViewModel()
                        });
                    default:
                        return NotFound();
                }
            }
            var applicationUsers = _context.Users.AsNoTracking().Take(10).ToArray();
            return View(applicationUsers);
        }

        // POST /Admin/Users/{uuid}/New
        // POST /Admin/Users/{uuid}/ResetPassword
        // POST /Admin/Users/{uuid}/Delete
        [HttpPost("Users/{id?}/{subaction?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Users(string id, string subaction, UserDetailPostViewModel model, string returnUrl = null)
        {
            if (id == "New")
            {
                ViewBag.ReturnUrl = returnUrl;
                if (!ModelState.IsValid) return View(model);
                try
                {
                    var ret = await _userManager.CreateWithPortalAsync(_context, model.NewUserViewModel.Email, model.NewUserViewModel.Password);
                    if (ret.result.Succeeded)
                    {
                        _logger.LogInformation(3, "New account is created by admin.");
                        return RedirectToLocal(returnUrl, new RouteValueDictionary {{"id", ""}});
                    }
                    AddErrors(ret.result);
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.
                    ModelState.AddModelError("", "Unable to save changes. " +
                                                 "Try again, and if the problem persists " +
                                                 "see your system administrator.");
                }
                return View(model);
            }
            if (Util.IsCorrectUuid(id))
            {
                if (!ModelState.IsValid) return View(model);
                switch (subaction)
                {
                    case "ResetPassword":
                    {
                        var user = await _userManager.FindByIdAsync(id);
                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await _userManager.ResetPasswordAsync(user, code, model.ResetPasswordViewModel.Password);
                        if (result.Succeeded)
                        {
                            return RedirectToAction(nameof(Users), "Admin", new RouteValueDictionary
                            {
                                {"id", id},
                                {"subaction", ""},
                                {"message", "success_change_password"}
                            });
                        }
                        AddErrors(result);
                        return View();
                    }
                    case "Delete":
                    {
                        var user = await _userManager.FindByIdAsync(id);
                        var safeUser = await _context.SafeUsers.Where(u => u.Id == id).FirstAsync();
                        _context.SafeUsers.Remove(safeUser);
                        await _context.SaveChangesAsync();
                        var resultDelete = await _userManager.DeleteAsync(user);
                        if (resultDelete.Succeeded)
                        {
                            return RedirectToAction(nameof(Users), "Admin", new RouteValueDictionary
                            {
                                {"id", ""},
                                {"subaction", ""},
                                {"message", "success_delete_user"}
                            });
                        }
                        AddErrors(resultDelete);
                        return View();
                    }
                    default:
                        return NotFound();
                }
            }
            return NotFound();
        }

        // GET /Admin/Snippet
        // GET /Admin/Snippet/{uuid}/Edit
        [HttpGet("Snippet/{id?}/{subaction?}")]
        public async Task<IActionResult> Snippet(string id, string subaction, string message = null)
        {
            ViewBag.Message = message;
            if (Util.IsCorrectUuid(id))
            {
                var snippet = await _context.Snippets.Include(s => s.Group).FirstAsync(s => s.Id == id);
                switch (subaction)
                {
                    case "Edit":
                        return View("EditSnippet", snippet);
                    default:
                        return NotFound();
                }
            }
            if (id != null)
            {
                return NotFound();
            }
            return View();
        }

        // POST /Admin/Snippet/{uuid}/Edit
        [HttpPost("Snippet/{id}/{subaction}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Snippet(string id, string subaction, Snippet snippet)
        {
            if (Util.IsCorrectUuid(id))
            {
                switch (subaction)
                {
                    case "Edit":
                        var s = await _context.Snippets.FindAsync(id);
                        s.Content = snippet.Content;
                        _context.Snippets.Update(s);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Snippet), "Admin", new RouteValueDictionary
                        {
                            {"id", ""},
                            {"subaction", ""},
                            {"message", "success_save_snippet"}
                        });
                    default:
                        return NotFound();
                }
            }
            return NotFound();
        }

        // GET /Admin/Versions
        // GET /Admin/Versions/New
        // GET /Admin/Versions/{uuid:minecraftVersion}
        // GET /Admin/Versions/{uuid:minecraftVersion}/Up
        // GET /Admin/Versions/{uuid:minecraftVersion}/Down
        // GET /Admin/Versions/{uuid:minecraftVersion}/DockerImageVersion
        // GET /Admin/Versions/{uuid:minecraftVersion}/{uuid:forgeVersion}/Up
        // GET /Admin/Versions/{uuid:minecraftVersion}/{uuid:forgeVersion}/Down
        // GET /Admin/Versions/{uuid:minecraftVersion}/{uuid:forgeVersion}/Download
        // GET /Admin/Versions/{uuid:minecraftVersion}/{uuid:forgeVersion}/Edit
        [HttpGet("Versions/{id?}/{subaction?}/{subsubaction?}")]
        public async Task<IActionResult> Versions(string id, string subaction, string subsubaction, string message = null)
        {
            ViewBag.Message = message;
            if (Util.IsCorrectUuid(id))
            {
                switch (subaction)
                {
                    case null:
                    {
                        var minecraftVersion = await _context.MinecraftVersions
                            .AsNoTracking()
                            .Include(v => v.ForgeVersions)
                            .FirstOrDefaultAsync(v => v.Id == id);
                        var forgeVersions = minecraftVersion.ForgeVersions
                            .OrderBy(v => v.Rank)
                            .ThenByDescending(v => v.UpdatedAt);
                        return View("VersionDetail", new VersionDetailViewModel
                        {
                            MinecraftVersion = minecraftVersion,
                            ForgeVersions = forgeVersions
                        });
                    }
                    case "Up":
                    {
                        var minecraftVersion = await _context.MinecraftVersions.FindAsync(id);
                        minecraftVersion.Rank--;
                        minecraftVersion.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Versions), "Admin", new RouteValueDictionary
                        {
                            {"id", ""},
                            {"subaction", ""},
                            {"subsubaction", ""}
                        });
                    }
                    case "Down":
                    {
                        var minecraftVersion = await _context.MinecraftVersions.FindAsync(id);
                        minecraftVersion.Rank++;
                        minecraftVersion.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Versions), "Admin", new RouteValueDictionary
                        {
                            {"id", ""},
                            {"subaction", ""},
                            {"subsubaction", ""}
                        });
                    }
                    case "DockerImageVersion":
                    {
                        var minecraftVersion = await _context.MinecraftVersions.FindAsync(id);
                        return View("VersionDetailDockerImageVersion", new VersionsPostViewModel
                        {
                            DockerImageVersionViewModel = new VersionDetailDockerImageVersionViewModel
                            {
                                MinecraftVersion = minecraftVersion,
                                Version = minecraftVersion.DockerImageVersion
                            }
                        });
                    }
                    case "PullDockerImage":
                    {
                        var minecraftVersion = await _context.MinecraftVersions.FindAsync(id);
                        var buildService = HttpContext.RequestServices.GetRequiredService<IBuildService>();
                        if (!(buildService is DockerBuildService))
                        {
                            return NotFound();
                        }
                        await (buildService as DockerBuildService).CheckImageAsync(_context, minecraftVersion, CancellationToken.None);
                        return RedirectToAction(nameof(Versions), "Admin", new RouteValueDictionary
                        {
                            {"id", id},
                            {"subaction", ""},
                            {"message", "success_pull_image"}
                        });
                    }
                    default:
                    {
                        return await ForgeVersion(id, subaction, subsubaction);
                    }
                }
            }
            if (id == "New")
            {
                return View("NewVersion", new VersionsPostViewModel
                {
                    NewVersionViewModel = new NewVersionViewModel()
                });
            }
            if (id != null)
            {
                return NotFound();
            }
            var minecraftVersions = _context.MinecraftVersions
                .AsNoTracking()
                .Include(v => v.ForgeVersions)
                .OrderBy(v => v.Rank)
                .ThenByDescending(v => v.UpdatedAt);
            return View(new VersionsIndexViewModel
            {
                MinecraftVersions = minecraftVersions
            });
        }

        private async Task<IActionResult> ForgeVersion(string minecraftVersionId, string forgeVersionId, string action)
        {
            var minecraftVersion = await _context.MinecraftVersions.FindAsync(minecraftVersionId);
            if (minecraftVersion == null)
            {
                return NotFound();
            }
            if (!Util.IsCorrectUuid(forgeVersionId))
            {
                return NotFound();
            }
            switch (action)
            {
                case "Up":
                {
                    var forgeVersion = await _context.ForgeVersions.FindAsync(forgeVersionId);
                    forgeVersion.Rank--;
                    forgeVersion.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Versions), "Admin", new RouteValueDictionary
                    {
                        {"id", minecraftVersionId},
                        {"subaction", ""},
                        {"subsubaction", ""}
                    });
                }
                case "Down":
                {
                    var forgeVersion = await _context.ForgeVersions.FindAsync(forgeVersionId);
                    forgeVersion.Rank++;
                    forgeVersion.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Versions), "Admin", new RouteValueDictionary
                    {
                        {"id", minecraftVersionId},
                        {"subaction", ""},
                        {"subsubaction", ""}
                    });
                }
                case "Download":
                {
                    var forgeZipFile = _storageSetting.GetForgeStorageSetting()
                        .GetForgeZipFile(await _context.MinecraftVersions.FindAsync(minecraftVersionId), await _context.ForgeVersions.FindAsync(forgeVersionId));
                    return PhysicalFile(forgeZipFile.FullName, MediaTypeNames.Application.Zip, forgeZipFile.Name);
                }
                case "Edit":
                {
                    var forgeVersion = await _context.ForgeVersions.FindAsync(forgeVersionId);
                    return View("VersionDetailForgeEdit", new VersionsPostViewModel
                    {
                        ForgeEditViewModel = new VersionDetailForgeEditViewModel
                        {
                            MinecraftVersion = minecraftVersion,
                            Id = forgeVersionId,
                            Version = forgeVersion.Version,
                            IsRecommend = forgeVersion.IsRecommend
                        }
                    });
                }
            }
            return NotFound();
        }

        // POST /Admin/Versions/New
        // POST /Admin/Versions/{uuid:minecraftVersionId}/DockerImageVersion
        // POST /Admin/Versions/{uuid:minecraftVersionId}/{uuid:forgeVersionId}/Edit
        [HttpPost("Versions/{id?}/{subaction?}/{subsubaction?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Versions(string id, string subaction, string subsubaction, VersionsPostViewModel model)
        {
            switch (id)
            {
                case "New":
                {
                    if (await _context.MinecraftVersions.AnyAsync(v => v.Version == model.NewVersionViewModel.Version))
                    {
                        return BadRequest();
                    }
                    var maxRank = await _context.MinecraftVersions
                        .AsNoTracking()
                        .OrderByDescending(v => v.Rank)
                        .Select(v => v.Rank + 1)
                        .FirstOrDefaultAsync();
                    var now = DateTime.UtcNow;
                    var entry = await _context.MinecraftVersions.AddAsync(new MinecraftVersion
                    {
                        Version = model.NewVersionViewModel.Version,
                        DockerImageVersion = model.NewVersionViewModel.DockerImageVersion,
                        ForgeVersions = new List<ForgeVersion>(),
                        CreatedAt = now,
                        UpdatedAt = now,
                        Rank = maxRank
                    });
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Versions), "Admin", new RouteValueDictionary
                    {
                        {"id", entry.Entity.Id},
                        {"subaction", ""},
                        {"message", "success_create"}
                    });
                }
            }
            if (!Util.IsCorrectUuid(id))
            {
                return NotFound();
            }
            var minecraftVersion = await _context.MinecraftVersions.FindAsync(id);
            switch (subaction)
            {
                case "DockerImageVersion":
                {
                    minecraftVersion.DockerImageVersion = model.DockerImageVersionViewModel.Version;
                    _context.MinecraftVersions.Update(minecraftVersion);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Versions), "Admin", new RouteValueDictionary
                    {
                        {"id", id},
                        {"subaction", ""},
                        {"message", "success_save"}
                    });
                }
                default:
                {
                    return await ForgeVersion(id, subaction, subsubaction, model);
                }
            }
        }

        private async Task<IActionResult> ForgeVersion(string minecraftVersionId, string forgeVersionId, string action, VersionsPostViewModel model)
        {
            var minecraftVersion = await _context.MinecraftVersions.FindAsync(minecraftVersionId);
            if (minecraftVersion == null)
            {
                return NotFound();
            }
            if (!Util.IsCorrectUuid(forgeVersionId))
            {
                return NotFound();
            }
            switch (action)
            {
                case "Edit":
                {
                    var forgeVersion = await _context.ForgeVersions.FindAsync(forgeVersionId);
                    if (forgeVersion == null)
                    {
                        return NotFound();
                    }
                    forgeVersion.Version = model.ForgeEditViewModel.Version;
                    forgeVersion.IsRecommend = model.ForgeEditViewModel.IsRecommend;
                    // File check
                    if (model.ForgeEditViewModel.ForgeZipFile != null)
                    {
                        var file = _storageSetting.GetForgeStorageSetting().GetForgeZipFile(minecraftVersion, forgeVersion);
                        using (var target = file.OpenWrite())
                        {
                            await model.ForgeEditViewModel.ForgeZipFile.CopyToAsync(target);
                        }
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Versions), "Admin", new RouteValueDictionary
                    {
                        {"id", minecraftVersionId},
                        {"subaction", ""},
                        {"subsubaction", ""},
                        {"message", "success_save"}
                    });
                }
            }


            return NotFound();
        }


        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl, RouteValueDictionary route = null)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Users), "Admin", route);
        }

        #endregion
    }
}