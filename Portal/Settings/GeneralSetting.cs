using Microsoft.Extensions.Configuration;

namespace Portal.Settings
{
    public class GeneralSetting
    {
        public bool CreateAccount { get; }

        public GeneralSetting(IConfiguration configuration)
        {
            CreateAccount = configuration.GetValue<bool>("CreateAccount");
        }
    }
}