using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.Services;
using SimpleOidcOauth.Tests.Integration.Controllers;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.TestSuites.Services
{
	/// <summary>
	///     Integration tests for the <see cref="IDatabaseInitializerService"/>.
	///     The service is tested through the special controller: <see cref="TestDatabaseInitializerServiceController"/>.
	/// </summary>
	public class DatabaseInitializerServiceTests : IntegrationTestBase
	{
		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public DatabaseInitializerServiceTests(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
			: base(webAppFactory, testOutputHelper, TestDatabaseInitializationType.StructureOnly)
		{
		}


		/// <summary>Retrieves the default data to be saved which is used in the tests.</summary>
		/// <returns>Returns a new instance containing the default data.</returns>
		private TestDatabaseInitializerInputModel GetDefaultDataToSave() => new TestDatabaseInitializerInputModel()
		{
			Clients = new [] {
				Mapper.Map<SerializableClient>(TestData.ClientAuthorizationCodeFlowWithPkce),
				Mapper.Map<SerializableClient>(TestData.ClientResourceOwnerPasswordFlow),
			},
			ApiScopes = new [] {
				Mapper.Map<SerializableApiScope>(TestData.ApiScopeProductsApi),
				Mapper.Map<SerializableApiScope>(TestData.ApiScopeUserManagementApi)
			},
			ApiResources = new [] { Mapper.Map<SerializableApiResource>(TestData.ApiResourceProducts) },
			IdentityResources = new [] {
				Mapper.Map<SerializableIdentityResource>(new IdentityResources.OpenId()),
				Mapper.Map<SerializableIdentityResource>(TestData.IdentityResourceConfidentialUserInfo)
			},
			Users = new [] { Mapper.Map<SerializableTestUser>(TestData.UserBob) },
		};


		/// <summary>
		///     Runs the main Integration Tests, given the input data to work with.
		///     This method assumes valid data is provided to be used for initialization.
		/// </summary>
		/// <param name="dataToSave">The data that will be used to run the tests.</param>
		/// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
		private async Task RunTestsWithValidData(TestDatabaseInitializerInputModel dataToSave)
		{
			// Arrange
			var httpClient = WebAppFactory.CreateClient();


			// Act
			HttpResponseMessage initializationResponse = await httpClient.PostAsync(TestDatabaseInitializerServiceController.InitializeDatabaseEndpoint, JsonContent.Create(dataToSave)),
				allDataBeforeClearResponse = await httpClient.GetAsync(TestDatabaseInitializerServiceController.GetAllRegisteredDataEndpoint),
				clearAllDataResponse = await httpClient.GetAsync(TestDatabaseInitializerServiceController.ClearDatabaseEndpoint),
				allDataAfterClearResponse = await httpClient.GetAsync(TestDatabaseInitializerServiceController.GetAllRegisteredDataEndpoint);


			TestDatabaseInitializerOutputModel allRetrievedDataBeforeClear = await allDataBeforeClearResponse.Content.ReadFromJsonAsync<TestDatabaseInitializerOutputModel>(),
				allRetrievedDataAfterClear = await allDataAfterClearResponse.Content.ReadFromJsonAsync<TestDatabaseInitializerOutputModel>();


			// Assert
			Assert.Equal(HttpStatusCode.OK, initializationResponse.StatusCode);
			Assert.Equal(HttpStatusCode.OK, allDataBeforeClearResponse.StatusCode);
			Assert.Equal(HttpStatusCode.OK, clearAllDataResponse.StatusCode);
			Assert.Equal(HttpStatusCode.OK, allDataAfterClearResponse.StatusCode);

			Assert.Equal(dataToSave.Clients?.Count()           ?? 0, allRetrievedDataBeforeClear.Clients?.Count()           ?? 0);
			Assert.Equal(dataToSave.ApiScopes?.Count()         ?? 0, allRetrievedDataBeforeClear.ApiScopes?.Count()         ?? 0);
			Assert.Equal(dataToSave.ApiResources?.Count()      ?? 0, allRetrievedDataBeforeClear.ApiResources?.Count()      ?? 0);
			Assert.Equal(dataToSave.IdentityResources?.Count() ?? 0, allRetrievedDataBeforeClear.IdentityResources?.Count() ?? 0);
			Assert.Equal(dataToSave.Users?.Count()             ?? 0, allRetrievedDataBeforeClear.Users?.Count()             ?? 0);

			foreach (var clientToSave in dataToSave.Clients ?? Enumerable.Empty<SerializableClient>())
			{
				var savedClient = allRetrievedDataBeforeClear.Clients.Single(client => client.ClientName.Equals(clientToSave.ClientName));
				Assert.Equal(clientToSave.ClientId, savedClient.ClientId);
				Assert.Equal(clientToSave.ClientName, savedClient.ClientName);
				Assert.Equal(clientToSave.AllowAccessTokensViaBrowser, clientToSave.AllowAccessTokensViaBrowser);
				Assert.Equal(clientToSave.RequireClientSecret, clientToSave.RequireClientSecret);
				Assert.Equal(clientToSave.RequireConsent, clientToSave.RequireConsent);
				Assert.Equal(clientToSave.RequirePkce, clientToSave.RequirePkce);

				foreach (var corsOriginToSave in clientToSave.AllowedCorsOrigins ?? Enumerable.Empty<string>())
				{
					var savedCorsOrigin = savedClient.AllowedCorsOrigins.Single(origin => origin.Equals(corsOriginToSave));
					Assert.Equal(corsOriginToSave, savedCorsOrigin);
				}
				foreach (var grantTypeToSave in clientToSave.AllowedGrantTypes ?? Enumerable.Empty<string>())
				{
					var savedGrantType = savedClient.AllowedGrantTypes.Single(grantType => grantType.Equals(grantTypeToSave));
					Assert.Equal(grantTypeToSave, savedGrantType);
				}
				foreach (var scopeToSave in clientToSave.AllowedScopes ?? Enumerable.Empty<string>())
				{
					var savedScope = savedClient.AllowedScopes.Single(scope => scope.Equals(scopeToSave));
					Assert.Equal(scopeToSave, savedScope);
				}
				foreach (var postLogoutRedirectUriToSave in clientToSave.PostLogoutRedirectUris ?? Enumerable.Empty<string>())
				{
					var savedPostLogoutRedirectUri = savedClient.PostLogoutRedirectUris.Single(postLogoutRedirectUri => postLogoutRedirectUri.Equals(postLogoutRedirectUriToSave));
					Assert.Equal(postLogoutRedirectUriToSave, savedPostLogoutRedirectUri);
				}
				foreach (var redirectUriToSave in clientToSave.RedirectUris ?? Enumerable.Empty<string>())
				{
					var savedRedirectUri = savedClient.RedirectUris.Single(redirectUri => redirectUri.Equals(redirectUriToSave));
					Assert.Equal(redirectUriToSave, savedRedirectUri);
				}
				foreach (var secretToSave in clientToSave.ClientSecrets ?? Enumerable.Empty<SerializableSecret>())
				{
					var savedSecret = savedClient.ClientSecrets.SingleOrDefault(secret =>
						secret.Description == secretToSave.Description
						&& secret.Value == secretToSave.Value
						&& secret.Expiration == secretToSave.Expiration
						&& secret.Type == secretToSave.Type
					);
					Assert.NotNull(savedSecret);
				}
			}

			foreach (var apiScopeToSave in dataToSave.ApiScopes ?? Enumerable.Empty<SerializableApiScope>())
			{
				var savedApiScope = allRetrievedDataBeforeClear.ApiScopes.Single(apiScope => apiScope.Name.Equals(apiScopeToSave.Name));
				Assert.Equal(apiScopeToSave.Name, savedApiScope.Name);
				Assert.Equal(apiScopeToSave.DisplayName, savedApiScope.DisplayName);
			}

			foreach (var apiResourceToSave in dataToSave.ApiResources ?? Enumerable.Empty<SerializableApiResource>())
			{
				var savedApiResource = allRetrievedDataBeforeClear.ApiResources.Single(apiResource => apiResource.Name.Equals(apiResourceToSave.Name));
				Assert.Equal(apiResourceToSave.Name, savedApiResource.Name);
				Assert.Equal(apiResourceToSave.DisplayName, savedApiResource.DisplayName);
				Assert.Equal(apiResourceToSave.UserClaims?.Count() ?? 0, savedApiResource.UserClaims?.Count() ?? 0);
				foreach (var claimToSave in apiResourceToSave.UserClaims ?? Enumerable.Empty<string>())
					Assert.Contains(claimToSave, savedApiResource.UserClaims);
			}

			foreach (var identityResourceToSave in dataToSave.IdentityResources ?? Enumerable.Empty<SerializableIdentityResource>())
			{
				var savedApiResource = allRetrievedDataBeforeClear.IdentityResources.Single(identityResource => identityResource.Name.Equals(identityResourceToSave.Name));
				Assert.Equal(identityResourceToSave.Name, savedApiResource.Name);
				Assert.Equal(identityResourceToSave.DisplayName, savedApiResource.DisplayName);
				Assert.Equal(identityResourceToSave.Enabled, savedApiResource.Enabled);
				Assert.Equal(identityResourceToSave.Description, savedApiResource.Description);
				Assert.Equal(identityResourceToSave.ShowInDiscoveryDocument, savedApiResource.ShowInDiscoveryDocument);
				Assert.Equal(identityResourceToSave.Required, savedApiResource.Required);
				Assert.Equal(identityResourceToSave.Emphasize, savedApiResource.Emphasize);
				Assert.Equal(identityResourceToSave.UserClaims?.Count() ?? 0, savedApiResource.UserClaims?.Count() ?? 0);
				foreach (var claimToSave in identityResourceToSave.UserClaims ?? Enumerable.Empty<string>())
					Assert.Contains(claimToSave, savedApiResource.UserClaims);
			}

			foreach (var userToSave in dataToSave.Users ?? Enumerable.Empty<SerializableTestUser>())
			{
				var savedUser = allRetrievedDataBeforeClear.Users.Single(user => user.Username == userToSave.Username);
				Assert.Equal(userToSave.Username, savedUser.Username);

				Assert.Equal(userToSave.Claims?.Count() ?? 0, savedUser.Claims?.Count() ?? 0);
				foreach (var claimToSave in userToSave.Claims ?? Enumerable.Empty<SerializableClaim>())
				{
					var savedClaim = savedUser.Claims.SingleOrDefault(claim => claim.Type == claimToSave.Type && claim.Value == claimToSave.Value);
					Assert.NotNull(savedClaim);
				}
			}

			Assert.Empty(allRetrievedDataAfterClear.Clients);
			Assert.Empty(allRetrievedDataAfterClear.ApiScopes);
			Assert.Empty(allRetrievedDataAfterClear.ApiResources);
			Assert.Empty(allRetrievedDataAfterClear.IdentityResources);
			Assert.Empty(allRetrievedDataAfterClear.Users);
		}



		[Fact]
		public async Task InitializeAndCleanDatabase_ValidData_InitializesAndCleansDataCorrectly()
		{
			var dataToSave = GetDefaultDataToSave();
			await RunTestsWithValidData(dataToSave);
		}


		[Fact]
		public async Task InitializeAndCleanDatabase_ValidDataWithNullClients_InitializesAndCleansDataCorrectly()
		{
			var dataToSave = GetDefaultDataToSave();
			dataToSave.Clients = null;
			await RunTestsWithValidData(dataToSave);
		}


		[Fact]
		public async Task InitializeAndCleanDatabase_ValidDataWithNullApiScopes_InitializesAndCleansDataCorrectly()
		{
			var dataToSave = GetDefaultDataToSave();
			dataToSave.ApiScopes = null;
			await RunTestsWithValidData(dataToSave);
		}


		[Fact]
		public async Task InitializeAndCleanDatabase_ValidDataWithNullApiResources_InitializesAndCleansDataCorrectly()
		{
			var dataToSave = GetDefaultDataToSave();
			dataToSave.ApiResources = null;
			await RunTestsWithValidData(dataToSave);
		}


		[Fact]
		public async Task InitializeAndCleanDatabase_ValidDataWithNullIdentityResources_InitializesAndCleansDataCorrectly()
		{
			var dataToSave = GetDefaultDataToSave();
			dataToSave.IdentityResources = null;
			await RunTestsWithValidData(dataToSave);
		}


		[Fact]
		public async Task InitializeAndCleanDatabase_ValidDataWithNullUsers_InitializesAndCleansDataCorrectly()
		{
			var dataToSave = GetDefaultDataToSave();
			dataToSave.Users = null;
			await RunTestsWithValidData(dataToSave);
		}


		[Fact]
		public async Task InitializeDatabase_OneInvalidClientData_SavesEverythingExceptInvalidClients()
		{
			// Arrange
			var httpClient = WebAppFactory.CreateClient();
			var dataToSave = GetDefaultDataToSave();

			var firstClient = dataToSave.Clients.First();
			firstClient.ClientId = null;


			// Act
			HttpResponseMessage initializationResponse = await httpClient.PostAsync(TestDatabaseInitializerServiceController.InitializeDatabaseEndpoint, JsonContent.Create(dataToSave)),
				allSavedDataResponse = await httpClient.GetAsync(TestDatabaseInitializerServiceController.GetAllRegisteredDataEndpoint);

			TestDatabaseInitializerOutputModel allSavedData = await allSavedDataResponse.Content.ReadFromJsonAsync<TestDatabaseInitializerOutputModel>();

			// Assert
			Assert.Equal(HttpStatusCode.OK, initializationResponse.StatusCode);
			Assert.Equal(HttpStatusCode.OK, allSavedDataResponse.StatusCode);

			Assert.Equal(dataToSave.Clients.Count(), allSavedData.Clients.Count() + 1);
		}
	}
}