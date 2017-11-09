using System.IO;
using Microsoft.Extensions.Configuration;

namespace Portal.Settings.ProjectStorage
{
    public class LocalProjectStorageSetting : IProjectStorageSetting
    {
        private readonly DirectoryInfo _root;

        public LocalProjectStorageSetting(IConfiguration configuration)
        {
            _root = new DirectoryInfo(configuration.GetValue<string>("Root"));
        }

        public DirectoryInfo GetRootDirectory()
        {
            return _root;
        }
    }
}