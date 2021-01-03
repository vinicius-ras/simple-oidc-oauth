using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimpleOidcOauth.Controllers;
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
			: base(webAppFactory, testOutputHelper)
		{
			// Configure the Test Host to add a new, custom controller, whose only purpose is to fire an Unhandled Exception
			WebAppFactory = WebAppFactory.WithWebHostBuilder(builder => {
				builder.ConfigureServices(services => {
					services.AddControllers()
						.AddApplicationPart(typeof(TestExceptionThrowingController).Assembly);
				});
			});
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
			Assert.Equal("application/problem+json", response.Content.Headers.ContentType.MediaType);
		}
	}
}