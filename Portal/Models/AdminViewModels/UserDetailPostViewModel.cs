namespace Portal.Models.AdminViewModels
{
    public class UserDetailPostViewModel
    {
        public ApplicationUser User { get; set; }

        public NewUserViewModel NewUserViewModel { get; set; }
        public UserDetailResetPasswordViewModel ResetPasswordViewModel { get; set; }
        public UserDetailDeleteViewModel DeleteViewModel { get; set; }
    }
}