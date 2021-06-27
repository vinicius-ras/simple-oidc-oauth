using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleOidcOauth.Controllers;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Data.Security;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.Tests.Integration.Controllers;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Data.Comparers;
using SimpleOidcOauth.Tests.Integration.Exceptions;
using SimpleOidcOauth.Tests.Integration.Models;
using SimpleOidcOauth.Tests.Integration.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.TestSuites.Controllers
{
	/// <summary>Integration tests for the <see cref="ClientsManagementController"/>.</summary>
	public class ClientsManagementControllerTests : IntegrationTestBase
	{
		// CONSTANTS
		/// <summary>A test user that does not have any of the claims defined in <see cref="AuthServerClaimTypes"/>.</summary>
		/// <remarks>
		///     This user effectivelly simulates a non-manager user. Such users are only able to access third-party services through the IdP, but
		///     have no access to the IdP Management Interfaces.
		/// </remarks>
		private static readonly SerializableTestUser _userWithoutAuthServerClaims = new SerializableTestUser {
			Username = "user-without-auth-server-claims",
			Password = "password-b9f2e7753f024c1eb508e62be83ffe06",
			Claims = new[] {
				new SerializableClaim { Type = JwtClaimTypes.Name, Value = "User Name WithoutAuthServerClaims" },
				new SerializableClaim { Type = JwtClaimTypes.Email, Value = "user-WithoutAuthServerClaims@fakemail-e1998119ca594bc49f62014aabded699.com" },
				new SerializableClaim { Type = JwtClaimTypes.EmailVerified, Value = "true" },
			}
		};
		/// <summary>A test user that has the <see cref="AuthServerClaimTypes.CanViewClients"/> claim.</summary>
		private static readonly SerializableTestUser _userCanViewClients = new SerializableTestUser {
			Username = "user-can-view-clients",
			Password = "password-6b72b83a2daf46bfb23dd460af89a52f",
			Claims = new[] {
				new SerializableClaim { Type = JwtClaimTypes.Name, Value = "User Name CanViewClients" },
				new SerializableClaim { Type = JwtClaimTypes.Email, Value = "user-CanViewClients@fakemail-f52da03fe41441069824bd8f510492bf.com" },
				new SerializableClaim { Type = JwtClaimTypes.EmailVerified, Value = "true" },
				new SerializableClaim { Type = AuthServerClaimTypes.CanViewClients, Value = "true" },
			}
		};
		/// <summary>A test user that has the <see cref="AuthServerClaimTypes.CanViewAndEditClients"/> claim.</summary>
		private static readonly SerializableTestUser _userCanViewAndEditClients = new SerializableTestUser {
			Username = "user-can-view-and-edit-clients",
			Password = "password-eccf7d5c26f54e5aa3c28d05df276e61",
			Claims = new[] {
				new SerializableClaim { Type = JwtClaimTypes.Name, Value = "User Name CanViewAndEditClients" },
				new SerializableClaim { Type = JwtClaimTypes.Email, Value = "user-CanViewAndEditClients@fakemail-a0de0bdd2d4c4fe9ba873ea5bd5cd053.com" },
				new SerializableClaim { Type = JwtClaimTypes.EmailVerified, Value = "true" },
				new SerializableClaim { Type = AuthServerClaimTypes.CanViewAndEditClients, Value = "true" },
			}
		};
		/// <summary>Some sample resources to be inserted and used in the tests' databases.</summary>
		private static readonly SerializableResource[] _sampleResources = new SerializableResource[] {
			new SerializableApiScope {
				Enabled = true,
				ShowInDiscoveryDocument = true,
				Name = "Sample API Scope #70bd975e361741759ff17827b51acdf1",
				DisplayName = "Display Name for Sample API Scope #70bd975e361741759ff17827b51acdf1",
				Description = "Description for Sample API Scope #70bd975e361741759ff17827b51acdf1",
				UserClaims = new[] {
					"api-scope-70bd975e361741759ff17827b51acdf1-claim-1",
					"api-scope-70bd975e361741759ff17827b51acdf1-claim-2",
					"api-scope-70bd975e361741759ff17827b51acdf1-claim-3",
				},
				Properties = new Dictionary<string, string> {
					{ "api-scope-70bd975e361741759ff17827b51acdf1-property-1", "Resource #70bd975e361741759ff17827b51acdf1 property value 1" },
					{ "api-scope-70bd975e361741759ff17827b51acdf1-property-2", "Resource #70bd975e361741759ff17827b51acdf1 property value 2" },
					{ "api-scope-70bd975e361741759ff17827b51acdf1-property-3", "Resource #70bd975e361741759ff17827b51acdf1 property value 3" },
				},
				Emphasize = true,
				Required = true,
			},
			new SerializableApiScope {
				Enabled = false,
				ShowInDiscoveryDocument = false,
				Name = "Sample API Scope #8963ef2f44d04eee896ca10338f24882",
				DisplayName = "Display Name for Sample API Scope #8963ef2f44d04eee896ca10338f24882",
				Description = "Description for Sample API Scope #8963ef2f44d04eee896ca10338f24882",
				UserClaims = new[] {
					"api-scope-8963ef2f44d04eee896ca10338f24882-claim-1",
					"api-scope-8963ef2f44d04eee896ca10338f24882-claim-2",
				},
				Properties = new Dictionary<string, string> {
					{ "api-scope-8963ef2f44d04eee896ca10338f24882-property-1", "Resource #8963ef2f44d04eee896ca10338f24882 property value 1" },
				},
				Emphasize = true,
				Required = true,
			},
			new SerializableApiResource {
				Enabled = true,
				ShowInDiscoveryDocument = true,
				Name = "Sample API Resource #eb3709b7400c446bb56ce12243e3880f",
				DisplayName = "Display Name for Sample API Resource #eb3709b7400c446bb56ce12243e3880f",
				Description = "Description for Sample API Resource #eb3709b7400c446bb56ce12243e3880f",
				UserClaims = new[] {
					"api-resource-eb3709b7400c446bb56ce12243e3880f-claim-1",
					"api-resource-eb3709b7400c446bb56ce12243e3880f-claim-2",
					"api-resource-eb3709b7400c446bb56ce12243e3880f-claim-3",
				},
				Properties = new Dictionary<string, string> {
					{ "api-resource-eb3709b7400c446bb56ce12243e3880f-property-1", "Resource #eb3709b7400c446bb56ce12243e3880f property value 1" },
					{ "api-resource-eb3709b7400c446bb56ce12243e3880f-property-2", "Resource #eb3709b7400c446bb56ce12243e3880f property value 2" },
					{ "api-resource-eb3709b7400c446bb56ce12243e3880f-property-3", "Resource #eb3709b7400c446bb56ce12243e3880f property value 3" },
				},
				AllowedAccessTokenSigningAlgorithms = null,
				Scopes = new[] {
					"api-resource-eb3709b7400c446bb56ce12243e3880f-scope-1",
					"api-resource-eb3709b7400c446bb56ce12243e3880f-scope-2",
					"api-resource-eb3709b7400c446bb56ce12243e3880f-scope-3",
				},
				ApiSecrets = new[] {
					new SerializableSecret {
						Description = "Description for Sample Secret #1 for resource #eb3709b7400c446bb56ce12243e3880f",
						Expiration = new DateTime(2000, 8, 20),
						IsValueHashed = true,
						Value = "random-secret-value-70063fcc76d04dfeb5c12de080254d9d",
						Type = IdentityServerConstants.SecretTypes.SharedSecret,
					},
					new SerializableSecret {
						Description = "Description for Sample Secret #2 for resource #eb3709b7400c446bb56ce12243e3880f",
						Expiration = null,
						IsValueHashed = false,
						Value = "random-secret-value-ab23d77771db424389b8065086b653d2",
						Type = IdentityServerConstants.SecretTypes.JsonWebKey,
					},
				}
			},
			new SerializableApiResource {
				Enabled = false,
				ShowInDiscoveryDocument = false,
				Name = "Sample API Resource #bb382de595be42ff8fdd53d9fa6a062d",
				DisplayName = "Display Name for Sample API Resource #bb382de595be42ff8fdd53d9fa6a062d",
				Description = "Description for Sample API Resource #bb382de595be42ff8fdd53d9fa6a062d",
				UserClaims = new[] {
					"api-resource-bb382de595be42ff8fdd53d9fa6a062d-claim-1",
					"api-resource-bb382de595be42ff8fdd53d9fa6a062d-claim-2",
				},
				Properties = new Dictionary<string, string> {
					{ "api-resource-bb382de595be42ff8fdd53d9fa6a062d-property-1", "Resource #bb382de595be42ff8fdd53d9fa6a062d property value 1" },
				},
				AllowedAccessTokenSigningAlgorithms = null,
				Scopes = new[] {
					"api-resource-bb382de595be42ff8fdd53d9fa6a062d-scope-1",
					"api-resource-bb382de595be42ff8fdd53d9fa6a062d-scope-2",
				},
				ApiSecrets = new[] {
					new SerializableSecret {
						Description = "Description for Sample Secret #1 for resource #bb382de595be42ff8fdd53d9fa6a062d",
						Expiration = new DateTime(2000, 8, 20),
						IsValueHashed = true,
						Value = "random-secret-value-70063fcc76d04dfeb5c12de080254d9d",
						Type = IdentityServerConstants.SecretTypes.SharedSecret,
					},
				}
			},
			new SerializableIdentityResource {
				Enabled = true,
				ShowInDiscoveryDocument = true,
				Name = "Sample Identity Resource #72f8ce3fe2284aaabc327d0a2c3205fb",
				DisplayName = "Display Name for Sample API Resource #72f8ce3fe2284aaabc327d0a2c3205fb",
				Description = "Description for Sample API Resource #72f8ce3fe2284aaabc327d0a2c3205fb",
				Emphasize = true,
				Required = true,
				UserClaims = new[] {
					"api-resource-72f8ce3fe2284aaabc327d0a2c3205fb-claim-1",
					"api-resource-72f8ce3fe2284aaabc327d0a2c3205fb-claim-2",
					"api-resource-72f8ce3fe2284aaabc327d0a2c3205fb-claim-3",
				},
				Properties = new Dictionary<string, string> {
					{ "api-resource-72f8ce3fe2284aaabc327d0a2c3205fb-property-1", "Resource #72f8ce3fe2284aaabc327d0a2c3205fb property value 1" },
					{ "api-resource-72f8ce3fe2284aaabc327d0a2c3205fb-property-2", "Resource #72f8ce3fe2284aaabc327d0a2c3205fb property value 2" },
					{ "api-resource-72f8ce3fe2284aaabc327d0a2c3205fb-property-3", "Resource #72f8ce3fe2284aaabc327d0a2c3205fb property value 3" },
				},
			},
			new SerializableIdentityResource {
				Enabled = false,
				ShowInDiscoveryDocument = false,
				Name = "Sample Identity Resource #c6406849b10643ff893d8c1b6797333f",
				DisplayName = "Display Name for Sample API Resource #c6406849b10643ff893d8c1b6797333f",
				Description = "Description for Sample API Resource #c6406849b10643ff893d8c1b6797333f",
				Emphasize = false,
				Required = false,
				UserClaims = new[] {
					"api-resource-c6406849b10643ff893d8c1b6797333f-claim-1",
					"api-resource-c6406849b10643ff893d8c1b6797333f-claim-2",
				},
				Properties = new Dictionary<string, string> {
					{ "api-resource-c6406849b10643ff893d8c1b6797333f-property-1", "Resource #c6406849b10643ff893d8c1b6797333f property value 1" },
				},
			},
		};





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public ClientsManagementControllerTests(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
			: base(webAppFactory, testOutputHelper, TestDatabaseInitializationType.StructureOnly)
		{
		}


		/// <summary>Initializes the database for the integration tests performed by this class.</summary>
		/// <param name="withSampleClients">
		///     <para>A flag indicating if sample clients should be initialized in the database.</para>
		///     <para>
		///         A value of <c>false</c> indicates no sample client data will be used to initialize the database.
		///         This can be used for test instances which require an empty list of registered clients (e.g., to make it easier to test for
		///         the registration of new client applications).
		///     </para>
		/// </param>
		/// <returns>Returns a <see cref="Task"/> representing the asynchronous operation which will initialize the database.</returns>
		private async Task SetupDatabaseForTests(bool withSampleClients)
		{
			var client = WebAppFactory.CreateClient();

			// Initialize database
			var autoMapperConfigs = new MapperConfiguration(configs =>
			{
				configs.AddProfile<AutoMapperProfile>();
			});
			autoMapperConfigs.AssertConfigurationIsValid();
			var objectsMapper = autoMapperConfigs.CreateMapper();


			var initializationData = new TestDatabaseInitializerInputModel {
				Users = new[] {
					_userCanViewClients,
					_userWithoutAuthServerClaims,
					_userCanViewAndEditClients
				},
				Clients = withSampleClients
					? TestData.SampleClients.Select(client => objectsMapper.Map<SerializableClient>(client))
					: Enumerable.Empty<SerializableClient>(),
				ApiScopes = _sampleResources.OfType<SerializableApiScope>().Select(apiScope => objectsMapper.Map<SerializableApiScope>(apiScope)),
				ApiResources = _sampleResources.OfType<SerializableApiResource>().Select(apiResource => objectsMapper.Map<SerializableApiResource>(apiResource)),
				IdentityResources = _sampleResources.OfType<SerializableIdentityResource>().Select(identityResource => objectsMapper.Map<SerializableIdentityResource>(identityResource)),
			};
			var response = await client.PostAsync(TestDatabaseInitializerServiceController.InitializeDatabaseEndpoint, JsonContent.Create(initializationData));
			if (response.IsSuccessStatusCode == false)
				throw new IntegrationTestInitializationException($@"Failed to setup database for test: database initialization endpoint returned a non-successful HTTP Status Code (code={response.StatusCode.ToString("D")} ""{response.StatusCode.ToString()}"").");
		}


		/// <summary>Compares a <see cref="Client"/> object with a <see cref="SerializableClient"/> to verify for their equality.</summary>
		/// <param name="identityServerClient">The IdentityServer's representation of the client to be compared.</param>
		/// <param name="serializableClient">The auth-server's representation of the client to be compared.</param>
		/// <returns>Returns a flag indicating if both clients are to be considered equal.</returns>
		private static bool AreClientsEqual(Client identityServerClient, SerializableClient serializableClient)
		{
			// Compare basic data
			if (identityServerClient.AllowAccessTokensViaBrowser != serializableClient.AllowAccessTokensViaBrowser
				|| identityServerClient.ClientName != serializableClient.ClientName
				|| identityServerClient.RequireClientSecret != serializableClient.RequireClientSecret
				|| identityServerClient.RequireConsent != serializableClient.RequireConsent
				|| identityServerClient.RequirePkce != serializableClient.RequirePkce)
			{
				return false;
			}

			// Compare collections of strings
			if ( AreCollectionsSetEqual(identityServerClient.AllowedCorsOrigins, serializableClient.AllowedCorsOrigins) == false
				|| AreCollectionsSetEqual(identityServerClient.AllowedGrantTypes, serializableClient.AllowedGrantTypes) == false
				|| AreCollectionsSetEqual(identityServerClient.AllowedScopes, serializableClient.AllowedScopes) == false
				|| AreCollectionsSetEqual(identityServerClient.PostLogoutRedirectUris, serializableClient.PostLogoutRedirectUris) == false
				|| AreCollectionsSetEqual(identityServerClient.RedirectUris, serializableClient.RedirectUris) == false)
			{
				return false;
			}

			// Compare secrets
			foreach (var curSampleSecret in identityServerClient.ClientSecrets)
			{
				var incomingResponseClientSecret = serializableClient.ClientSecrets
					.FirstOrDefault(secret =>
						secret.Type == curSampleSecret.Type
						&& secret.Description == curSampleSecret.Description
						&& secret.Expiration == curSampleSecret.Expiration);
				if (incomingResponseClientSecret == null || incomingResponseClientSecret.Value != curSampleSecret.Value)
					return false;
			}

			// All comparisons passed: clients are equal.
			return true;
		}


		/// <summary>Utility method for comparing two sets of strings to verify for set equality.</summary>
		/// <param name="collection1">The first collection of strings.</param>
		/// <param name="collection2">The second collection of strings.</param>
		/// <returns>Returns a flag indicating if both passed collections are to be considered</returns>
		private static bool AreCollectionsSetEqual(IEnumerable<string> collection1, IEnumerable<string> collection2)
			=> collection1.ToHashSet().SetEquals(collection2.ToHashSet());





		// TESTS
		#region ENDPOINT: GetAllClients()
		[Fact]
		public async Task GetAllClients_UnauthenticatedUser_ReturnsUnauthorizedResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);
			var httpClient = WebAppFactory.CreateClient();

			// Act
			var getAllClientsHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAllRegisteredClients);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, getAllClientsHttpResponse.StatusCode);
		}


		[Fact]
		public async Task GetAllClients_AuthenticatedAndUnauthorizedUser_ReturnsForbiddenResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			var httpClient = WebAppFactory.CreateClient();

			var userToLogin = _userWithoutAuthServerClaims;
			var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);
			var getAllClientsHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAllRegisteredClients);

			// Assert
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.Equal(HttpStatusCode.Forbidden, getAllClientsHttpResponse.StatusCode);
		}


		[Fact]
		public async Task GetAllClients_AuthenticatedAndAuthorizedUser_ReturnsOkResponseAndAllCorrectData()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: true);

			var usersToTest = new[] { _userCanViewClients, _userCanViewAndEditClients };
			foreach (var userToLogin in usersToTest)
			{
				var httpClient = WebAppFactory.CreateClient();

				var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

				// Act
				var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);
				var getAllClientsHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAllRegisteredClients);
				var getAllClientsHttpResponseClients = await getAllClientsHttpResponse.Content.ReadFromJsonAsync<IEnumerable<SerializableClient>>();

				// Assert
				Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
				Assert.Equal(HttpStatusCode.OK, getAllClientsHttpResponse.StatusCode);
				Assert.Equal(TestData.SampleClients.Count(), getAllClientsHttpResponseClients.Count());

				foreach (var curSampleClient in TestData.SampleClients)
				{
					var incomingResponseClient = getAllClientsHttpResponseClients.Single(responseClient => responseClient.ClientId == curSampleClient.ClientId);

					bool clientsAreEqual = AreClientsEqual(curSampleClient, incomingResponseClient);
					Assert.True(clientsAreEqual);
				}
			}
		}
		#endregion


		#region ENDPOINT: GetClient()
		[Fact]
		public async Task GetClient_UnauthenticatedUser_ReturnsUnauthorizedResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			var httpClient = WebAppFactory.CreateClient();
			var clientToTest = TestData.SampleClients.First();

			// Act
			string targetEndpoint = AppEndpoints.GetRegisteredClient
				.Replace($@"{{{AppEndpoints.ClientIdParameterName}}}", clientToTest.ClientId);
			var getClientHttpResponse = await httpClient.GetAsync(targetEndpoint);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, getClientHttpResponse.StatusCode);
		}


		[Fact]
		public async Task GetClient_AuthenticatedAndUnauthorizedUser_ReturnsForbiddenResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			var httpClient = WebAppFactory.CreateClient();

			var userToLogin = _userWithoutAuthServerClaims;
			var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			var clientToTest = TestData.SampleClients.First();

			// Act
			var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);

			string targetEndpoint = AppEndpoints.GetRegisteredClient
				.Replace($@"{{{AppEndpoints.ClientIdParameterName}}}", clientToTest.ClientId);
			var getClientHttpResponse = await httpClient.GetAsync(targetEndpoint);

			// Assert
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.Equal(HttpStatusCode.Forbidden, getClientHttpResponse.StatusCode);
		}


		[Fact]
		public async Task GetClient_AuthenticatedAndAuthorizedUserWithExistingTargetClient_ReturnsOkResponseAndAllCorrectData()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: true);

			var usersToTest = new[] { _userCanViewClients, _userCanViewAndEditClients };
			foreach (var userToLogin in usersToTest)
			{
				var httpClient = WebAppFactory.CreateClient();

				var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

				// Act
				var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);

				foreach (var clientToTest in TestData.SampleClients)
				{
					string targetEndpoint = AppEndpoints.GetRegisteredClient
						.Replace($@"{{{AppEndpoints.ClientIdParameterName}}}", clientToTest.ClientId);
					var getClientHttpResponse = await httpClient.GetAsync(targetEndpoint);
					var getClientHttpResponseClient = await getClientHttpResponse.Content.ReadFromJsonAsync<SerializableClient>();

					// Assert
					Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
					Assert.Equal(HttpStatusCode.OK, getClientHttpResponse.StatusCode);
					Assert.True(AreClientsEqual(clientToTest, getClientHttpResponseClient));
				}
			}
		}


		[Fact]
		public async Task GetClient_AuthenticatedAndAuthorizedUserWithNonExistingTargetClient_ReturnsNotFoundResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: true);

			const string nonExistingClientId = "random-non-existing-client-id-89cdb0ce9f70445790740936f2da1b78";

			var usersToTest = new[] { _userCanViewClients, _userCanViewAndEditClients };
			foreach (var userToLogin in usersToTest)
			{
				var httpClient = WebAppFactory.CreateClient();

				var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

				// Act
				var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);

				string targetEndpoint = AppEndpoints.GetRegisteredClient
					.Replace($@"{{{AppEndpoints.ClientIdParameterName}}}", nonExistingClientId);
				var getClientHttpResponse = await httpClient.GetAsync(targetEndpoint);

				// Assert
				Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
				Assert.Equal(HttpStatusCode.NotFound, getClientHttpResponse.StatusCode);
			}
		}
		#endregion


		#region ENDPOINT: GetAllowedClientRegistrationGrantTypes()
		[Fact]
		public async Task GetAllowedClientRegistrationGrantTypes_UnauthenticatedUser_ReturnsUnauthorizedResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);
			var httpClient = WebAppFactory.CreateClient();

			// Act
			var getGrantTypesHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAllowedClientRegistrationGrantTypes);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, getGrantTypesHttpResponse.StatusCode);
		}


		[Fact]
		public async Task GetAllowedClientRegistrationGrantTypes_AuthenticatedAndUnauthorizedUser_ReturnsForbiddenResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			var httpClient = WebAppFactory.CreateClient();

			var userToLogin = _userWithoutAuthServerClaims;
			var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);
			var getGrantTypesHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAllowedClientRegistrationGrantTypes);

			// Assert
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.Equal(HttpStatusCode.Forbidden, getGrantTypesHttpResponse.StatusCode);
		}


		[Fact]
		public async Task GetAllowedClientRegistrationGrantTypes_AuthenticatedAndAuthorizedUser_ReturnsOkResponseAndAllCorrectData()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			var usersToTest = new[] { _userCanViewClients, _userCanViewAndEditClients };
			foreach (var userToLogin in usersToTest)
			{
				var httpClient = WebAppFactory.CreateClient();

				var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

				// Act
				var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);
				var getGrantTypesHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAllowedClientRegistrationGrantTypes);
				var getGrantTypesHttpResponseGrantTypes = await getGrantTypesHttpResponse.Content.ReadFromJsonAsync<IEnumerable<string>>();

				// Assert
				Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
				Assert.Equal(HttpStatusCode.OK, getGrantTypesHttpResponse.StatusCode);
				Assert.True(AreCollectionsSetEqual(AppConfigs.AllowedClientRegistrationGrantTypes, getGrantTypesHttpResponseGrantTypes));
			}
		}
		#endregion


		#region ENDPOINT: GetAvailableResources()
		[Fact]
		public async Task GetAvailableResources_UnauthenticatedUser_ReturnsUnauthorizedResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			var httpClient = WebAppFactory.CreateClient();

			// Act
			var getAvailableResourcesHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAvailableClientRegistrationResources);

			// Assert
			Assert.Equal(HttpStatusCode.Unauthorized, getAvailableResourcesHttpResponse.StatusCode);
		}


		[Fact]
		public async Task GetAvailableResources_AuthenticatedAndUnauthorizedUser_ReturnsForbiddenResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			var httpClient = WebAppFactory.CreateClient();

			var userToLogin = _userWithoutAuthServerClaims;
			var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);

			var getAvailableResourcesHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAvailableClientRegistrationResources);

			// Assert
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.Equal(HttpStatusCode.Forbidden, getAvailableResourcesHttpResponse.StatusCode);
		}


		[Fact]
		public async Task GetAvailableResources_AuthenticatedAndAuthorizedUserWithExistingTargetClient_ReturnsOkResponseAndAllCorrectData()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
			jsonSerializerOptions.Converters.Add(new PolymorphicDiscriminatorJsonConverterFactory());

			var allSampleApiScopes = _sampleResources.OfType<SerializableApiScope>();
			var allSampleApiResources = _sampleResources.OfType<SerializableApiResource>();
			var allSampleIdentityResources = _sampleResources.OfType<SerializableIdentityResource>();

			var apiScopeEqualityComparer = new SerializableApiScopeEqualityComparer {
				CompareProperties = false,
				CompareUserClaims = false,
			};
			var apiResourceEqualityComparer = new SerializableApiResourceEqualityComparer {
				CompareAllowedAccessTokenSigningAlgorithms = false,
				CompareApiSecrets = false,
				CompareProperties = false,
				CompareUserClaims = false,
				CompareScopes = false,
			};
			var identityResourceEqualityComparer = new SerializableIdentityResourceEqualityComparer {
				CompareProperties = false,
				CompareUserClaims = false,
			};

			var usersToTest = new[] { _userCanViewClients, _userCanViewAndEditClients };
			foreach (var userToLogin in usersToTest)
			{
				var httpClient = WebAppFactory.CreateClient();

				var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;


				// Act
				var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);

				var getAvailableResourcesHttpResponse = await httpClient.GetAsync(AppEndpoints.GetAvailableClientRegistrationResources);
				var responseStream = await getAvailableResourcesHttpResponse.Content.ReadAsStreamAsync();
				var returnedResources = await JsonSerializer.DeserializeAsync<IEnumerable<SerializableResource>>(responseStream, jsonSerializerOptions);


				// Assert
				Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
				Assert.Equal(HttpStatusCode.OK, getAvailableResourcesHttpResponse.StatusCode);
				foreach (var resource in returnedResources)
				{
					if (resource is SerializableApiScope apiScope)
						Assert.Contains(apiScope, allSampleApiScopes, apiScopeEqualityComparer);
					else if (resource is SerializableApiResource apiResource)
						Assert.Contains(apiResource, allSampleApiResources, apiResourceEqualityComparer);
					else if (resource is SerializableIdentityResource identityResource)
						Assert.Contains(identityResource, allSampleIdentityResources, identityResourceEqualityComparer);
					else
						throw new NotImplementedException($@"No assertions implemented for resources of type {resource.GetType().Name}");
				}
			}
		}
		#endregion


		#region ENDPOINT: CreateNewClientApplication()
		[Fact]
		public async Task CreateNewClientApplication_UnauthenticatedUser_ReturnsUnauthorizedResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);


			foreach (var clientApp in TestData.SampleClients)
			{
				var httpClient = WebAppFactory.CreateClient();

				// Act
				var createNewClientHttpResponse = await httpClient.PostAsync(AppEndpoints.CreateNewClientApplication, JsonContent.Create(clientApp));

				// Assert
				Assert.Equal(HttpStatusCode.Unauthorized, createNewClientHttpResponse.StatusCode);
			}
		}


		[Fact]
		public async Task CreateNewClientApplication_AuthenticatedAndUnauthorizedUser_ReturnsForbiddenResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: false);

			foreach (var clientApp in TestData.SampleClients)
			{
				var usersToTest = new[] { _userWithoutAuthServerClaims, _userCanViewClients };
				foreach (var userToLogin in usersToTest)
				{
					var httpClient = WebAppFactory.CreateClient();

					var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

					// Act
					var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);
					var createNewClientHttpResponse = await httpClient.PostAsync(AppEndpoints.CreateNewClientApplication, JsonContent.Create(clientApp));

					// Assert
					Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
					Assert.Equal(HttpStatusCode.Forbidden, createNewClientHttpResponse.StatusCode);
				}
			}
		}


		[Fact]
		public async Task CreateNewClientApplication_AuthenticatedAndAuthorizedUserWithValidData_ReturnsCreatedResponseAndClientData()
		{
			// ***** Arrange *****
			await SetupDatabaseForTests(withSampleClients: false);
			var clientsEqualityComparer = new SerializableClientEqualityComparer
			{
				CompareClientId = false,
			};

			foreach (var clientApp in TestData.SampleClients)
			{
				var httpClient = WebAppFactory.CreateClient();

				var userToLogin = _userCanViewAndEditClients;
				var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

				// New client applications should not have a set "Client ID", as it will be automatically generated by the IdP after client registration
				var serializableClientApp = Mapper.Map<SerializableClient>(clientApp);
				serializableClientApp.ClientId = null;


				// ***** Act *****
				var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);
				var createNewClientHttpResponse = await httpClient.PostAsync(AppEndpoints.CreateNewClientApplication, JsonContent.Create(serializableClientApp));
				var returnedClientData = await createNewClientHttpResponse.Content.ReadFromJsonAsync<SerializableClient>();


				// ***** Assert *****
				Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
				Assert.Equal(HttpStatusCode.Created, createNewClientHttpResponse.StatusCode);
				Assert.Equal(serializableClientApp, returnedClientData, clientsEqualityComparer);
				Assert.NotNull(returnedClientData.ClientId);
				Assert.NotEmpty(returnedClientData.ClientId);
			}
		}


		[Fact]
		public async Task CreateNewClientApplication_DataContainingClientId_ReturnsBadRequest()
		{
			// ***** Arrange *****
			await SetupDatabaseForTests(withSampleClients: false);

			foreach (var clientApp in TestData.SampleClients)
			{
				var httpClient = WebAppFactory.CreateClient();

				var userToLogin = _userCanViewAndEditClients;
				var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

				var serializableClientApp = Mapper.Map<SerializableClient>(clientApp);


				// ***** Act *****
				var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);
				var createNewClientHttpResponse = await httpClient.PostAsync(AppEndpoints.CreateNewClientApplication, JsonContent.Create(serializableClientApp));


				// ***** Assert *****
				Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
				Assert.Equal(HttpStatusCode.BadRequest, createNewClientHttpResponse.StatusCode);
			}
		}
		#endregion


		#region ENDPOINT: UpdateClientApplication()
		[Fact]
		public async Task UpdateClientApplication_UnauthenticatedUser_ReturnsUnauthorizedResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: true);


			foreach (var clientApp in TestData.SampleClients)
			{
				var httpClient = WebAppFactory.CreateClient();

				// Act
				var createNewClientHttpResponse = await httpClient.PutAsync(AppEndpoints.UpdateClientApplication, null);

				// Assert
				Assert.Equal(HttpStatusCode.Unauthorized, createNewClientHttpResponse.StatusCode);
			}
		}


		[Fact]
		public async Task UpdateClientApplication_AuthenticatedAndUnauthorizedUser_ReturnsForbiddenResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: true);

			var usersToTest = new[] { _userWithoutAuthServerClaims, _userCanViewClients };
			foreach (var userToLogin in usersToTest)
			{
				var httpClient = WebAppFactory.CreateClient();
				var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

				// Act
				var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);
				var createNewClientHttpResponse = await httpClient.PutAsync(AppEndpoints.UpdateClientApplication, null);

				// Assert
				Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
				Assert.Equal(HttpStatusCode.Forbidden, createNewClientHttpResponse.StatusCode);
			}
		}


		[Fact]
		public async Task UpdateClientApplication_AuthenticatedAndAuthorizedUserWithValidData_ReturnsOkResponseAndClientData()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: true);

			var httpClient = WebAppFactory.CreateClient();

			var userToLogin = _userCanViewAndEditClients;
			var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			var originalClientData = Mapper.Map<SerializableClient>(TestData.ClientAuthorizationCodeFlowWithPkce);

			var clientDataToSend = Mapper.Map<SerializableClient>(TestData.ClientAuthorizationCodeFlowWithPkce);
			clientDataToSend.AllowAccessTokensViaBrowser = !clientDataToSend.AllowAccessTokensViaBrowser;
			clientDataToSend.AllowedCorsOrigins = clientDataToSend.AllowedCorsOrigins.Select(origin => origin.ToUpper());
			clientDataToSend.AllowedGrantTypes = clientDataToSend.AllowedGrantTypes
				.Skip(clientDataToSend.AllowedGrantTypes.Count() / 2)
				.Select(grantType => {
					// Find the current Grant Type in the list of valid client grant types, and select the next Grant Type in the list (performing an index rotation if necessary)
					int nextGrantIndex = (Array.IndexOf(AppConfigs.AllowedClientRegistrationGrantTypes, grantType) + 1) % AppConfigs.AllowedClientRegistrationGrantTypes.Length;
					return AppConfigs.AllowedClientRegistrationGrantTypes[nextGrantIndex];
				});
			clientDataToSend.AllowedScopes = clientDataToSend.AllowedScopes.Select(scope => scope.ToUpper());
			clientDataToSend.ClientName = clientDataToSend.ClientName.ToUpper();
			clientDataToSend.ClientSecrets = clientDataToSend.ClientSecrets.Select(secret => new SerializableSecret {
				Description = (secret.Description ?? "random-description-9ab718ad560e479993aacb397c74c64c").ToUpper(),
				Expiration = secret.Expiration?.AddDays(-1) ?? DateTime.Now,
				IsValueHashed = !(secret.IsValueHashed ?? true),
				Type = secret.Type,
				Value = secret.Value.ToUpper().Substring(secret.Value.Length / 2),
			});
			clientDataToSend.PostLogoutRedirectUris = clientDataToSend.PostLogoutRedirectUris.Select(uri => uri.ToUpper());
			clientDataToSend.RedirectUris = clientDataToSend.RedirectUris.Select(uri => uri.ToUpper());
			clientDataToSend.RequireClientSecret = !clientDataToSend.RequireClientSecret;
			clientDataToSend.RequireConsent = !clientDataToSend.RequireConsent;
			clientDataToSend.RequirePkce = !clientDataToSend.RequirePkce;

			var clientsEqualityComparer = new SerializableClientEqualityComparer
			{
				CompareClientId = false,
				CompareClientSecrets = false,
			};

			var updateEndpointUri = AppEndpoints.UpdateClientApplication
				.Replace($"{{{AppEndpoints.ClientIdParameterName}}}", originalClientData.ClientId);

			// Act
			var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);

			var updateClientHttpResponse = await httpClient.PutAsync(updateEndpointUri, JsonContent.Create(clientDataToSend));
			var updatedClientData = await updateClientHttpResponse.Content.ReadFromJsonAsync<SerializableClient>();

			// Assert
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.Equal(HttpStatusCode.OK, updateClientHttpResponse.StatusCode);
			Assert.NotEqual(originalClientData, updatedClientData, clientsEqualityComparer);
			Assert.Equal(clientDataToSend, updatedClientData, clientsEqualityComparer);
		}


		[Fact]
		public async Task UpdateClientApplication_ClientIdInUriIsDifferentThanClientIdInJsonPayload_ReturnsBadRequestResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: true);

			var httpClient = WebAppFactory.CreateClient();

			var userToLogin = _userCanViewAndEditClients;
			var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			var clientDataToSend = Mapper.Map<SerializableClient>(TestData.ClientAuthorizationCodeFlowWithPkce);

			var updateEndpointUri = AppEndpoints.UpdateClientApplication
				.Replace($"{{{AppEndpoints.ClientIdParameterName}}}", TestData.ClientClientCredentialsFlow.ClientId);

			// Act
			var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);

			var updateClientHttpResponse = await httpClient.PutAsync(updateEndpointUri, JsonContent.Create(clientDataToSend));
			using var responseStream = await updateClientHttpResponse.Content.ReadAsStreamAsync();
			using var jsonDocument = await JsonDocument.ParseAsync(responseStream);
			var clientIdErrorsArray = jsonDocument.RootElement
				.GetProperty("errors")
				.GetProperty("clientID");

			// Assert
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.Equal(HttpStatusCode.BadRequest, updateClientHttpResponse.StatusCode);
			Assert.Equal(JsonValueKind.Array, clientIdErrorsArray.ValueKind);
			Assert.True(clientIdErrorsArray.GetArrayLength() > 0);
		}


		[Fact]
		public async Task UpdateClientApplication_NonExistantClientId_ReturnsNotFoundResponse()
		{
			// Arrange
			await SetupDatabaseForTests(withSampleClients: true);

			var httpClient = WebAppFactory.CreateClient();

			var userToLogin = _userCanViewAndEditClients;
			var userEmail = userToLogin.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			var clientDataToSend = Mapper.Map<SerializableClient>(TestData.ClientAuthorizationCodeFlowWithPkce);
			clientDataToSend.ClientId = "random-client-id-c1e17fa85550474c9f3ebdbd7179ea86";

			var updateEndpointUri = AppEndpoints.UpdateClientApplication
				.Replace($"{{{AppEndpoints.ClientIdParameterName}}}", clientDataToSend.ClientId);

			// Act
			var loginResponse = await AuthenticationUtilities.PerformRequestToLoginEndpointAsync(httpClient, userEmail, userToLogin.Password);

			var updateClientHttpResponse = await httpClient.PutAsync(updateEndpointUri, JsonContent.Create(clientDataToSend));

			// Assert
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.Equal(HttpStatusCode.NotFound, updateClientHttpResponse.StatusCode);
		}
		#endregion
	}
}
