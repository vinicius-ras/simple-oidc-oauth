using IdentityServer4.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Data.Security;
using SimpleOidcOauth.IdentityServer;
using SimpleOidcOauth.OpenApi.Swagger.Filters;
using SimpleOidcOauth.Security.Authorization.Handlers;
using SimpleOidcOauth.Security.Authorization.Requirements;
using SimpleOidcOauth.Services;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

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
            services.AddIdentity<ApplicationUser, ApplicationRole>(opts => {
                    opts.SignIn.RequireConfirmedAccount = true;
                    opts.SignIn.RequireConfirmedEmail = true;
                    opts.SignIn.RequireConfirmedPhoneNumber = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();


            var identityServerBuilder = services.AddIdentityServer(opts => {
                    opts.UserInteraction.LoginUrl = appConfigs.Spa.LoginUrl;
                    opts.UserInteraction.LogoutUrl = appConfigs.Spa.LogoutUrl;
                    opts.UserInteraction.ErrorUrl = appConfigs.AuthServer.IdentityProviderErrorUrl;
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
                .AddAspNetIdentity<ApplicationUser>();


            // Configures the cookies used by the application
            services.ConfigureApplicationCookie(opts => {
                opts.Cookie.Name = appConfigs.AuthServer.ApplicationCookieName;
                opts.Cookie.SameSite = SameSiteMode.None;

                // The workarounds implemented below are described in: https://github.com/dotnet/aspnetcore/issues/9039

                // On authentication failure when the clients try to access API endpoints,
                // prevent HTTP 302 (Found) redirections to the "standard login page" and send HTTP 401 (Unauthorized) instead.
                var originalRedirectToLogin = opts.Events.OnRedirectToLogin;
                opts.Events.OnRedirectToLogin = (context) => {
                    var requestPath = context.Request.Path;
                    if (requestPath.StartsWithSegments(AppEndpoints.ApiUriPrefix))
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
                    if (requestPath.StartsWithSegments(AppEndpoints.ApiUriPrefix))
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


            // Configure AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // Configure Swashbuckle / Swagger
            services.AddSwaggerGen(opts => {
                // Use the application's XML Documentation to generate an OpenAPI documentation
                string curAssemblyName = Assembly.GetExecutingAssembly().GetName().Name,
                    appBasePath = AppContext.BaseDirectory,
                    xmlDocumentationPath = $"{appBasePath}{curAssemblyName}.xml";
                opts.IncludeXmlComments(xmlDocumentationPath);

                // Create our application's OpenAPI Document
                var swaggerConfigs = appConfigs.Swagger;
                var openApiInfo = new OpenApiInfo
                {
                    Title = swaggerConfigs.ApiTitleFull,
                    Version = swaggerConfigs.ApiVersion,
                    Description = swaggerConfigs.ApiDescription,
                    Contact = new OpenApiContact {
                        Name = swaggerConfigs.ApiContactName,
                        Email = swaggerConfigs.ApiContactEmail,
                        Url = string.IsNullOrWhiteSpace(swaggerConfigs.ApiContactUrl)
                            ? null
                            : new Uri(swaggerConfigs.ApiContactUrl),
                    },
                    License = new OpenApiLicense {
                        Name = swaggerConfigs.ApiLicenseName,
                        Url = string.IsNullOrWhiteSpace(swaggerConfigs.ApiLicenseUrl)
                            ? null
                            : new Uri(swaggerConfigs.ApiLicenseUrl),
                    },
                    TermsOfService = string.IsNullOrWhiteSpace(swaggerConfigs.ApiTermsOfServiceUrl)
                        ? null
                        : new Uri(swaggerConfigs.ApiTermsOfServiceUrl),
                };
                opts.SwaggerDoc(swaggerConfigs.ApiDocumentNameUrlFriendly, openApiInfo);

                // Configure some other Swashbuckle's options and filters
                opts.EnableAnnotations();
                opts.SchemaFilter<ValidationProblemDetailsFilter>();
            });


            // Configure authorization handlers and policies
            services.AddSingleton<IAuthorizationHandler, AtLeastOneClaimAuthorizationHandler>();

            services.AddAuthorization(opts => {
                // Client Application policies
                opts.AddPolicy(AuthorizationPolicyNames.ClientsView, policyBuilder => {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.AddRequirements(new AtLeastOneClaimAuthorizationRequirement(AuthServerClaimTypes.CanViewClients, AuthServerClaimTypes.CanViewAndEditClients));
                });
                opts.AddPolicy(AuthorizationPolicyNames.ClientsViewAndEdit, policyBuilder => {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireClaim(AuthServerClaimTypes.CanViewAndEditClients);
                });

                // Registered Users policies
                opts.AddPolicy(AuthorizationPolicyNames.UsersView, policyBuilder => {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.AddRequirements(new AtLeastOneClaimAuthorizationRequirement(AuthServerClaimTypes.CanViewUsers, AuthServerClaimTypes.CanViewAndEditUsers));
                });
                opts.AddPolicy(AuthorizationPolicyNames.UsersViewAndEdit, policyBuilder => {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireClaim(AuthServerClaimTypes.CanViewAndEditUsers);
                });

                // Registered Resources (API Scopes, API Resources and Identity Resources) policies
                opts.AddPolicy(AuthorizationPolicyNames.ResourcesView, policyBuilder => {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.AddRequirements(new AtLeastOneClaimAuthorizationRequirement(AuthServerClaimTypes.CanViewResources, AuthServerClaimTypes.CanViewAndEditResources));
                });
                opts.AddPolicy(AuthorizationPolicyNames.ResourcesViewAndEdit, policyBuilder => {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireClaim(AuthServerClaimTypes.CanViewAndEditResources);
                });
            });


            // Configure other custom services
            services.AddTransient<IReturnUrlParser, CustomReturnUrlParser>();
            services.AddTransient<IEmbeddedResourcesService, EmbeddedResourcesService>();

            services.AddTransient<ISmtpClient, SmtpClient>();
            services.AddTransient<IEmailService, EmailService>();

            services.AddTransient<IDatabaseInitializerService, DatabaseInitializerService>();
            services.AddHostedService<DatabaseInitializerHostedService>();

            // Configure key/signing material
            if (_webHostEnvironment.IsDevelopment())
                identityServerBuilder.AddDeveloperSigningCredential();
            else
                throw new NotImplementedException($@"Signing credentials not configured/implemented for environment of type ""{_webHostEnvironment.EnvironmentName}"".");
        }


        /// <summary>Configures the HTTP request processing pipeline for the application.</summary>
        /// <param name="app">An object which can be used for configuring the application's HTTP request processing pipeline.</param>
        /// <param name="appConfigsOptions">Configurations provided to the application.</param>
        public void Configure(IApplicationBuilder app, IOptions<AppConfigs> appConfigsOptions)
        {
            var appConfigs = appConfigsOptions.Value;
            app.UseExceptionHandler(AppEndpoints.UnhandledExceptionUri)
                .UseCookiePolicy()
                .UseStaticFiles()
                .UseSwagger(opts => {
                    opts.RouteTemplate = appConfigs.Swagger.OpenApiDocumentRouteTemplate;
                })
                .UseSwaggerUI(opts => {
                    var swaggerConfigs = appConfigs.Swagger;

                    string routeTemplate = swaggerConfigs.OpenApiDocumentRouteTemplate,
                        targetOpenApiDocument = swaggerConfigs.ApiDocumentNameUrlFriendly,
                        effectiveEndpoint = routeTemplate.Replace("{documentName}", targetOpenApiDocument);
                    opts.SwaggerEndpoint(effectiveEndpoint, swaggerConfigs.ApiTitleShort);

                    opts.DocumentTitle = swaggerConfigs.SwaggerUIPageTitle;
                    opts.RoutePrefix = swaggerConfigs.SwaggerUIRoutePrefix;
                })
                .UseRouting()
                .UseCors()
                .UseIdentityServer()
                .UseAuthorization()
                .UseEndpoints(endpoints => {
                    endpoints.MapDefaultControllerRoute();
                });
        }
    }
}
