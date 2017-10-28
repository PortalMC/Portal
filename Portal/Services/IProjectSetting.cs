using System.IO;

namespace Portal.Services
{
    public interface IProjectSetting
    {
        DirectoryInfo GetProjectsRoot();
    }
}