using System.IO;
using Microsoft.Extensions.Configuration;

namespace Portal.Settings
{
    public class LocalBuildStorageSetting : IBuildStorageSetting
    {
        private readonly DirectoryInfo _root;

        public LocalBuildStorageSetting(IConfiguration configuration)
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