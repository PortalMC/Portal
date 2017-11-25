using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Data;
using Portal.Extensions;
using Portal.Models;
using Portal.Models.AdminViewModels;
using Portal.Utils;

namespace Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AdminController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILoggerFactory loggerFactory)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users(string id, string subaction, string message = null)
        {
            ViewBag.Message = message;
            if (id == "New")
            {
                return View("NewUser");
            }
            if (Util.IsCorrectUuid(id))
            {
                switch (subaction)
                {
                    case null:
                    {
                        var user = await _userManager.FindByIdAsync(id);
                        var safeUser = await _context.SafeUsers.AsNoTracking().Where(u => u.Id == id).FirstAsync();
                        var roles = await _userManager.GetRolesAsync(user);
                        return View("UserDetail", new UserDetailViewModel
                        {
                            User = user,
                            SafeUser = safeUser,
                            Roles = roles
                        });
                    }
                    case "Login":
                    {
                        var user = await _userManager.FindByIdAsync(id);
                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }
                    case "ResetPassword":
                    {
                        var user = await _userManager.FindByIdAsync(id);
                        return View("UserDetailResetPassword", new UserDetailPostViewModel
                        {
                            ResetPasswordViewModel = new UserDetailResetPasswordViewModel
                            {
                                User = user
                            }
                        });
                    }
                    case "Delete":
                        break;
                    default:
                        return NotFound();
                }
            }
            var applicationUsers = _context.Users.AsNoTracking().Take(10).ToArray();
            return View(applicationUsers);
        }

        [HttpPost]
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
                if (subaction == "ResetPassword")
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
            }
            return NotFound();
        }

        [HttpGet]
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