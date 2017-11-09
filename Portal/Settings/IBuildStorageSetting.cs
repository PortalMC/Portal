using System.IO;

namespace Portal.Settings
{
    public interface IBuildStorageSetting
    {
        DirectoryInfo GetRootDirectory();
        void AfterBuild(string projectId);
    }
}