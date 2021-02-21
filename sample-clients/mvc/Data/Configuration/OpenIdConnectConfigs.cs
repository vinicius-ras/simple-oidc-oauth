using System.Collections.Generic;

namespace SimpleOidcOauth.SampleClients.Mvc.Data.Configuration
{
	/// <summary>Main configurations for the Sample MVC Client.</summary>
	class OpenIdConnectConfigs
	{
		// CONSTANTS
		/// <summary>The name of the section of the configuration which will hold the configurations represented by this class.</summary>
		public const string SectionName = "OpenID";
		/// <summary>The name of the Authentication Scheme which uses OpenID Connect.</summary>
		public const string AuthenticationSchemeName = "oidc";





		// INSTANCE PROPERTIES
		/// <summary>The OpenID Connect / OAuth authority (the IdP server).</summary>
		/// <value>A URL or IP representing the authority server.</value>
		public string Authority { get; set; }
		/// <summary>A list of scopes used by the client application.</summary>
		public IEnumerable<string> RequiredScopes { get; set; }
		/// <summary>The ID for this client application.</summary>
		public string ClientId { get; set; }
		/// <summary>The secret ("password") for this client application.</summary>
		public string ClientSecret { get; set; }
		/// <summary>The response type that should be requested to the target IdP.</summary>
		public string ResponseType { get; set; }
		/// <summary>A flag indicating if PKCE should be used for improved protection.</summary>
		public bool UsePkce { get; set; }
	}
}