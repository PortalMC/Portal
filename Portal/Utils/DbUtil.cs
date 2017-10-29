using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Portal.Data;

namespace Portal.Utils
{
    public static class DbUtil
    {
        public static async Task<bool> CanAccessToProject(this ApplicationDbContext context, string userId,
            string projectId)
        {
            var accessRightCount = await context.AccessRights
                .AsNoTracking()
                .Where(a => a.User.Id == userId)
                .Include(a => a.Project)
                .Where(a => a.Project.Id == projectId)
                .CountAsync();
            return accessRightCount != 0;
        }
    }
}