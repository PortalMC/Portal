using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Portal.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }
    }
}