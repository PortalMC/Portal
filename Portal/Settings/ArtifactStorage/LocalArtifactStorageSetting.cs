using System.IO;
using Microsoft.Extensions.Configuration;

namespace Portal.Settings.ArtifactStorage
{
    public class LocalArtifactStorageSetting : IArtifactStorageSetting
    {
        private readonly DirectoryInfo _root;

        public LocalArtifactStorageSetting(IConfiguration configuration)
        {
            _root = new DirectoryInfo(configuration.GetValue<string>("Root"));
        }

        public DirectoryInfo GetRootDirectory()
        {
            return _root;
        }

        public void AfterBuild(string projectId)
        {
        }
    }
}