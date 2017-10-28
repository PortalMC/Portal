using System.Collections.Generic;
using System.IO;
using Portal.Models;

namespace Portal.Services
{
    public interface IMinecraftVersionProvider
    {
        ICollection<MinecraftVersion> GetMinecraftVersions();
        FileInfo GetForgeZipFile(MinecraftVersion minecraftVersion, ForgeVersion forgeVersion);
    }
}