using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Data.Serialization;
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
		// CONSTANTS
		/// <summary>The base address used by the Test Server.</summary>
		public const string TestServerOrigin = "https://simple-oidc-oauth-test-server-9cad20d0e9244c5691cb49222cbd09b1";
		/// <summary>An <see cref="Uri"/> representation of the base address (<see cref="TestServerOrigin"/>) used by the Test Server.</summary>
		public static readonly Uri TestServerBaseUri = new Uri($"{TestServerOrigin}");
		/// <summary>
		///     <para>A name for a JSON file that gets created to hold test data (see <see cref="TestData"/>) for the tests that rely on it.</para>
		///     <para>
		///         This file gets created/updated everytime the Integration Tests are run, containing a serialized form of the <see cref="TestData"/> data.
		///         It is then added to the list of Configuration Providers for the Test Server, which in turn allows the <see cref="DatabaseInitializerHostedService"/> to
		///         pick it up and configure a sample database for the Integration Tests.
		///     </para>
		/// </summary>
		public const string TestDataJsonFileName = "serialized-test-data.json";
		/// <summary>Full/absolute path to the <see cref="TestDataJsonFileName"/>.</summary>
		public static string TestDataJsonFullPath => Path.Combine(Directory.GetCurrentDirectory(), TestDataJsonFileName);





		// STATIC PROPERTIES
		/// <summary>An <see cref="IMapper"/>, with the same configuration specified by the <see cref="AutoMapperProfile"/> class.</summary>
		protected static IMapper Mapper { get; }





		// INSTANCE PROPERTIES
		/// <summary>Reference to an <see cref="WebApplicationFactory{TEntryPoint}"/>, injected by the test engine.</summary>
		protected WebApplicationFactory<Startup> WebAppFactory { get; init; }
		/// <summary>Reference to an <see cref="ITestOutputHelper"/>, injected by the test engine.</summary>
		protected ITestOutputHelper TestOutputHelper { get; }
		/// <summary>A <see cref="MockEmailService"/>, created for accessing sent email data during the tests (when necessary).</summary>
		protected MockEmailService MockEmailService { get; } = new MockEmailService();





		// STATIC METHODS
		/// <summary>Static constructor.</summary>
		static IntegrationTestBase()
		{
			// Create a mapper object
			var mapperConfigs = new MapperConfiguration(configs =>
			{
				configs.AddProfile<AutoMapperProfile>();
			});
			mapperConfigs.AssertConfigurationIsValid();
			Mapper = mapperConfigs.CreateMapper();


			// Write the Test Data used by integration tests into a configuration file
			using (var file = File.Create(TestDataJsonFullPath))
			using (var jsonWriter = new Utf8JsonWriter(file))
			{
				var configToSave = new {
					App = new {
						DatabaseInitialization = new {
							Clients = TestData.SampleClients,
							ApiScopes = TestData.SampleApiScopes,
							ApiResources = TestData.SampleApiResources,
							IdentityResources = TestData.SampleIdentityResources,
							Users = TestData.SampleUsers.Select(model => Mapper.Map<SerializableTestUser>(model)),
						},
					},
				};
				JsonSerializer.Serialize(jsonWriter, configToSave);
			}
		}





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		/// <param name="databaseInitializationType">
		///     A value indicating if and how a default test database should be initialized for the current test suite.
		///     The default test database's data is contained in the internal <see cref="TestData"/> class.
		/// </param>
		public IntegrationTestBase(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper, TestDatabaseInitializationType databaseInitializationType)
		{
			TestOutputHelper = testOutputHelper;

			// Reconfigure the test host to prepare it for the tests
			WebAppFactory = webAppFactory.WithWebHostBuilder(builder => {
				// Use a custom/separate SQLite file to store the database for this class, and update the base-url to be considered for the Auth Server
				builder.ConfigureAppConfiguration((builderContext, configurationBuilder) => {
					bool initializeDatabaseStructure = (databaseInitializationType != TestDatabaseInitializationType.None),
						initializeDatabaseWithTestData = (databaseInitializationType == TestDatabaseInitializationType.StructureAndTestData);
					string testSuiteName = this.GetType().Name;
					var customConfigs = new Dictionary<string,string> {
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerConfiguration}", $"Data Source={testSuiteName}-IdentityServerConfigs.sqlite;" },
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerOperational}", $"Data Source={testSuiteName}-IdentityServerOperational.sqlite;" },
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerUsers}", $"Data Source={testSuiteName}-IdentityServerUsers.sqlite;" },

						{ AppConfigs.GetAppConfigurationKey(configs => configs.AuthServer.BaseUrl), $"{TestServerOrigin}" },
						{ AppConfigs.GetAppConfigurationKey(configs => configs.DatabaseInitialization.CleanBeforeInitialize), "true" },
						{ AppConfigs.GetAppConfigurationKey(configs => configs.DatabaseInitialization.InitializeStructure), initializeDatabaseStructure.ToString().ToLower() },
						{ AppConfigs.GetAppConfigurationKey(configs => configs.DatabaseInitialization.InitializeData), initializeDatabaseWithTestData.ToString().ToLower() },
					};
					configurationBuilder.AddInMemoryCollection(customConfigs);

					// Add test data to the configuration (if required)
					if (initializeDatabaseWithTestData)
						configurationBuilder.AddJsonFile(TestDataJsonFullPath);
				});


				// Configure custom services and database initialization for the Test Server
				builder.ConfigureServices(services => {
					// Adds an Application Part referencing the Integration Tests project's assembly.
					// This allows us to inject special/customized test controllers, used by some of the Integration Tests.
					services.AddControllers()
						.AddApplicationPart(typeof(IntegrationTestBase).Assembly);

					// Configure the Test Server to use a stub email sevice
					var emailServices = services
						.Where(svc => svc.ServiceType == typeof(IEmailService))
						.ToList();
					foreach (var curService in emailServices)
						services.Remove(curService);
					services.AddSingleton<IEmailService>(this.MockEmailService);
				});

				// Configure ILogger objects to use the ITestOutputHelper, which collects logs for unit/integration tests
				builder.ConfigureLogging(loggingBuilder => {
					loggingBuilder.ClearProviders();
					loggingBuilder.AddXUnit(TestOutputHelper);
				});
			});

			// Set the base address of the Test Host and its clients
			WebAppFactory.Server.BaseAddress = TestServerBaseUri;
			WebAppFactory.ClientOptions.BaseAddress = TestServerBaseUri;
		}
	}
}