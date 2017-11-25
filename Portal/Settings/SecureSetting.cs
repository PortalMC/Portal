using System.Dynamic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Portal.Settings
{
    public class SecureSetting
    {
        public dynamic[] ApiClients { get; }
        public dynamic[] DefaultUsers { get; }

        public SecureSetting(IConfiguration configuration)
        {
            ApiClients = configuration.GetSection("ApiClients").GetChildren().Select(c =>
            {
                dynamic o = new ExpandoObject();
                o.ClientId = c.GetValue<string>("ClientId");
                o.ClientSecret = c.GetValue<string>("ClientSecret");
                o.DisplayName = c.GetValue<string>("DisplayName");
                return o;
            }).ToArray();
            DefaultUsers = configuration.GetSection("DefaultUsers").GetChildren().Select(c =>
            {
                dynamic o = new ExpandoObject();
                o.UserName = c.GetValue<string>("UserName");
                o.Password = c.GetValue<string>("Password");
                o.Role = c.GetValue<string>("Role");
                return o;
            }).ToArray();
        }
    }
}