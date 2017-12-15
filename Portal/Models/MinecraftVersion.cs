using System;
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

        public int Rank { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; }
    }
}