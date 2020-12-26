using IdentityModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Controllers;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
			var loggedInUser = await AuthenticationUtilities.PerformUserLoginAsync(_webAppFactory, targetUser, queryParams);

			// Assert
			Assert.Null(loggedInUser);
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
	}
}