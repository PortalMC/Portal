using System.IO;
using Microsoft.Extensions.Configuration;

namespace Portal.Settings.CoremodStorage
{
    public class LocalCoremodStorageSetting : CoremodStorageSetting
    {
        private readonly DirectoryInfo _root;

        public LocalCoremodStorageSetting(IConfiguration configuration)
        {
            _root = new DirectoryInfo(configuration.GetValue<string>("Root"));
        }

        public override DirectoryInfo GetRootDirectory()
        {
            return _root;
        }
    }
}