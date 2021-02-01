using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleOidcOauth.Services;
using SimpleOidcOauth.Tests.Integration.Controllers;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Models;
using SimpleOidcOauth.Tests.Integration.Models.DTO;
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
			: base(webAppFactory, testOutputHelper, false)
		{
		}


		[Fact]
		public async Task InitializeAndCleanDatabase_ValidData_InitializesAndCleansDataCorrectly()
		{
			// Arrange
			var dataToSave = new TestDatabaseInitializerInputModel()
			{
				Clients = new [] {
					new ClientDto(TestData.ClientAuthorizationCodeFlowWithPkce),
					new ClientDto(TestData.ClientResourceOwnerPasswordFlow),
				},
				ApiScopes = new [] { TestData.ApiScopeProductsApi, TestData.ApiScopeUserManagementApi },
				ApiResources = new [] { TestData.ApiResourceProducts },
				IdentityResources = new [] { new IdentityResources.OpenId(), TestData.IdentityResourceConfidentialUserInfo },
				Users = new [] { new UserDto(TestData.UserBob) },
			};
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

			Assert.Equal(dataToSave.Clients.Count(), allRetrievedDataBeforeClear.Clients.Count());
			Assert.Equal(dataToSave.ApiScopes.Count(), allRetrievedDataBeforeClear.ApiScopes.Count());
			Assert.Equal(dataToSave.ApiResources.Count(), allRetrievedDataBeforeClear.ApiResources.Count());
			Assert.Equal(dataToSave.IdentityResources.Count(), allRetrievedDataBeforeClear.IdentityResources.Count());
			Assert.Equal(dataToSave.Users.Count(), allRetrievedDataBeforeClear.Users.Count());

			foreach (var clientToSave in dataToSave.Clients)
			{
				var savedClient = allRetrievedDataBeforeClear.Clients.Single(client => client.ClientName.Equals(clientToSave.ClientName));
				Assert.Equal(clientToSave.ClientId, savedClient.ClientId);
				Assert.Equal(clientToSave.ClientName, savedClient.ClientName);

				foreach (var corsOriginToSave in clientToSave.AllowedCorsOrigins)
				{
					var savedCorsOrigin = savedClient.AllowedCorsOrigins.Single(origin => origin.Equals(corsOriginToSave));
					Assert.Equal(corsOriginToSave, savedCorsOrigin);
				}
				foreach (var grantTypeToSave in clientToSave.AllowedGrantTypes)
				{
					var savedGrantType = savedClient.AllowedGrantTypes.Single(grantType => grantType.Equals(grantTypeToSave));
					Assert.Equal(grantTypeToSave, savedGrantType);
				}
				foreach (var scopeToSave in clientToSave.AllowedScopes)
				{
					var savedScope = savedClient.AllowedScopes.Single(scope => scope.Equals(scopeToSave));
					Assert.Equal(scopeToSave, savedScope);
				}
				foreach (var postLogoutRedirectUriToSave in clientToSave.PostLogoutRedirectUris)
				{
					var savedPostLogoutRedirectUri = savedClient.PostLogoutRedirectUris.Single(postLogoutRedirectUri => postLogoutRedirectUri.Equals(postLogoutRedirectUriToSave));
					Assert.Equal(postLogoutRedirectUriToSave, savedPostLogoutRedirectUri);
				}
				foreach (var redirectUriToSave in clientToSave.RedirectUris)
				{
					var savedRedirectUri = savedClient.RedirectUris.Single(redirectUri => redirectUri.Equals(redirectUriToSave));
					Assert.Equal(redirectUriToSave, savedRedirectUri);
				}
			}

			Assert.Empty(allRetrievedDataAfterClear.Clients);
			Assert.Empty(allRetrievedDataAfterClear.ApiScopes);
			Assert.Empty(allRetrievedDataAfterClear.ApiResources);
			Assert.Empty(allRetrievedDataAfterClear.IdentityResources);
			Assert.Empty(allRetrievedDataAfterClear.Users);
		}
	}
}