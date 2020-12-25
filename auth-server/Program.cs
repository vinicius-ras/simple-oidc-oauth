using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace SimpleOidcOauth
{
    /// <summary>The main class of the application, which builds the web server and starts it up.</summary>
    public class Program
    {
        /// <summary>Application's entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Returns an integer with the application's result.</returns>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        /// <summary>Creates the web host and initializes its startup configurations.</summary>
        /// <param name="args">The command line arguments received by the application.</param>
        /// <returns>Returns an <see cref="IHostBuilder" /> reference representing the host being built.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}