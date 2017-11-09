using System.IO;

namespace Portal.Settings.ArtifactStorage
{
    public interface IArtifactStorageSetting
    {
        DirectoryInfo GetRootDirectory();
        void AfterBuild(string projectId);
    }
}