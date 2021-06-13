using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Readers;
using SimpleOidcOauth.Data.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace SimpleOidcOauth.Tests.Integration.TestSuites.Misc
{
	/// <summary>Integration tests for the OpenAPI (Swagger) implementation via Swashbuckle.</summary>
	public class SwaggerIntegrationTests : IntegrationTestBase
	{
		// CONSTANTS
		/// <summary>A fake email to be used in the Contact information ("info.contact" section of the OpenAPI document).</summary>
		private const string FAKE_CONTACT_EMAIL = "fake-mail-39db30d67e9e49f8b7610ba153ceb6d9@mail-1939be5e719649acbcd5c32ec3cde479.com";
		/// <summary>A fake name to be used in the Contact information ("info.contact" section of the OpenAPI document).</summary>
		private const string FAKE_CONTACT_NAME = "Fake Contact Name";
		/// <summary>A fake URL to be used in the Contact information ("info.contact" section of the OpenAPI document).</summary>
		private const string FAKE_CONTACT_URL = "https://fake-contact-url-42b9348b3fdf41cebc6e6134163992d8.org/contact";
		/// <summary>A fake description for the test OpenAPI Document.</summary>
		private const string FAKE_API_DESCRIPTION = "A fake API description for testing.";
		/// <summary>A fake name for the test OpenAPI Document.</summary>
		private const string FAKE_API_DOCUMENT_NAME = "Fake Doc Reloaded";
		/// <summary>A URL-friendly version of <see cref="FAKE_API_DOCUMENT_NAME"/>, which will be used to access the test OpenAPI Document.</summary>
		private const string FAKE_API_URL_FRIENDLY = "fake-doc-super-friendly";
		/// <summary>A fake License name for the test OpenAPI Document.</summary>
		private const string FAKE_API_LICENSE_NAME = "FakeNU Lesser General Protected License";
		/// <summary>A fake License URL for the test OpenAPI Document.</summary>
		private const string FAKE_API_LICENSE_URL = "https://fake-license-url-60f53c40e81d431b9c921e52b8d3f290/licensing/makes-me-sleep";
		/// <summary>A fake Terms Of Service URL for the test OpenAPI Document.</summary>
		private const string FAKE_API_TERMS_OF_SERVICE = "https://super-random-terms-of-service-9183ee4f4a5d43e2ba939080f81f5c9a/sketchy-data-collection/just-accept?dont-read=true";
		/// <summary>A fake title for the OpenAPI Document.</summary>
		private const string FAKE_API_TITLE_FULL = "My Fake API";
		/// <summary>A short version of the title provided in <see cref="FAKE_API_TITLE_FULL"/>.</summary>
		private const string FAKE_API_TITLE_SHORT = "FakeAPI";
		/// <summary>A fake version for the OpenAPI Document.</summary>
		private const string FAKE_API_VERSION = "v72.8.12";
		/// <summary>A route template to be used for accessing the fake OpenAPI Document during the tests.</summary>
		/// <remarks>This route has to contain a <c>{documentName}</c> path parameter, as required by the Swashbuckle library.</remarks>
		private const string FAKE_API_ROUTE_TEMPLATE = "/fake-api-docs/{documentName}/api.json";
		/// <summary>The title to be used for the Swagger UI's web page.</summary>
		private const string FAKE_SWAGGER_UI_PAGE_TITLE = "fake-api-docs-viewer";
		/// <summary>The route prefix to be used to access the Swagger UI's web page.</summary>
		private const string FAKE_SWAGGER_UI_ROUTE_PREFIX = "fake-api-docs/ui";





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="webAppFactory">Injected instance for the <see cref="WebApplicationFactory{TEntryPoint}"/> service.</param>
		/// <param name="testOutputHelper">Injected instance for the <see cref="ITestOutputHelper"/> service.</param>
		public SwaggerIntegrationTests(WebApplicationFactory<Startup> webAppFactory, ITestOutputHelper testOutputHelper)
			: base(webAppFactory, testOutputHelper, TestDatabaseInitializationType.None)
		{
			WebAppFactory = WebAppFactory.WithWebHostBuilder(builder => {
				// Use custom configurations
				builder.ConfigureAppConfiguration((builderContext, configurationBuilder) => {
					var customConfigs = new Dictionary<string,string> {
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiContactEmail)}", FAKE_CONTACT_EMAIL},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiContactName)}", FAKE_CONTACT_NAME},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiContactUrl)}", FAKE_CONTACT_URL},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiDescription)}", FAKE_API_DESCRIPTION},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiDocumentName)}", FAKE_API_DOCUMENT_NAME},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiDocumentNameUrlFriendly)}", FAKE_API_URL_FRIENDLY},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiLicenseName)}", FAKE_API_LICENSE_NAME},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiLicenseUrl)}", FAKE_API_LICENSE_URL},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiTermsOfServiceUrl)}", FAKE_API_TERMS_OF_SERVICE},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiTitleFull)}", FAKE_API_TITLE_FULL},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiTitleShort)}", FAKE_API_TITLE_SHORT},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.ApiVersion)}", FAKE_API_VERSION},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.OpenApiDocumentRouteTemplate)}", FAKE_API_ROUTE_TEMPLATE},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.SwaggerUIPageTitle)}", FAKE_SWAGGER_UI_PAGE_TITLE},
						{ $"{AppConfigs.GetAppConfigurationKey(cfg => cfg.Swagger.SwaggerUIRoutePrefix)}", FAKE_SWAGGER_UI_ROUTE_PREFIX},
					};
					configurationBuilder.AddInMemoryCollection(customConfigs);
				});
			});
		}


		/// <summary>Retrieves a URL which can be used to access the OpenAPI Document via an HTTP Client configured to access the Test Host.</summary>
		/// <param name="routeTemplate">
		///     The route template to be used to access the target OpenAPI Document.
		///     If this parameter is omitted, the value <see cref="FAKE_API_ROUTE_TEMPLATE"/> will be used instead.
		/// </param>
		/// <param name="documentName">
		///     The name of the document to be retrieved.
		///     If this parameter is omitted, the value <see cref="FAKE_API_URL_FRIENDLY"/> will be used instead.
		/// </param>
		/// <returns>Returns a string containing the URL to be used to access the given document.</returns>
		private string GetOpenApiDocumentUrl(string routeTemplate = FAKE_API_ROUTE_TEMPLATE, string documentName = FAKE_API_URL_FRIENDLY)
			=> routeTemplate.Replace("{documentName}", documentName);





		// TESTS
		[Fact]
		public async Task RequestOpenApiDocument_RightJsonUrl_ReturnsSuccessAndParseableOpenApiDocument()
		{
			// Arrange
			var client = WebAppFactory.CreateClient();
			var documentUrl = GetOpenApiDocumentUrl();

			// Act
			using var response = await client.GetAsync(documentUrl);
			using var responseBodyStream = await response.Content.ReadAsStreamAsync();

			var openApiReader = new OpenApiStreamReader();
			var openApiDocument = openApiReader.Read(responseBodyStream, out OpenApiDiagnostic openApiDiagnostics);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal(OpenApiSpecVersion.OpenApi3_0, openApiDiagnostics.SpecificationVersion);
			Assert.Equal(0, openApiDiagnostics.Errors?.Count ?? 0);
		}


		[Fact]
		public async Task RequestOpenApiDocument_InvalidDocumentName_ReturnsNotFoundResponse()
		{
			// Arrange
			var client = WebAppFactory.CreateClient();
			var documentUrl = GetOpenApiDocumentUrl(documentName: "non-existent-document-32d4417bad104e1faec90260fd64698f");

			// Act
			using var response = await client.GetAsync(documentUrl);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}


		[Fact]
		public async Task RequestOpenApiDocument_InvalidTemplateRoute_ReturnsNotFoundResponse()
		{
			// Arrange
			var client = WebAppFactory.CreateClient();
			var documentUrl = GetOpenApiDocumentUrl(routeTemplate: "/non-existent-route-template-99341c737b964a97bec06f90ff9fd62c/{documentName}/thedoc.json");

			// Act
			using var response = await client.GetAsync(documentUrl);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}


		[Fact]
		public async Task RequestOpenApiDocument_RightJsonUrl_ReturnsConfiguredDocumentInformation()
		{
			// Arrange
			var client = WebAppFactory.CreateClient();
			var documentUrl = GetOpenApiDocumentUrl();

			// Act
			using var response = await client.GetAsync(documentUrl);
			using var responseBodyStream = await response.Content.ReadAsStreamAsync();

			var openApiReader = new OpenApiStreamReader();
			var openApiDocument = openApiReader.Read(responseBodyStream, out _);

			// Assert
			Assert.Equal(FAKE_CONTACT_EMAIL, openApiDocument.Info.Contact.Email);
			Assert.Equal(FAKE_CONTACT_NAME, openApiDocument.Info.Contact.Name);
			Assert.Equal(FAKE_CONTACT_URL, openApiDocument.Info.Contact.Url.OriginalString);

			Assert.Equal(FAKE_API_DESCRIPTION, openApiDocument.Info.Description);
			Assert.Equal(FAKE_API_LICENSE_NAME, openApiDocument.Info.License.Name);
			Assert.Equal(FAKE_API_LICENSE_URL, openApiDocument.Info.License.Url.OriginalString);
			Assert.Equal(FAKE_API_TERMS_OF_SERVICE, openApiDocument.Info.TermsOfService.OriginalString);
			Assert.Equal(FAKE_API_TITLE_FULL, openApiDocument.Info.Title);
			Assert.Equal(FAKE_API_VERSION, openApiDocument.Info.Version);
		}


		[Fact]
		public async Task RequestOpenApiDocument_RightUrlSwaggerUI_ReturnsConfiguredDocumentInformation()
		{
			// Arrange
			var client = WebAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true,  });

			// Act
			using var response = await client.GetAsync($"{FAKE_SWAGGER_UI_ROUTE_PREFIX.TrimEnd('/')}/index.html");
			using var responseBodyStream = await response.Content.ReadAsStreamAsync();

			var parser = new HtmlParser();
			var parsedHtmlDoc = await parser.ParseDocumentAsync(responseBodyStream);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal(FAKE_SWAGGER_UI_PAGE_TITLE, parsedHtmlDoc.Title);
		}


		[Fact]
		public async Task RequestOpenApiDocument_WrongUrlSwaggerUI_ReturnsConfiguredDocumentInformation()
		{
			// Arrange
			var client = WebAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true,  });

			// Act
			using var response = await client.GetAsync($"{FAKE_SWAGGER_UI_ROUTE_PREFIX.TrimEnd('/')}/wrong-url-e0cd1c15b8a74ce6975182c0ae5788af.html");
			using var responseBodyStream = await response.Content.ReadAsStreamAsync();

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}
	}
}