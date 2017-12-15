using System.ComponentModel.DataAnnotations;

namespace Portal.Models.AdminViewModels
{
    public class VersionDetailDockerImageVersionViewModel
    {
        public MinecraftVersion MinecraftVersion { get; set; }

        [Required]
        public string Version { get; set; }
    }
}