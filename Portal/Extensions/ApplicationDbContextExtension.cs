using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;

namespace Portal.Extensions
{
    public static class ApplicationDbContextExtension
    {
        public static async Task<bool> CanAccessToProjectAsync(this ApplicationDbContext context, string userId,
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

        public static async Task<(bool canAccess, Project project)> CanAccessToProjectWithProjectAsync(
            this ApplicationDbContext context,
            string userId,
            string projectId)
        {
            var project = await context.AccessRights
                .AsNoTracking()
                .Where(a => a.User.Id == userId)
                .Select(a => a.Project)
                .Where(p => p.Id == projectId)
                .SingleOrDefaultAsync();
            return project == default(Project) ? (false, null) : (true, project);
        }

        public static async Task UpdateProjectUpdatedAtAsync(this ApplicationDbContext context, string projectId)
        {
            var project = await context.Projects
                .SingleAsync(p => p.Id == projectId);
            project.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}