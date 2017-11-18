using System;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OpenIddict.Models;
using Portal.Data;
using Portal.Models;
using Portal.Services;
using Portal.Settings;

namespace Portal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict();
            });

            ConfigureServicesInternal(services);
        }

        private void ConfigureServicesInternal(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.AddAuthentication()
                .AddCookie()
                .AddOAuthValidation();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Authenticated", policy =>
                {
                    policy.AddAuthenticationSchemes(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        OAuthValidationDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddOpenIddict(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the token endpoint (required to use the password flow).
                options.EnableAuthorizationEndpoint("/connect/authorize");
                options.EnableTokenEndpoint("/connect/token");

                // Allow client applications to use the grant_type=password flow.
                options.AllowPasswordFlow();

                // During development, you can disable the HTTPS requirement.
                options.DisableHttpsRequirement();
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Add application services.
            var buildSetting = new BuildSetting(Configuration.GetSection("Building"));
            services.AddSingleton(new StorageSetting(Configuration.GetSection("Storages")));
            services.AddSingleton(buildSetting);
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IMinecraftVersionProvider>(new LocalMinecraftVersionProvider(Configuration.GetSection("Minecraft")));
            services.AddBuildService(buildSetting.GetBuildMethodType());
            services.AddSingleton<IBuildService, DockerBuildService>();
            services.AddSingleton<WebSocketConnectionManager>();
            services.AddSingleton<WebSocketService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseWebSockets();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "Projects",
                    "Projects/{uuid}/{action}",
                    new
                    {
                        controller = "Projects",
                        action = "Index",
                        uuid = ""
                    }
                );
                routes.MapRoute(
                    "StatusCode",
                    "StatusCode/{statusCode}",
                    new
                    {
                        controller = "StatusCode",
                        action = "Index"
                    });
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Use(serviceProvider.GetService<WebSocketService>().Acceptor);

            InitializeAsync(app.ApplicationServices, CancellationToken.None).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync(cancellationToken);

                var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictApplication>>();
                var clients = Configuration.GetSection("Secure").GetSection("ApiClients").GetChildren();

                foreach (var client in clients)
                {
                    var id = client.GetValue<string>("ClientId");
                    if (await manager.FindByClientIdAsync(id, cancellationToken) == null)
                    {
                        var descriptor = new OpenIddictApplicationDescriptor
                        {
                            ClientId = id,
                            ClientSecret = client.GetValue<string>("ClientSecret"),
                            DisplayName = client.GetValue<string>("DisplayName")
                        };

                        await manager.CreateAsync(descriptor, cancellationToken);
                    }
                }
            }
        }
    }
}