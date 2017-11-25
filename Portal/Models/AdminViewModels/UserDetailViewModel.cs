using System.Collections.Generic;

namespace Portal.Models.AdminViewModels
{
    public class UserDetailViewModel
    {
        public ApplicationUser User { get; set; }

        public User SafeUser { get; set; }

        public IList<string> Roles { get; set; }
    }
}