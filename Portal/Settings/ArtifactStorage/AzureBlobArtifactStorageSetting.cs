using System.IO;

namespace Portal.Settings.ArtifactStorage
{
    public class AzureBlobArtifactStorageSetting : IArtifactStorageSetting
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