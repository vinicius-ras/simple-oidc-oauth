// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleOidcOauth.Data;

namespace SimpleOidcOauth
{
    /// <summary>
    ///     Startup class for configuring the services available for the application's components,
    ///     as well as the HTTP request processing pipeline.
    /// </summary>
    public class Startup
    {
        // CONSTANTS
        /// <summary>Name of the file which stores the configurations database.</summary>
        private const string DbFileNameConfigurations = "identity-database-configs.sqlite";
        /// <summary>Name of the file which stores the operational database.</summary>
        private const string DbFileNameOperational = "identity-database-operational.sqlite";
        /// <summary>Name of the file which stores user-related data.</summary>
        private const string DbFileNameUsers = "identity-database-users.sqlite";


        // PRIVATE PROPERTIES
        /// <summary>Refererence to an <see cref="IWebHostEnvironment" /> object, injected by the container.</summary>
        /// <value>The <see cref="IWebHostEnvironment" /> object that has been injected by the container.</value>
        private readonly IWebHostEnvironment _webHostEnvironment;





        // PUBLIC METHODS
        /// <summary>Constructor.</summary>
        /// <param name="environment">Reference to an injected <see cref="IWebHostEnvironment"/>, provided by the container.</param>
        public Startup(IWebHostEnvironment environment)
        {
            _webHostEnvironment = environment;
        }


        /// <summary>
        ///     Configures the services of the application, which will then be available to be
        ///     injected by the container.
        /// </summary>
        /// <param name="services">The collection where services can be added for the container to know them.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Configure ASP.NET Core Identity, IdentityServer and the data stores to be used
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<AppDbContext>(appDbOptions => {
                appDbOptions.UseSqlite(
                    @$"Data Source={DbFileNameUsers};",
                    sqlServerOptions => sqlServerOptions.MigrationsAssembly(migrationsAssembly)
                );
            });


            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();


            var identityServerBuilder = services.AddIdentityServer()
                .AddConfigurationStore(configStoreOptions => {
                    configStoreOptions.ConfigureDbContext = genericDbOptions => {
                        genericDbOptions.UseSqlite(
                            @$"Data Source={DbFileNameConfigurations};",
                            sqlServerOptions => sqlServerOptions.MigrationsAssembly(migrationsAssembly)
                        );
                    };
                })
                .AddOperationalStore(opStoreOptions => {
                    opStoreOptions.ConfigureDbContext = genericDbOptions => {
                        genericDbOptions.UseSqlite(
                            @$"Data Source={DbFileNameOperational};",
                            sqlServerOptions => sqlServerOptions.MigrationsAssembly(migrationsAssembly)
                        );
                    };
                })
                .AddAspNetIdentity<IdentityUser>();


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
            if (_webHostEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                TestData.InitializeDatabase(app).Wait();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
               endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
