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

namespace Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _context = context;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users(string id)
        {
            if (id == "New")
            {
                return View("NewUser");
            }
            var applicationUsers = _context.Users.AsNoTracking().Take(10).ToArray();
            return View(applicationUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Users(string id, NewUserViewModel model, string returnUrl = null)
        {
            if (id != "New")
            {
                return NotFound();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    var ret = await _userManager.CreateWithPortalAsync(_context, model.Email, model.Password);
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
            }

            return View(model);
        }

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