using IdentityModel;
using IdentityServer4;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Controllers;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Exceptions;
using SimpleOidcOauth.Tests.Integration.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.Controllers
{
	/// <summary>Integration tests for the <see cref="AccountController" />.</summary>
	public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>
	{
		// CONSTANTS
		/// <summary>A fake PKCE Code Challenge to be used during tests, whenever necessary.</summary>
		private const string FakePkceCodeChallenge = "55ea0909dcb34a8182fd2c4a619aae0cc8a5074f08b545cd892a1f84e6c482e3e34cf9bbe0e7369f52b5219abda46c1155ea0909dcb34a8182fd2c4a619aae0c";
		/// <summary>
		///     <para>A fake "nonce" value to be used in tests.</para>
		///     <para>A "nonce" value is required to be sent to the Authorization Endpoint for both the Implicit Flow and the Hybrid Flow.</para>
		/// </summary>
		private const string FakeNonceValue = "9c3cfd4e9f1f497aaf7c773e59a771865aba0298d5b643a18732bbdd279b74bcb67642da6bce465ba9326c3b260ac956c8399b4c488641188459b348e13228d453f09f5a3b7a441082286673168e7d8e";
		/// <summary>Data for a non-registered user to be used during the pertinent tests.</summary>
		public static readonly TestUser NotRegisteredUser = new TestUser{
			SubjectId = "6bd0fdfac61c498d86ab33019aae0b1c",
			Username = "NotRegisteredUser",
			Password = "484bb180411E41EC95B48E957A5febd6$",
			Claims =
			{
				new Claim(JwtClaimTypes.Name, "Unregisteredon Turingson"),
				new Claim(JwtClaimTypes.GivenName, "Unregisteredon"),
				new Claim(JwtClaimTypes.FamilyName, "Turingson"),
				new Claim(JwtClaimTypes.Email, "UnregisteredonTuringson@email.com"),
				new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean),
				new Claim(JwtClaimTypes.WebSite, "https://turingson.unregisteredon.b15879e860c84e03a0d9ff5446faf91c.com"),
				new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'Keeper Parkway', 'locality': 'Eversmile', 'postal_code': 66697, 'country': 'United Kingdom' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
			}
		};




		// FIELDS
		/// <summary>Reference to an <see cref="WebApplicationFactory{TEntryPoint}"/>, injected by the test engine.</summary>
		private readonly WebApplicationFactory<Startup> _webAppFactory;
		/// <summary>Reference to an <see cref="ITestOutputHelper"/>, injected by the test engine.</summary>
		private readonly ITestOutputHelper _testOutputHelper;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public AccountControllerTests(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper;

			// Reconfigure the test host to prepare it for the tests
			_webAppFactory = webAppFactory.WithWebHostBuilder(builder => {
				// Use a custom/separate SQLite file to store the database for this class, and update the base-url to be considered for the Auth Server
				builder.ConfigureAppConfiguration((builderContext, configurationBuilder) => {
					var customConfigs = new Dictionary<string,string> {
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerConfiguration}", $"Data Source={nameof(AccountControllerTests)}-IdentityServerConfigs.sqlite;" },
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerOperational}", $"Data Source={nameof(AccountControllerTests)}-IdentityServerOperational.sqlite;" },
						{ $"ConnectionStrings:{AppConfigs.ConnectionStringIdentityServerUsers}", $"Data Source={nameof(AccountControllerTests)}-IdentityServerUsers.sqlite;" },

						{ $"App:AuthServerBaseUrl", "http://localhost" },
						{ $"App:Spa:ErrorUrl", "http://localhost/api/account/error" },
					};
					configurationBuilder.AddInMemoryCollection(customConfigs);
				});


				// Initialize the test database
				builder.ConfigureTestServices(services => {
					using (var serviceProvider = services.BuildServiceProvider())
					{
						TestData.ClearDatabaseAsync(serviceProvider).Wait();
						TestData.InitializeDatabaseAsync(serviceProvider).Wait();
					}
				});

				// Configure ILogger objects to use the ITestOutputHelper, which collects logs for unit/integration tests
				builder.ConfigureLogging(loggingBuilder => {
					loggingBuilder.ClearProviders();
					loggingBuilder.AddXUnit(_testOutputHelper);
				});
			});
		}





		// TESTS
		[Fact]
        public async Task Login_ConfirmedUserAuthorizationCodeFlowWithPkce_ReturnsSuccess()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
				{ OidcConstants.AuthorizeRequest.CodeChallenge, FakePkceCodeChallenge },
				{ OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
			};

			var targetUser = TestData.UserAlice;
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.NotNull(loggedInUser);
			Assert.Equal(targetUserEmail, loggedInUser.Email);
			Assert.Equal(targetUser.Username, loggedInUser.Name);
        }


		[Fact]
        public async Task Login_UnconfirmedUserAuthorizationCodeFlowWithPkce_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
				{ OidcConstants.AuthorizeRequest.CodeChallenge, FakePkceCodeChallenge },
				{ OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
			};

			var targetUser = TestData.UserBob;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
        }


		[Fact]
        public async Task Login_UnregisteredUserAuthorizationCodeFlowWithPkce_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
				{ OidcConstants.AuthorizeRequest.CodeChallenge, FakePkceCodeChallenge },
				{ OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
			};

			var targetUser = NotRegisteredUser;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
        }


		[Fact]
        public async Task Login_RequestMissingPkceAuthorizationCodeFlowWithPkce_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
			};

			var targetUser = TestData.UserAlice;

			// Act
			var loginTask = AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			await Assert.ThrowsAsync<AuthorizeEndpointResponseException>(() => loginTask);
        }


		[Fact]
        public async Task Login_RequestInvalidScopeAuthorizationCodeFlowWithPkce_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, TestData.ScopeApiResourceUserManagement },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
				{ OidcConstants.AuthorizeRequest.CodeChallenge, FakePkceCodeChallenge },
				{ OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
			};

			var targetUser = TestData.UserAlice;

			// Act
			var loginTask = AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			await Assert.ThrowsAsync<AuthorizeEndpointResponseException>(() => loginTask);
        }


		[Fact]
        public async Task Login_ConfirmedUserAuthorizationCodeFlowWithoutPkce_ReturnsSuccess()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithoutPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
				{ OidcConstants.AuthorizeRequest.CodeChallenge, FakePkceCodeChallenge },
				{ OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
			};

			var targetUser = TestData.UserAlice;
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.NotNull(loggedInUser);
			Assert.Equal(targetUserEmail, loggedInUser.Email);
			Assert.Equal(targetUser.Username, loggedInUser.Name);
        }


		[Fact]
        public async Task Login_UnregisteredUserAuthorizationCodeFlowWithoutPkce_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithoutPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
				{ OidcConstants.AuthorizeRequest.CodeChallenge, FakePkceCodeChallenge },
				{ OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
			};

			var targetUser = NotRegisteredUser;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
        }


		[Fact]
        public async Task Login_UnconfirmedUserAuthorizationCodeFlowWithoutPkce_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithoutPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
				{ OidcConstants.AuthorizeRequest.CodeChallenge, FakePkceCodeChallenge },
				{ OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
			};

			var targetUser = TestData.UserBob;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
        }


		[Fact]
        public async Task Login_RequestMissingPkceAuthorizationCodeFlowWithoutPkce_ReturnsSuccess()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithoutPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
			};

			var targetUser = TestData.UserAlice;
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.NotNull(loggedInUser);
			Assert.Equal(targetUserEmail, loggedInUser.Email);
			Assert.Equal(targetUser.Username, loggedInUser.Name);
        }


		[Fact]
        public async Task Login_RequestInvalidScopeAuthorizationCodeFlowWithoutPkce_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientAuthorizationCodeFlowWithoutPkce;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, TestData.ScopeApiResourceUserManagement },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
			};

			var targetUser = TestData.UserAlice;
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loginTask = AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			await Assert.ThrowsAsync<AuthorizeEndpointResponseException>(() => loginTask);
        }


		[Fact]
        public async Task Login_ConfirmedUserImplicitFlowAccessTokensOnly_ReturnsSuccess()
        {
			// Arrange
			var targetClient = TestData.ClientImplicitFlowAccessTokensOnly;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token },
			};

			var targetUser = TestData.UserAlice;
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.NotNull(loggedInUser);
			Assert.Equal(targetUserEmail, loggedInUser.Email);
			Assert.Equal(targetUser.Username, loggedInUser.Name);
        }


		[Fact]
        public async Task Login_UnconfirmedUserImplicitFlowAccessTokensOnly_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientImplicitFlowAccessTokensOnly;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token },
			};

			var targetUser = TestData.UserBob;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
        }


		[Fact]
        public async Task Login_UnregisteredUserImplicitFlowAccessTokensOnly_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientImplicitFlowAccessTokensOnly;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token },
			};

			var targetUser = NotRegisteredUser;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
        }


		[Fact]
        public async Task Login_RequestInvalidScopeImplicitFlowAccessTokensOnly_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientImplicitFlowAccessTokensOnly;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, IdentityServerConstants.StandardScopes.OpenId },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token },
			};

			var targetUser = TestData.UserAlice;

			// Act
			var loginTask = AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			await Assert.ThrowsAsync<AuthorizeEndpointResponseException>(() => loginTask);
        }


		[Fact]
        public async Task Login_ConfirmedUserImplicitFlowAccessAndIdTokens_ReturnsSuccess()
        {
			// Arrange
			var targetClient = TestData.ClientImplicitFlowAccessAndIdTokens;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken },
				{ OidcConstants.AuthorizeRequest.Nonce, FakeNonceValue },
			};

			var targetUser = TestData.UserAlice;
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.NotNull(loggedInUser);
			Assert.Equal(targetUserEmail, loggedInUser.Email);
			Assert.Equal(targetUser.Username, loggedInUser.Name);
        }


		[Fact]
        public async Task Login_UnconfirmedUserImplicitFlowAccessAndIdTokens_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientImplicitFlowAccessAndIdTokens;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken },
				{ OidcConstants.AuthorizeRequest.Nonce, FakeNonceValue },
			};

			var targetUser = TestData.UserBob;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
        }


		[Fact]
        public async Task Login_UnregisteredUserImplicitFlowAccessAndIdTokens_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientImplicitFlowAccessAndIdTokens;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", targetClient.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken },
				{ OidcConstants.AuthorizeRequest.Nonce, FakeNonceValue },
			};

			var targetUser = NotRegisteredUser;

			// Act
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
        }


		[Fact]
        public async Task Login_RequestInvalidScopeImplicitFlowAccessAndIdTokens_ReturnsFailure()
        {
			// Arrange
			var targetClient = TestData.ClientImplicitFlowAccessAndIdTokens;
			var returnUrlAfterLogin = targetClient.RedirectUris.First();
            var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, targetClient.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, TestData.ScopeApiResourceUserManagement },
				{ OidcConstants.AuthorizeRequest.RedirectUri, returnUrlAfterLogin },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken },
				{ OidcConstants.AuthorizeRequest.Nonce, FakeNonceValue },
			};

			var targetUser = TestData.UserAlice;

			// Act
			var loginTask = AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			await Assert.ThrowsAsync<AuthorizeEndpointResponseException>(() => loginTask);
        }
	}
}