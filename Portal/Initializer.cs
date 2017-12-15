﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OpenIddict.Models;
using Portal.Extensions;
using Portal.Data;
using Portal.Models;
using Portal.Services;

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

                InitializeRoleAsync(scope.ServiceProvider).Wait(cancellationToken);
                InitializeApiClientAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
                InitializeDefaultUserAsync(scope.ServiceProvider).Wait(cancellationToken);
                InitializeMinecraftAndForgeVersionAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
                InitializeSnippetAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
                InitializeDockerAsync(scope.ServiceProvider, cancellationToken).Wait(cancellationToken);
            }
        }

        private static async Task InitializeRoleAsync(IServiceProvider services)
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
            var clients = services.GetRequiredService<IConfiguration>().GetSection("Secure").GetSection("ApiClients");

            foreach (var client in clients.GetChildren())
            {
                var clientId = client.GetValue<string>("ClientId");
                var clientSecret = client.GetValue<string>("ClientSecret");
                var displayName = client.GetValue<string>("DisplayName");
                var c = await manager.FindByClientIdAsync(clientId, cancellationToken);
                if (c != null) continue;
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    DisplayName = displayName
                };
                await manager.CreateAsync(descriptor, cancellationToken);
            }
        }

        private static async Task InitializeDockerAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            if (!(services.GetRequiredService<IBuildService>() is DockerBuildService buildService)) return;
            var context = services.GetRequiredService<ApplicationDbContext>();
            await buildService.CheckAllImageAsync(context, cancellationToken);
        }

        private static async Task InitializeDefaultUserAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();
            var users = services.GetRequiredService<IConfiguration>().GetSection("Secure").GetSection("DefaultUsers");
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Initializer).Name);

            foreach (var user in users.GetChildren())
            {
                var username = user.GetValue<string>("UserName");
                var password = user.GetValue<string>("Password");
                var role = user.GetValue<string>("Role");
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

        private static async Task InitializeMinecraftAndForgeVersionAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var minecraftVersions = services.GetRequiredService<IConfiguration>().GetSection("Minecraft");
            var minecraftVersionList = new List<MinecraftVersion>();
            var forgeVersionList = new List<ForgeVersion>();
            var minecraftVersionOrder = 1;
            var now = DateTime.UtcNow;
            foreach (var minecraftVersionConfig in minecraftVersions.GetSection("Versions").GetChildren())
            {
                var version = minecraftVersionConfig.GetValue<string>("MinecraftVersion");
                if (await context.MinecraftVersions.AnyAsync(v => v.Version == version, cancellationToken))
                {
                    continue;
                }
                var forgeVersionOrder = 1;
                var forgeVersions = minecraftVersionConfig.GetSection("ForgeVersions").GetChildren()
                    .Select(forgeVersion =>
                        new ForgeVersion
                        {
                            Version = forgeVersion.GetValue<string>("ForgeVersion"),
                            FileName = forgeVersion.GetValue<string>("File"),
                            IsRecommend = forgeVersion.GetValue("Recommended", false),
                            CreatedAt = now,
                            UpdatedAt = now,
                            Rank = forgeVersionOrder++
                        })
                    .Where(v => !context.ForgeVersions.Any(fv => fv.Version == v.Version))
                    .ToArray();
                forgeVersionList.AddRange(forgeVersions);
                minecraftVersionList.Add(new MinecraftVersion
                {
                    Version = version,
                    DockerImageVersion = minecraftVersionConfig.GetValue<string>("DockerImageVersion"),
                    ForgeVersions = forgeVersions,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Rank = minecraftVersionOrder++
                });
            }
            await context.ForgeVersions.AddRangeAsync(forgeVersionList, cancellationToken);
            await context.MinecraftVersions.AddRangeAsync(minecraftVersionList, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task InitializeSnippetAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var snippetGroups = services.GetRequiredService<IConfiguration>().GetSection("SnippetGroups");
            foreach (var group in snippetGroups.GetChildren())
            {
                if (!await context.SnippetGroups.AnyAsync(g => g.Id == group.GetValue<string>("Id"), cancellationToken))
                {
                    await context.SnippetGroups.AddAsync(new SnippetGroup()
                    {
                        Id = group.GetValue<string>("Id"),
                        DisplayName = group.GetValue<string>("DisplayName"),
                        Order = group.GetValue<int>("Order"),
                    }, cancellationToken);
                }
            }
            var snippets = services.GetRequiredService<IConfiguration>().GetSection("Snippets");
            foreach (var snippet in snippets.GetChildren())
            {
                if (!await context.Snippets.AsNoTracking().AnyAsync(s => s.Type == snippet.GetValue<string>("Type"), cancellationToken))
                {
                    await context.Snippets.AddAsync(new Snippet
                    {
                        Type = snippet.GetValue<string>("Type"),
                        DisplayName = snippet.GetValue<string>("DisplayName"),
                        Group = context.SnippetGroups.Find(snippet.GetValue<string>("GroupId")),
                        Order = snippet.GetValue<int>("Order"),
                        Content = snippet.GetValue<string>("Content")
                    }, cancellationToken);
                }
            }
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}