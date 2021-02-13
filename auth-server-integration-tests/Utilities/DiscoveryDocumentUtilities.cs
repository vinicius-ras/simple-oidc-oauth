using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleOidcOauth.Tests.Integration.Extensions;

namespace SimpleOidcOauth.Tests.Integration.Utilities
{
	/// <summary>
	///     Utility class used to cache the OpenID Connect Discovery Document for the tests.
	///     The Discovery Document can be safely cached and accessed by all test threads because it isn't expected
	///     to change at any time during the execution of any of the Test Server instances.
	/// </summary>
	static class DiscoveryDocumentUtilities
	{
		// STATIC FIELDS
		/// <summary>
		///     A lock used to initialize and fetch a cached <see cref="DiscoveryDocumentResponse"/>.
		///     See <see cref="_cachedDiscoveryDocumentResponse"/> for more information.
		/// </summary>
		private static object _lockDiscoveryDocumentResponse = new object();
		/// <summary>
		///    A cached instance of a fetched <see cref="DiscoveryDocumentResponse"/>.
		///    The OpenID Connect Discovery Document should be immutable in our Integration Tests, so a cache is kept
		///    and can be accessed by all running tests.
		/// </summary>
		private static DiscoveryDocumentResponse _cachedDiscoveryDocumentResponse = null;





		// STATIC METHODS
		/// <summary>Retrieves (or builds) the cached Discovery Document for the tests.</summary>
		/// <param name="webAppFactory">
		///     Reference to the <see cref="WebApplicationFactory{TEntryPoint}"/> used in the test.
		///     This will be used to generate clients as necessary to perform the correct calls to the target endpoints.
		/// </param>
		/// <returns>Returns a representation of the Discovery Document's response.</returns>
		public static DiscoveryDocumentResponse GetDiscoveryDocumentResponse<TStartup>(WebApplicationFactory<TStartup> webAppFactory)
			where TStartup : class
		{
			lock (_lockDiscoveryDocumentResponse)
			{
				if (_cachedDiscoveryDocumentResponse == null)
				{
					using var httpClient = webAppFactory.CreateIntegrationTestClient(true);
					_cachedDiscoveryDocumentResponse = httpClient.GetDiscoveryDocumentAsync().Result;
				}
			}
			return _cachedDiscoveryDocumentResponse;
		}
	}
}