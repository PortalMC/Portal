using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Models
{
    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MinecraftVersion { get; set; }
        public List<AccessRight> AccessRights { get; set; }
    }
}