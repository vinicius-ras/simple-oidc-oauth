using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleOidcOauth.Controllers;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.IdentityServer;

namespace SimpleOidcOauth
{
    /// <summary>
    ///     Startup class for configuring the services available for the application's components,
    ///     as well as the HTTP request processing pipeline.
    /// </summary>
    public class Startup
    {
        // PRIVATE PROPERTIES
        /// <summary>Refererence to an <see cref="IWebHostEnvironment" /> object, injected by the container.</summary>
        private readonly IWebHostEnvironment _webHostEnvironment;
        /// <summary>Refererence to an <see cref="IConfiguration" /> object, injected by the container.</summary>
		private readonly IConfiguration _config;





		// PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="environment">Reference to an injected <see cref="IWebHostEnvironment"/>, provided by the container.</param>
		/// <param name="config">Reference to an injected <see cref="IConfiguration"/>, provided by the container.</param>
		public Startup(IWebHostEnvironment environment, IConfiguration config)
        {
            _webHostEnvironment = environment;
            _config = config;
        }


        /// <summary>
        ///     Configures the services of the application, which will then be available to be
        ///     injected by the container.
        /// </summary>
        /// <param name="services">The collection where services can be added for the container to know them.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Load app custom configurations
            var appConfigsSection = _config.GetSection(AppConfigs.ConfigKey);
            services.Configure<AppConfigs>(appConfigsSection);

            var appConfigs = appConfigsSection.Get<AppConfigs>();
            string connStrUsersStore = _config.GetConnectionString(AppConfigs.ConnectionStringIdentityServerUsers),
                connStrISConfigurationStore = _config.GetConnectionString(AppConfigs.ConnectionStringIdentityServerConfiguration),
                connStrISOperationalStore = _config.GetConnectionString(AppConfigs.ConnectionStringIdentityServerOperational);

            // Add controllers only (we don't need full MVC support for this project)
            services.AddControllers();


            // Add database access services
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<AppDbContext>(appDbOptions => {
                appDbOptions.UseSqlite(
                    connStrUsersStore,
                    sqlServerOptions => sqlServerOptions.MigrationsAssembly(migrationsAssembly)
                );
            });


            // Add ASP.NET Core Identity and IdentityServer4 services
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();


            var identityServerBuilder = services.AddIdentityServer(opts => {
                    opts.UserInteraction.LoginUrl = appConfigs.Spa.LoginUrl;
                    opts.UserInteraction.LogoutUrl = appConfigs.Spa.LogoutUrl;
                    opts.UserInteraction.ErrorUrl = appConfigs.Spa.ErrorUrl;
                })
                .AddConfigurationStore(configStoreOptions => {
                    configStoreOptions.ConfigureDbContext = genericDbOptions => {
                        genericDbOptions.UseSqlite(
                            connStrISConfigurationStore,
                            sqlServerOptions => sqlServerOptions.MigrationsAssembly(migrationsAssembly)
                        );
                    };
                })
                .AddOperationalStore(opStoreOptions => {
                    opStoreOptions.ConfigureDbContext = genericDbOptions => {
                        genericDbOptions.UseSqlite(
                            connStrISOperationalStore,
                            sqlServerOptions => sqlServerOptions.MigrationsAssembly(migrationsAssembly)
                        );
                    };
                })
                .AddAspNetIdentity<IdentityUser>();

            services.AddTransient<IReturnUrlParser, CustomReturnUrlParser>();


            // Configures the cookies used by the application
            services.ConfigureApplicationCookie(opts => {
                opts.Cookie.Name = appConfigs.ApplicationCookieName;
                opts.Cookie.SameSite = SameSiteMode.None;

                // The following workarounds implemented below are described in: https://github.com/dotnet/aspnetcore/issues/9039

                // On authentication failure when the clients try to access API endpoints,
                // prevent HTTP 302 (Found) redirections to the "standard login page" and send HTTP 401 (Unauthorized) instead.
                var originalRedirectToLogin = opts.Events.OnRedirectToLogin;
                opts.Events.OnRedirectToLogin = (context) => {
                    var requestPath = context.Request.Path;
                    if (requestPath.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    }
                    return originalRedirectToLogin(context);
                };


                // On authorization failure when the clients try to access API endpoints,
                // prevent HTTP 302 (Found) redirections to a "not authorized" page and send HTTP 403 (Forbiden) instead.
                var originalRedirectToAccessDenied = opts.Events.OnRedirectToAccessDenied;
                opts.Events.OnRedirectToAccessDenied = (context) => {
                    var requestPath = context.Request.Path;
                    if (requestPath.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                        return Task.CompletedTask;
                    }
                    return originalRedirectToAccessDenied(context);
                };
            });

            services.ConfigureSameSiteCookies();


            // Configure CORS
            services.AddCors(opts => {
                opts.AddDefaultPolicy(corsOpts => {
                    corsOpts.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithOrigins(appConfigs.Spa.BaseUrl);
                });
            });


            // Configure key/signing material
            if (_webHostEnvironment.IsDevelopment())
                identityServerBuilder.AddDeveloperSigningCredential();
            else
                throw new NotImplementedException($@"Signing credentials not configured/implemented for environment of type ""{_webHostEnvironment.EnvironmentName}"".");
        }


        /// <summary>Configures the HTTP request processing pipeline for the application.</summary>
        /// <param name="app">An object which can be used for configuring the application's HTTP request processing pipeline.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler(UnhandledExceptionsController.EXCEPTION_HANDLER_ROUTE);
            if (_webHostEnvironment.IsDevelopment())
                TestData.InitializeDatabase(app).Wait();

            app.UseCookiePolicy();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();

            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
               endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
