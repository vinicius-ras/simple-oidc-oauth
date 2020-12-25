using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Controllers;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Models;
using SimpleOidcOauth.Tests.Integration.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
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
        public async Task Login_CorrectCredentials_ReturnsSuccess()
        {
            HttpClient httpDefaultClient = _webAppFactory.CreateClient(),
				httpNonFollowingClient = _webAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });


			// Retrieve the discovery document from the proper IdP endpoint
			var oidcDiscoveryDoc = await httpDefaultClient.GetDiscoveryDocumentAsync();
			bool discoveryDocRequestHasError = oidcDiscoveryDoc.IsError;

			// Perform a request to the Authorization Endpoint with the non-redirecting HTTP Client.
			// This is expected to return an HTTP redirection which would normally redirect the unauthenticated user to a login page.
			var queryParams = new Dictionary<string, string>
			{
				{ OidcConstants.AuthorizeRequest.ClientId, TestData.ClientMvc.ClientId },
				{ OidcConstants.AuthorizeRequest.Scope, string.Join(" ", TestData.ClientMvc.AllowedScopes) },
				{ OidcConstants.AuthorizeRequest.RedirectUri, TestData.ClientMvc.RedirectUris.First() },
				{ OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
				{ OidcConstants.AuthorizeRequest.CodeChallenge, FakePkceCodeChallenge },
				{ OidcConstants.AuthorizeRequest.CodeChallengeMethod, OidcConstants.CodeChallengeMethods.Sha256 },
			};
			var queryBuilder = new QueryBuilder(queryParams);
			var authorizeEndpointUri = new Uri(oidcDiscoveryDoc.AuthorizeEndpoint);
			var uriToCall = $"{authorizeEndpointUri.AbsolutePath}{queryBuilder.ToQueryString()}";

			var authorizeRequestResponseMessage = await httpNonFollowingClient.GetAsync(uriToCall);

			// The "Location" HTTP Header in the response will contain the target URL for the redirection. From this target URL,
			// we must extract the "return URL" query parameter, which must be passed along with the user's credentials during login
			// in order for the IdentityServer4 to be able to correctly perform the user's authentication
			var authorizeRequestResponseQueryParams = HttpUtility.ParseQueryString(authorizeRequestResponseMessage.Headers.Location.Query);
			var returnUrlAfterLogin = authorizeRequestResponseQueryParams[nameof(LoginInputModel.ReturnUrl)];

			var targetUser = TestData.UserAlice;
			var targetUserEmail = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Email).Value;
			var targetUserName = targetUser.Claims.First(claim => claim.Type == JwtClaimTypes.Name).Value;
			var loginInputData = new LoginInputModel
			{
				Email = targetUserEmail,
				Password = targetUser.Password,
				ReturnUrl = returnUrlAfterLogin,
			};
			var loginResult = await httpDefaultClient.PostAsync("/api/Account/login", JsonContent.Create(loginInputData));
			var loginResultObj = await loginResult.Content.ReadFromJsonAsync<LoginOutputModel>();


			// Assert
			Assert.True(loginResult.IsSuccessStatusCode);
			Assert.Equal(targetUserEmail, loginResultObj.Email);
			Assert.Equal(targetUser.Username, loginResultObj.Name);
			Assert.Equal(returnUrlAfterLogin, loginResultObj.ReturnUrl);
        }
	}
}