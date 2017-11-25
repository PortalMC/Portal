using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Portal.Data;
using Portal.Models;

namespace Portal.Extensions
{
    public static class UserManagerExtension
    {
        public static async Task<(ApplicationUser user, IdentityResult result)> CreateWithPortalAsync(this UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            string username, string password = null)
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = username
            };
            var safeUser = new User
            {
                Id = user.Id
            };
            await dbContext.SafeUsers.AddAsync(safeUser);
            await dbContext.SaveChangesAsync();
            if (password != null)
            {
                var result = await userManager.CreateAsync(user, password);
                return (user, result);
            }
            else
            {
                var result = await userManager.CreateAsync(user);
                return (user, result);
            }
        }
    }
}