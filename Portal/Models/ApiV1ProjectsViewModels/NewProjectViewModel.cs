using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ApiV1ProjectsViewModels
{
    public class NewProjectViewModel
    {
        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public string MinecraftVersionId { get; set; }

        [Required]
        public string ForgeVersionId { get; set; }
    }
}