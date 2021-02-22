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
            var oidcConfigs = _configs.GetSection(OpenIdConnectConfigs.SectionName)
                .Get<OpenIdConnectConfigs>();
            services.AddMvc();
            services
                .AddAuthentication(opts => {
                    opts.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = OpenIdConnectConfigs.AuthenticationSchemeName;
                })
                .AddCookie()
                .AddOpenIdConnect(OpenIdConnectConfigs.AuthenticationSchemeName, opts => {
                    opts.Authority = oidcConfigs.Authority;
                    opts.ClientId = oidcConfigs.ClientId;
                    opts.ClientSecret = oidcConfigs.ClientSecret;
                    opts.ResponseType = oidcConfigs.ResponseType;
                    foreach (var scope in oidcConfigs.RequiredScopes)
                        opts.Scope.Add(scope);
                    opts.UsePkce = oidcConfigs.UsePkce;

                    // Path to the endpoint where our client's OpenID Connect middleware will treat logout operations
                    opts.SignedOutCallbackPath = oidcConfigs.SignedOutCallbackPath;

                    // Path to where the user will be redirected after the OpenID Connect middleware finishes the logout operations.
                    // This can be either a local path (starting with "/") or an external path (e.g., "https://some-other-website.com/abc"), if desired.
                    opts.SignedOutRedirectUri = oidcConfigs.SignedOutRedirectUri;

                    // Saving the tokens is necessary for the OpenID Connect authentication scheme to pass an "id_token_hint" parameter
                    // to the IdP. In IdentityServer4, this is required in order to perform the correct "Post Logout Redirection"
                    // for the client application after the IdP performs the logout. Without the "id_token_hint", IdentityServer4
                    // will always return "null" as the "Post Logout Redirection URI" in a call to IIdentityServerInteractionService.GetLogoutContextAsync(),
                    // which will prevent the user from being redirected back to the client application after logout.
                    opts.SaveTokens = true;
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
