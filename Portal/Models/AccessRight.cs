using System.ComponentModel.DataAnnotations;

namespace Portal.Models
{
    public class AccessRight
    {
        public string Id { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public int Level { get; set; }
    }
}