using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Models
{
    public class User
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public List<Project> Projects { get; set; }
    }
}