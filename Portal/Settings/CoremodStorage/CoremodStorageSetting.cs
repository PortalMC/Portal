using System.IO;
using Portal.Extensions;
using Portal.Models;

namespace Portal.Settings.CoremodStorage
{
    public abstract class CoremodStorageSetting
    {
        public abstract DirectoryInfo GetRootDirectory();

        public FileInfo GetCoremodFile(MinecraftVersion minecraftVersion)
        {
            return GetRootDirectory().Resolve(minecraftVersion.Id + ".zip");
        }

        public FileInfo GetPropertyFile(MinecraftVersion minecraftVersion)
        {
            return GetRootDirectory().Resolve(minecraftVersion.Id + ".properties");
        }
    }
}