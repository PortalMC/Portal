using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Portal.Models
{
    public class Project
    {
        public string Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public MinecraftVersion MinecraftVersion { get; set; }

        [Required]
        public ForgeVersion ForgeVersion { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; }

        [Required]
        public int BuildId { get; set; }

        [Required]
        public IList<AccessRight> AccessRights { get; set; }
    }
}