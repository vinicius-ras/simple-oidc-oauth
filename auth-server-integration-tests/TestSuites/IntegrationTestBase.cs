using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Tests.Integration.Data;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.TestSuites
{
	/// <summary>Base class for all implemented Integration Tests.</summary>
	public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Startup>>
	{
		// INSTANCE PROPERTIES
		/// <summary>Reference to an <see cref="WebApplicationFactory{TEntryPoint}"/>, injected by the test engine.</summary>
		protected WebApplicationFactory<Startup> WebAppFactory { get; }
		/// <summary>Reference to an <see cref="ITestOutputHelper"/>, injected by the test engine.</summary>
		protected ITestOutputHelper TestOutputHelper { get; }





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public IntegrationTestBase(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
		{
			TestOutputHelper = testOutputHelper;

			// Reconfigure the test host to prepare it for the tests
			var testServerBaseUri = webAppFactory.Server.BaseAddress;
			WebAppFactory = webAppFactory.WithWebHostBuilder(builder => {
				// Use a custom/separate SQLite file to store the database for this class, and update the base-url to be considered for the Auth Server
				builder.ConfigureAppConfiguration((builderContext, configurationBuilder) => {
					string testSuiteName = this.GetType().Name;
					var customConfigs = new Dictionary<string,string> {
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerConfiguration}", $"Data Source={testSuiteName}-IdentityServerConfigs.sqlite;" },
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerOperational}", $"Data Source={testSuiteName}-IdentityServerOperational.sqlite;" },
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerUsers}", $"Data Source={testSuiteName}-IdentityServerUsers.sqlite;" },

						{ $"{AppConfigs.ConfigKey}:{nameof(AppConfigs.AuthServer)}:{nameof(AppConfigs.AuthServer.BaseUrl)}", testServerBaseUri.AbsoluteUri.TrimEnd('/') },
					};
					configurationBuilder.AddInMemoryCollection(customConfigs);
				});


				// Initialize the test database
				builder.ConfigureServices(services => {
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