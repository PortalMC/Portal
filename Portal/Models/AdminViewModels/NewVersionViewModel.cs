using System.ComponentModel.DataAnnotations;

namespace Portal.Models.AdminViewModels
{
    public class NewVersionViewModel
    {
        [Required]
        public string Version { get; set; }

        [Required]
        public string DockerImageVersion { get; set; }
    }
}