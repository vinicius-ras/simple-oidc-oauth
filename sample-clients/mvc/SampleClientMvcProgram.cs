using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SimpleOidcOauth.SampleClients.Mvc
{
    /// <summary>Class containing the entry point for the Sample MVC Client application.</summary>
    public class SampleClientMvcProgram
    {
        /// <summary>Application's entry point.</summary>
        /// <param name="args">Application's command line arguments (if any).</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        /// <summary>Builds the application's host, which will be used to run the application's server.</summary>
        /// <param name="args">Command line arguments (if any).</param>
        /// <returns>Returns the host's builder object.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<SampleClientMvcStartup>();
                });
    }
}
