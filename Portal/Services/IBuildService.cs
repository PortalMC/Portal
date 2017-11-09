using Portal.Models;

namespace Portal.Services
{
    public interface IBuildService
    {
        void StartBuild(Project project, string userId);
    }
}