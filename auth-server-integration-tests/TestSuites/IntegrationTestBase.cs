using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Services;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Services;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.TestSuites
{
	/// <summary>Base class for all implemented Integration Tests.</summary>
	public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Startup>>
	{
		// INSTANCE PROPERTIES
		/// <summary>Reference to an <see cref="WebApplicationFactory{TEntryPoint}"/>, injected by the test engine.</summary>
		protected WebApplicationFactory<Startup> WebAppFactory { get; init; }
		/// <summary>Reference to an <see cref="ITestOutputHelper"/>, injected by the test engine.</summary>
		protected ITestOutputHelper TestOutputHelper { get; }
		/// <summary>A <see cref="MockEmailService"/>, created for accessing sent email data during the tests (when necessary).</summary>
		protected MockEmailService MockEmailService { get; } = new MockEmailService();
		/// <summary>The base address used by the Test Server.</summary>
		protected string TestServerBaseAddress { get; }





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public IntegrationTestBase(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
		{
			TestOutputHelper = testOutputHelper;

			// Reconfigure the test host to prepare it for the tests
			TestServerBaseAddress = webAppFactory.Server.BaseAddress.AbsoluteUri.TrimEnd('/');
			WebAppFactory = webAppFactory.WithWebHostBuilder(builder => {
				// Use a custom/separate SQLite file to store the database for this class, and update the base-url to be considered for the Auth Server
				builder.ConfigureAppConfiguration((builderContext, configurationBuilder) => {
					string testSuiteName = this.GetType().Name;
					var customConfigs = new Dictionary<string,string> {
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerConfiguration}", $"Data Source={testSuiteName}-IdentityServerConfigs.sqlite;" },
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerOperational}", $"Data Source={testSuiteName}-IdentityServerOperational.sqlite;" },
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerUsers}", $"Data Source={testSuiteName}-IdentityServerUsers.sqlite;" },

						{ $"{AppConfigs.ConfigKey}:{nameof(AppConfigs.AuthServer)}:{nameof(AppConfigs.AuthServer.BaseUrl)}", TestServerBaseAddress },
					};
					configurationBuilder.AddInMemoryCollection(customConfigs);
				});


				// Configure custom services and database initialization for the Test Server
				builder.ConfigureServices(services => {
					// Configure the Test Server to use a stub email sevice
					var emailServices = services
						.Where(svc => svc.ServiceType == typeof(IEmailService))
						.ToList();
					foreach (var curService in emailServices)
						services.Remove(curService);
					services.AddSingleton<IEmailService>(this.MockEmailService);

					// Initialize the test database
					using (var serviceProvider = services.BuildServiceProvider())
					{
						TestData.ClearDatabaseAsync(serviceProvider).Wait();
						TestData.InitializeDatabaseAsync(serviceProvider).Wait();
					}
				});

				// Configure ILogger objects to use the ITestOutputHelper, which collects logs for unit/integration tests
				builder.ConfigureLogging(loggingBuilder => {
					loggingBuilder.ClearProviders();
					loggingBuilder.AddXUnit(TestOutputHelper);
				});
			});
		}
	}
}