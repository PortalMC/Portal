using Microsoft.Extensions.Configuration;

namespace Portal.Models
{
    public class ApiClient
    {
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string DisplayName { get; }

        public ApiClient(string clientId, string clientSecret, string displayName)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            DisplayName = displayName;
        }

        public static ApiClient Parse(IConfiguration configuration)
        {
            return new ApiClient(
                configuration.GetValue<string>("ClientId"),
                configuration.GetValue<string>("ClientSecret"),
                configuration.GetValue<string>("DisplayName")
            );
        }
    }
}