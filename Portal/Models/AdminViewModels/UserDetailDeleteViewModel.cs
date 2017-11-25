using System.ComponentModel.DataAnnotations;

namespace Portal.Models.AdminViewModels
{
    public class UserDetailDeleteViewModel
    {
        [Required]
        [Display(Name = "Username")]
        [Compare("UserNameConfirm", ErrorMessage = "The username and input value do not match.")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "UsernameConfirm")]
        public string UserNameConfirm { get; set; }
    }
}