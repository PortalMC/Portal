using System.Collections.Generic;

namespace Portal.Models
{
    public class MinecraftVersion
    {
        public string Version { get; }
        public ICollection<ForgeVersion> ForgeVersions { get; }
        public string DockerImageVersion { get; }

        public MinecraftVersion(string version, ICollection<ForgeVersion> forgeVersions, string dockerImageVersion)
        {
            Version = version;
            ForgeVersions = forgeVersions;
            DockerImageVersion = dockerImageVersion;
        }
    }
}