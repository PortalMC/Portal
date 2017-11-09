using System.IO;

namespace Portal.Settings
{
    public class AzureBlobBuildStorageSetting : IBuildStorageSetting
    {
        public DirectoryInfo GetRootDirectory()
        {
            throw new System.NotImplementedException();
        }

        public void AfterBuild(string projectId)
        {
            throw new System.NotImplementedException();
        }
    }
}