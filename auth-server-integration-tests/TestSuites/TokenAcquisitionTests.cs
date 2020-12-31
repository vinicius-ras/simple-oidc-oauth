using Microsoft.AspNetCore.Mvc.Testing;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Utilities;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;


namespace SimpleOidcOauth.Tests.Integration.TestSuites
{
	/// <summary>Integration tests for token acquisition procedures.</summary>
	public class TokenAcquisitionTests : IntegrationTestBase
	{
		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public TokenAcquisitionTests(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
			: base(webAppFactory, testOutputHelper)
		{
		}





		// TESTS
		[Fact]
		public async Task RetrieveToken_AuthorizationCodeFlowWithoutPkce_ReturnsValidToken()
		{
			// Arrange
			var targetUser = TestData.UserAlice;
			var targetClient = TestData.ClientAuthorizationCodeFlowWithoutPkce;
			var targetClientPassword = TestData.PlainTextPasswordClientAuthorizationCodeFlowWithoutPkce;

			// Act
			var acquiredToken = await AuthenticationUtilities.RetrieveUserTokenForAuthorizationCodeFlowAsync(WebAppFactory, targetUser, targetClient, targetClientPassword);

			// Assert
			Assert.False(acquiredToken.IsError);
		}


		[Fact]
		public async Task RetrieveToken_AuthorizationCodeFlowWithPkce_ReturnsValidToken()
		{
			// Arrange
			var targetUser = TestData.UserAlice;
			var targetClient = TestData.ClientAuthorizationCodeFlowWithPkce;
			var targetClientPassword = TestData.PlainTextPasswordClientAuthorizationCodeFlowWithPkce;

			// Act
			var acquiredToken = await AuthenticationUtilities.RetrieveUserTokenForAuthorizationCodeFlowAsync(WebAppFactory, targetUser, targetClient, targetClientPassword);

			// Assert
			Assert.False(acquiredToken.IsError);
		}


		[Fact]
		public async Task RetrieveToken_ImplicitFlow_ReturnsValidToken()
		{
			// Arrange
			var targetUser = TestData.UserAlice;
			var targetClient = TestData.ClientImplicitFlowAccessTokensOnly;

			// Act
			var acquiredToken = await AuthenticationUtilities.RetrieveUserTokenForImplicitFlowAsync(WebAppFactory, targetUser, targetClient);

			// Assert
			Assert.NotEmpty(acquiredToken);
		}
	}
}