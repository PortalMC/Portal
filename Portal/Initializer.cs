using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using OpenIddict.Models;
using Portal.Data;
using Portal.Settings;

namespace Portal
{
    public static class Initializer
    {
        public static void Initialize(IServiceProvider services, CancellationToken cancellationToken)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();

                var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictApplication>>();
                var clients = scope.ServiceProvider.GetRequiredService<SecureSetting>().ApiClients;

                foreach (var client in clients)
                {
                    if (manager.FindByClientIdAsync(client.ClientId, cancellationToken).GetAwaiter().GetResult() == null)
                    {
                        var descriptor = new OpenIddictApplicationDescriptor
                        {
                            ClientId = client.ClientId,
                            ClientSecret = client.ClientSecret,
                            DisplayName = client.DisplayName
                        };
                        manager.CreateAsync(descriptor, cancellationToken).Wait(cancellationToken);
                    }
                }
            }
        }
    }
}