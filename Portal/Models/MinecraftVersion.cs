using System.Collections.Generic;

namespace Portal.Models
{
    public class MinecraftVersion
    {
        public string Version { get; }
        public ICollection<ForgeVersion> ForgeVersions { get; }

        public MinecraftVersion(string version, ICollection<ForgeVersion> forgeVersions)
        {
            Version = version;
            ForgeVersions = forgeVersions;
        }
    }
}