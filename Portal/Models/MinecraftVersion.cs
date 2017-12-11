using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Portal.Models
{
    public class MinecraftVersion
    {
        public string Id { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public IEnumerable<ForgeVersion> ForgeVersions { get; set; }

        [Required]
        public string DockerImageVersion { get; set; }
    }
}