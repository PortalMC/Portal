using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Portal.Models.AdminViewModels
{
    public class VersionDetailForgeEditViewModel
    {
        public MinecraftVersion MinecraftVersion { get; set; }

        public string Id { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public bool IsRecommend { get; set; }

        [FileExtensions(Extensions = "zip")]
        public IFormFile ForgeZipFile { get; set; }
    }
}