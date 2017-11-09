using System.IO;

namespace Portal.Settings.ProjectStorage
{
    public interface IProjectStorageSetting
    {
        DirectoryInfo GetRootDirectory();
    }
}