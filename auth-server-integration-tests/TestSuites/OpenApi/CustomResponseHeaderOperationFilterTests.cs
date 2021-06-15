using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.OpenApi.Swagger.Attributes;
using SimpleOidcOauth.OpenApi.Swagger.Filters;
using SimpleOidcOauth.Tests.Integration.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.TestSuites.OpenApi
{
	/// <summary>Integration tests for the <see cref="CustomResponseHeaderOperationFilter"/> and <see cref="CustomResponseHeaderAttribute"/> classes.</summary>
	/// <remarks>
	///     The tests in this suite make use of the special controller <see cref="TestCustomResponseHeaderController"/>. This controller provides a well-known
	///     set of documented APIs that can be safely controlled and used for the tests.
	/// </remarks>
	public class CustomResponseHeaderOperationFilterTests : IntegrationTestBase
	{
		// CONSTANTS
		/// <summary>Controlled name of the OpenAPI document used in the tests.</summary>
		private const string FAKE_API_URL_FRIENDLY = "fake-doc";
		/// <summary>Controlled route template used to access the OpenAPI document used in the tests.</summary>
		private const string FAKE_API_ROUTE_TEMPLATE = "/fake-api-docs/{documentName}/fake-openapi.json";





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public CustomResponseHeaderOperationFilterTests(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
			: base(webAppFactory, testOutputHelper, TestDatabaseInitializationType.None)
		{
			WebAppFactory = WebAppFactory.WithWebHostBuilder(builder => {
				// Use custom configurations
				builder.ConfigureAppConfiguration((builderContext, configurationBuilder) => {
					var customConfigs = new Dictionary<string,string> {
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiDocumentNameUrlFriendly)}", FAKE_API_URL_FRIENDLY},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.OpenApiDocumentRouteTemplate)}", FAKE_API_ROUTE_TEMPLATE},
					};
					configurationBuilder.AddInMemoryCollection(customConfigs);
				});

				builder.ConfigureServices(services => {
					// Add the XML Documentation for the Integration Tests' Controllers to the OpenAPI Documentation
					services.AddSwaggerGen(opts => {
						string curAssemblyName = Assembly.GetExecutingAssembly().GetName().Name,
							appBasePath = AppContext.BaseDirectory,
							xmlDocumentationPath = $"{appBasePath}{curAssemblyName}.xml";
						opts.IncludeXmlComments(xmlDocumentationPath);
					});
				});
			});
		}


		/// <summary>Retrieves a URL which can be used to access the OpenAPI Document via an HTTP Client configured to access the Test Host.</summary>
		/// <param name="client">An HTTP Client used to access the Test Host.</param>
		/// <returns>Returns a <see cref="Task"/>-wrapped string containing the URL to be used to access the given document.</returns>
		private async Task<OpenApiDocument> GetOpenApiDocument(HttpClient client)
		{
			string documentUrl = FAKE_API_ROUTE_TEMPLATE.Replace("{documentName}", FAKE_API_URL_FRIENDLY);
			using var response = await client.GetAsync(documentUrl);
			using var responseBodyStream = await response.Content.ReadAsStreamAsync();

			var openApiReader = new OpenApiStreamReader();
			var openApiDocument = openApiReader.Read(responseBodyStream, out var parseDiagnostics);
			if (parseDiagnostics?.Errors?.Count > 0)
				throw new InvalidOperationException($"Failed to obtain and/or read OpenAPI Document from the Test Host.");
			return openApiDocument;
		}





		// TESTS
		[Fact]
		public async Task CustomResponseHeaderOperationFilter_EndpointWithSingleDocumentedHeader_ReturnsSingleAndCorrectHeaderDocumentation()
		{
			// Arrange
			var client = WebAppFactory.CreateClient();


			// Act
			var openApiDocument = await GetOpenApiDocument(client);

			string strHttpStatusCode = TestCustomResponseHeaderController.SingleDocumentedLocationHeaderStatusCode.ToString("D"),
				targetEndpointPath = TestCustomResponseHeaderController.SingleDocumentedLocationHeaderEndpoint;
			var response = openApiDocument
				?.Paths?[targetEndpointPath]
				?.Operations?[OperationType.Get]
				?.Responses?[strHttpStatusCode];

			string locationHeaderKey = response?.Headers
				?.Keys
				?.FirstOrDefault(headerKey => headerKey.Equals(nameof(HeaderNames.Location), StringComparison.OrdinalIgnoreCase));
			var locationHeader = response?.Headers?[locationHeaderKey];


			// Assert
			Assert.NotNull(openApiDocument);
			Assert.NotNull(response);
			Assert.Equal(1, response.Headers?.Count ?? 0);
			Assert.Equal(TestCustomResponseHeaderController.SingleDocumentedLocationHeaderDescription, locationHeader?.Description);
			Assert.Equal(typeof(string).Name, locationHeader?.Schema?.Type, ignoreCase: true);
		}


		[Fact]
		public async Task CustomResponseHeaderOperationFilter_EndpointWithoutDocumentedHeaders_ReturnsNullOrEmptyHeadersDocumentation()
		{
			// Arrange
			var client = WebAppFactory.CreateClient();


			// Act
			var openApiDocument = await GetOpenApiDocument(client);

			string strHttpStatusCodeCreated = HttpStatusCode.OK.ToString("D"),
				targetEndpointPath = TestCustomResponseHeaderController.NoDocumentedHeadersEndpoint;
			var endpointPath = openApiDocument
				?.Paths?[targetEndpointPath];

			var allHeaderDocumentationsForPath = endpointPath?.Operations?.Values
				?.SelectMany(operation => operation.Responses.Values)
				?.SelectMany(response => response.Headers.Values)
				?.ToList();


			// Assert
			Assert.NotNull(openApiDocument);
			Assert.NotNull(endpointPath);
			Assert.True(allHeaderDocumentationsForPath == null || allHeaderDocumentationsForPath.Count == 0);
		}


		[Fact]
		public async Task CustomResponseHeaderOperationFilter_EndpointWithMultipleDocumentedHeaders_ReturnsMultipleHeaderDocumentations()
		{
			// Arrange
			var client = WebAppFactory.CreateClient();


			// Act
			var openApiDocument = await GetOpenApiDocument(client);

			string strHttpOKStatusCode = HttpStatusCode.OK.ToString("D"),
				strHttpUnauthorizedStatusCode = HttpStatusCode.Unauthorized.ToString("D"),
				strHttpForbiddenStatusCode = HttpStatusCode.Forbidden.ToString("D"),
				targetEndpointPath = TestCustomResponseHeaderController.MultipleDocumentedHeadersEndpoint;
			var targetEndpointOperation = openApiDocument
				?.Paths?[targetEndpointPath]
				?.Operations?[OperationType.Delete];

			var httpOkHeaders = targetEndpointOperation?.Responses[strHttpOKStatusCode]?.Headers;
			var httpUnauthorizedHeaders = targetEndpointOperation?.Responses[strHttpUnauthorizedStatusCode]?.Headers;
			var httpForbiddenHeaders = targetEndpointOperation?.Responses[strHttpForbiddenStatusCode]?.Headers;


			// Assert
			Assert.NotNull(httpOkHeaders);
			Assert.NotNull(httpUnauthorizedHeaders);
			Assert.NotNull(httpForbiddenHeaders);

			Assert.Equal(3, httpOkHeaders?.Count ?? 0);
			Assert.Equal(0, httpUnauthorizedHeaders?.Count ?? 0);
			Assert.Equal(1, httpForbiddenHeaders?.Count ?? 0);


			Assert.Contains(httpOkHeaders, header =>
				header.Key == TestCustomResponseHeaderController.CustomHeaderName1
				&& header.Value.Description == TestCustomResponseHeaderController.CustomHeaderDescription1);
			Assert.Contains(httpOkHeaders, header =>
				header.Key == TestCustomResponseHeaderController.CustomHeaderName2
				&& header.Value.Description == TestCustomResponseHeaderController.CustomHeaderDescription2);
			Assert.Contains(httpOkHeaders, header =>
				header.Key == TestCustomResponseHeaderController.CustomHeaderName3
				&& header.Value.Description == TestCustomResponseHeaderController.CustomHeaderDescription3);


			Assert.Contains(httpForbiddenHeaders, header =>
				header.Key == TestCustomResponseHeaderController.CustomHeaderName4
				&& header.Value.Description == TestCustomResponseHeaderController.CustomHeaderDescription4);

		}
	}
}
