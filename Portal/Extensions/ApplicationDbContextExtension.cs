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
            return await context.Projects
                .AsNoTracking()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.AccessRights)
                .Where(a => a.User.Id == userId)
                .AnyAsync();
        }

        public static async Task<(bool canAccess, Project project)> CanAccessToProjectWithProjectAsync(
            this ApplicationDbContext context,
            string userId,
            string projectId,
            bool noTracking = true,
            bool includeAll = false)
        {
            var project = await context.Projects
                .Ternary(noTracking, q => q.AsNoTracking(), q => q.AsTracking())
                .Ternary(includeAll, q => q.Include(p => p.ForgeVersion).Include(p => p.MinecraftVersion), q => q)
                .Include(p => p.AccessRights)
                .ThenInclude(a => a.User)
                .Where(p => p.Id == projectId)
                .SingleOrDefaultAsync(p => p.AccessRights.Any(a => a.User.Id == userId));
            return project == default(Project) ? (false, null) : (true, project);
        }

        public static async Task UpdateProjectUpdatedAtAsync(this ApplicationDbContext context, string projectId)
        {
            var project = await context.Projects
                .SingleAsync(p => p.Id == projectId);
            project.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        private static IQueryable<TK> Ternary<TK>(this IQueryable<TK> q, bool cond,
            Func<IQueryable<TK>, IQueryable<TK>> trueAction, Func<IQueryable<TK>, IQueryable<TK>> falseAction)
        {
            return cond ? trueAction(q) : falseAction(q);
        }
    }
}