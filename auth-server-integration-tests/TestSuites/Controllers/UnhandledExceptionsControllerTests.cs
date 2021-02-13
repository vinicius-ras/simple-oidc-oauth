using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimpleOidcOauth.Controllers;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Tests.Integration.Controllers;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.TestSuites.Controllers
{
	/// <summary>Integration tests for the <see cref="UnhandledExceptionsController" />.</summary>
	public class UnhandledExceptionsControllerTests : IntegrationTestBase
	{
		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public UnhandledExceptionsControllerTests(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
			: base(webAppFactory, testOutputHelper, TestDatabaseInitializationType.None)
		{
		}





		// TESTS
		[Fact]
        public async Task OnUnhandledException_WhenControllerThrowsUnhandledException_ReturnsInternalServerErrorWithProblemDetails()
        {
			// Arrange
			var httpClient = WebAppFactory.CreateClient();

			// Act
			var response = await httpClient.GetAsync(TestExceptionThrowingController.EndpointUri);

			// Assert
			Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
			Assert.Equal(AppConfigs.MediaTypeApplicationProblemJson, response.Content.Headers.ContentType.MediaType);
		}
	}
}