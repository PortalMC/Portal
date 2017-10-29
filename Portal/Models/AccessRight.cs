using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Models
{
    public class AccessRight
    {
        public string Id { get; set; }
        public User User { get; set; }
        public Project Project { get; set; }
        public int Level { get; set; }
    }
}