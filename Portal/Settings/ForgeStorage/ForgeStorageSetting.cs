using System.IO;
using Portal.Extensions;
using Portal.Models;

namespace Portal.Settings.ForgeStorage
{
    public abstract class ForgeStorageSetting
    {
        public abstract DirectoryInfo GetRootDirectory();

        public FileInfo GetForgeZipFile(MinecraftVersion minecraftVersion, ForgeVersion forgeVersion)
        {
            return GetRootDirectory().ResolveDir(minecraftVersion.Id).Resolve(forgeVersion.Id + ".zip");
        }
    }
}