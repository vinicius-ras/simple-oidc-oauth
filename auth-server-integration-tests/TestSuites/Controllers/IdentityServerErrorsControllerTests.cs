using IdentityModel;
using IdentityServer4;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleOidcOauth.Controllers;
using SimpleOidcOauth.Tests.Integration.Data;
using SimpleOidcOauth.Tests.Integration.Exceptions;
using SimpleOidcOauth.Tests.Integration.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.TestSuites.Controllers
{
	/// <summary>Integration tests for the <see cref="IdentityServerErrorsController" />.</summary>
	public class IdentityServerErrorsControllerTests : IntegrationTestBase
	{
		// CONSTANTS
		/// <summary>An invalid Error ID to be sent to the IdentityServer Error Endpoint during some tests.</summary>
		private const string InvalidErrorId = "__invalid-error-id__";





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public IdentityServerErrorsControllerTests(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
			: base(webAppFactory, testOutputHelper)
		{
		}





		// TESTS
		[Fact]
        public async Task ErrorEndpoint_InvalidErrorId_ReturnsInternalServerError()
        {
			// Arrange
			var httpClient = WebAppFactory.CreateClient();

			// Act
			var response = await httpClient.GetAsync($"/api/idp-error?errorId={InvalidErrorId}");
			string responseContentType = response.Content?.Headers?.ContentType?.MediaType;

			// Assert
			Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
			Assert.Equal("application/problem+json", responseContentType);
        }


		[Fact]
        public async Task ErrorEndpoint_ValidErrorId_ReturnsOk()
        {
			// Arrange
			var httpClient = WebAppFactory.CreateClient();

			// ACT
			// Call the IdP Authorize Endpoint without parameters to force the generation of an error
			var response = await httpClient.GetAsync($"/connect/authorize");
			string responseContentType = response.Content?.Headers?.ContentType?.MediaType;

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal(MediaTypeNames.Application.Json, responseContentType);
        }
	}
}