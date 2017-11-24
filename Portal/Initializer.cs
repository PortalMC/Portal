using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using OpenIddict.Models;
using Portal.Data;
using Portal.Services;
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
                InitializeApiClientAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
                InitializeDockerAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
            }
        }

        private static async Task InitializeApiClientAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var manager = services.GetRequiredService<OpenIddictApplicationManager<OpenIddictApplication>>();
            var clients = services.GetRequiredService<SecureSetting>().ApiClients;

            foreach (var client in clients)
            {
                var c = await manager.FindByClientIdAsync(client.ClientId, cancellationToken);
                if (c != null) continue;
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = client.ClientId,
                    ClientSecret = client.ClientSecret,
                    DisplayName = client.DisplayName
                };
                await manager.CreateAsync(descriptor, cancellationToken);
            }
        }

        private static async Task InitializeDockerAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            if (!(services.GetRequiredService<IBuildService>() is DockerBuildService buildService)) return;
            await buildService.CheckImageAsync(cancellationToken);
        }
    }
}