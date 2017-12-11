using System.ComponentModel.DataAnnotations;

namespace Portal.Models
{
    public class ForgeVersion
    {
        public string Id { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public bool IsRecommend { get; set; }
    }
}