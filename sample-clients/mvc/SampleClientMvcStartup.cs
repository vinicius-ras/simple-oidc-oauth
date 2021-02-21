using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleOidcOauth.SampleClients.Mvc.Data.Configuration;

namespace SimpleOidcOauth.SampleClients.Mvc
{
    /// <summary>Startup class for configuring the services on the Sample MVC Client.</summary>
    public class SampleClientMvcStartup
    {
        // INSTANCE FIELDS
        /// <summary>Container-injected instance for an <see cref="IConfiguration"/> object.</summary>
		private readonly IConfiguration _configs;





        // INSTANCE METHODS
        /// <summary>Constructor.</summary>
        /// <param name="configs">Container-injected instance for an <see cref="IConfiguration"/> object.</param>
		public SampleClientMvcStartup(IConfiguration configs)
        {
            _configs = configs;
        }


        /// <summary>Configures the services to be used by the Sample MVC Client's server.</summary>
        /// <param name="services">Collection of services where all necessary services will be added.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            var appConfigs = _configs.GetSection(OpenIdConnectConfigs.SectionName)
                .Get<OpenIdConnectConfigs>();
            services.AddMvc();
            services
                .AddAuthentication(opts => {
                    opts.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = OpenIdConnectConfigs.AuthenticationSchemeName;
                })
                .AddCookie()
                .AddOpenIdConnect(OpenIdConnectConfigs.AuthenticationSchemeName, opts => {
                    opts.Authority = appConfigs.Authority;
                    opts.ClientId = appConfigs.ClientId;
                    opts.ClientSecret = appConfigs.ClientSecret;
                    opts.ResponseType = appConfigs.ResponseType;
                    foreach (var scope in appConfigs.RequiredScopes)
                        opts.Scope.Add(scope);
                    opts.UsePkce = appConfigs.UsePkce;
                });
            services.AddAuthorization();
        }


        /// <summary>Configures the middlewares used by the application.</summary>
        /// <param name="app">An object used to build the application's middleware pipeline.</param>
        /// <param name="env">Carries information about the current environment where the application is running.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
