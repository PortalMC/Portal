﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portal.Models;

namespace Portal.Data
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<User> SafeUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<AccessRight> AccessRights { get; set; }
        public DbSet<Snippet> Snippets { get; set; }
        public DbSet<SnippetGroup> SnippetGroups { get; set; }
        public DbSet<MinecraftVersion> MinecraftVersions { get; set; }
        public DbSet<ForgeVersion> ForgeVersions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}