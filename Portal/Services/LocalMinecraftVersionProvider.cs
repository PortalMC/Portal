using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Portal.Extensions;
using Portal.Models;

namespace Portal.Services
{
    public class LocalMinecraftVersionProvider : IMinecraftVersionProvider
    {
        private readonly List<MinecraftVersion> _minecraftVersions;
        private readonly DirectoryInfo _root;

        public LocalMinecraftVersionProvider(IConfiguration configuration)
        {
            _root = new DirectoryInfo(configuration.GetSection("Root").Value);
            _minecraftVersions = (from versionRaw in configuration.GetSection("Versions").GetChildren()
                let minecraftVersionStr = versionRaw.GetValue("MinecraftVersion", "")
                let forgeVersionsRaw = versionRaw.GetSection("ForgeVersions").GetChildren()
                let forgeVersions = forgeVersionsRaw.Select(v =>
                        new ForgeVersion(v.GetValue("ForgeVersion", ""),
                            v.GetValue("File", ""),
                            v.GetValue("Recommended", false)))
                    .ToList()
                select new MinecraftVersion(minecraftVersionStr, forgeVersions)).ToList();
        }

        public ICollection<MinecraftVersion> GetMinecraftVersions()
        {
            return _minecraftVersions;
        }

        public FileInfo GetForgeZipFile(MinecraftVersion minecraftVersion, ForgeVersion forgeVersion)
        {
            return _root.ResolveDir(minecraftVersion.Version).Resolve(forgeVersion.FileName);
        }
    }
}