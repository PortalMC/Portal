using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OpenIddict.Models;
using Portal.Extensions;
using Portal.Data;
using Portal.Models;
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
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreatedAsync(cancellationToken).Wait(cancellationToken);

                InitializeRoleAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
                InitializeApiClientAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
                InitializeDefaultUserAsync(scope.ServiceProvider).Wait(cancellationToken);
                InitializeDockerAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
            }
        }

        private static async Task InitializeRoleAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Initializer).Name);
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = {"Admin", "Member"};
            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (roleExists) continue;
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (roleResult.Succeeded)
                {
                    logger.LogInformation(3, $"Create new role '{roleName}'");
                }
                else
                {
                    logger.LogError(3, $"Failed to create role '{roleName}'");
                }
            }
        }

        private static async Task InitializeApiClientAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
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

        private static async Task InitializeDefaultUserAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();
            var users = services.GetRequiredService<SecureSetting>().DefaultUsers;
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Initializer).Name);

            foreach (var user in users)
            {
                string username = user.UserName;
                string password = user.Password;
                string role = user.Role;
                var checkUser = await userManager.FindByNameAsync(username);
                if (checkUser != null)
                {
                    return;
                }
                var ret = await userManager.CreateWithPortalAsync(context, username, password);
                if (ret.result.Succeeded)
                {
                    var roleResult = await userManager.AddToRoleAsync(ret.user, role);
                    logger.LogInformation(3, "Default user is created with password.");
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation(3, $"Set default user role to '{role}'");
                    }
                    else
                    {
                        logger.LogError(3, $"Failed to set default user role to '{role}'");
                    }
                }
                else
                {
                    logger.LogError(3, $"Can't create default user : {username}");
                }
            }
        }
    }
}