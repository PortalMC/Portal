using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Portal.Models.AdminViewModels
{
    public class NewForgeVersionViewModel
    {
        public MinecraftVersion MinecraftVersion { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public bool IsRecommend { get; set; }

        [Required]
        [FileExtensions(Extensions = "zip")]
        public IFormFile ForgeZipFile { get; set; }
    }
}