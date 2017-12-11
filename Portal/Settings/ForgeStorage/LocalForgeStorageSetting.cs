using System.IO;
using Microsoft.Extensions.Configuration;

namespace Portal.Settings.ForgeStorage
{
    public class LocalForgeStorageSetting : ForgeStorageSetting
    {
        private readonly DirectoryInfo _root;

        public LocalForgeStorageSetting(IConfiguration configuration)
        {
            _root = new DirectoryInfo(configuration.GetValue<string>("Root"));
        }

        public override DirectoryInfo GetRootDirectory()
        {
            return _root;
        }
    }
}