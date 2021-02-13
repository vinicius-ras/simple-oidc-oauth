using IdentityServer4.Models;
using System.Collections.Generic;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="Client"/> object.</summary>
	public class SerializableClient
	{
		// INSTANCE PROPERTIES
		/// <summary>
		///     Controls whether access tokens are transmitted via the browser for this client
        ///     (defaults to false). This can prevent accidental leakage of access tokens when
        ///     multiple response types are allowed.
		/// </summary>
		/// <value><c>true</c> if access tokens can be transmitted via the browser; otherwise, <c>false</c>.</value>
		public bool AllowAccessTokensViaBrowser { get; set; }
		/// <summary>Gets or sets the allowed CORS origins for JavaScript clients.</summary>
		/// <value>The allowed CORS origins.</value>
		public IEnumerable<string> AllowedCorsOrigins { get; set; }
		/// <summary>
		///     Specifies the allowed grant types (legal combinations of AuthorizationCode, Implicit,
		///     Hybrid, ResourceOwner, ClientCredentials).
		/// </summary>
		public IEnumerable<string> AllowedGrantTypes { get; set; }
		/// <summary>
		///     Specifies the api scopes that the client is allowed to request. If empty, the
		///     client can't access any scope.
		/// </summary>
		public IEnumerable<string> AllowedScopes { get; set; }
		/// <summary>Unique ID of the client.</summary>
		public string ClientId { get; set; }
		/// <summary>Client display name (used for logging and consent screen).</summary>
		public string ClientName { get; set; }
		/// <summary>Client secrets - only relevant for flows that require a secret.</summary>
		public IEnumerable<SerializableSecret> ClientSecrets { get; set; }
		/// <summary>Specifies allowed URIs to redirect to after logout.</summary>
		public IEnumerable<string> PostLogoutRedirectUris { get; set; }
		/// <summary>Specifies allowed URIs to return tokens or authorization codes to.</summary>
		public IEnumerable<string> RedirectUris { get; set; }
		/// <summary>
		///     If set to false, no client secret is needed to request tokens at the token endpoint
		///     (defaults to true)
		/// </summary>
		public bool RequireClientSecret { get; set; }
		/// <summary>Specifies whether a consent screen is required (defaults to <c>false</c>).</summary>
		public bool RequireConsent { get; set; }
		/// <summary>
		///     Specifies whether a proof key is required for authorization code based token
		///     requests (defaults to true).
		/// </summary>
		public bool RequirePkce { get; set; }
	}
}