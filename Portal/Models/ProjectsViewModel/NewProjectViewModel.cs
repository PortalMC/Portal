using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ProjectsViewModel
{
    public class NewProjectViewModel
    {
        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required(ErrorMessage = "The Minecraft version must be selected.")]
        [Display(Name = "Minecraft version")]
        public string MinecraftVersionId { get; set; }

        [Required(ErrorMessage = "The Forge version must be selected.")]
        [Display(Name = "Forge version")]
        public string ForgeVersionId { get; set; }
    }
}