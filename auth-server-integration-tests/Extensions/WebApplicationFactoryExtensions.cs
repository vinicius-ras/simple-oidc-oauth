using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleOidcOauth.Tests.Integration.TestSuites;

namespace SimpleOidcOauth.Tests.Integration.Extensions
{
	/// <summary>Extension methods for the <see cref="WebApplicationFactory{TEntryPoint}"/></summary>
	public static class WebApplicationFactoryExtensions
	{
		/// <summary>Creates an <see cref="HttpClient"/> configured to be used during the integration tests.</summary>
		/// <param name="webApplicationFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> that is able to instantiate HTTP Clients
		///     which access the Test Host.
		/// </param>
		/// <param name="allowAutoRedirect">A flag indicating if the client should automatically follow redirects.</param>
		/// <returns>Returns a new <see cref="HttpClient"/> instance, preconfigured for the Integration Tests.</returns>
		public static HttpClient CreateIntegrationTestClient<TStartup>(this WebApplicationFactory<TStartup> webApplicationFactory, bool allowAutoRedirect = true)
			where TStartup : class
			=> webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = allowAutoRedirect, BaseAddress = IntegrationTestBase.TestServerBaseUri });
	}
}