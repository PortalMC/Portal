using System.Linq;
using Microsoft.Extensions.Configuration;
using Portal.Models;

namespace Portal.Settings
{
    public class SecureSetting
    {
        public ApiClient[] ApiClients { get; }

        public SecureSetting(IConfiguration configuration)
        {
            ApiClients = configuration.GetSection("ApiClients").GetChildren().Select(ApiClient.Parse).ToArray();
        }
    }
}