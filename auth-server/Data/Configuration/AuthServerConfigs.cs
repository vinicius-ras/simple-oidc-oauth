namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>Models the configurations for the Authentication/Authorization Server back end itself.</summary>
	public class AuthServerConfigs
	{
		/// <summary>The name of the cookie used to store user's session.</summary>
		/// <value>
		///     A name for the application cookie, which will be storing the user's session information.
		///     Beware: changing this cookie's name after users have logged in will lead to disconnection
		///     of these users, as their sessions will be invalidated.
		/// </value>
		public string ApplicationCookieName { get; set; }
		/// <summary>The base URL for the auth server's back end.</summary>
		/// <value>The base URL where the auth server's back end should be running.</value>
		public string BaseUrl { get; set; }
		/// <summary>The API Path which must be used to retrieve information about IdP errors.</summary>
		/// <value>
		///     <para>
		///         The API Endpoint's Path used by IdentityServer4 when an error is generated, in order to get information about that error.
		///         This is sometimes refered to as the IdP Error Endpoint throughout this project.
		///     </para>
		///     <para>Paths should start with a slash ('/') character.</para>
		/// </value>
		public string IdentityProviderErrorPath { get; set; }
		/// <summary>The full URL which must be used to retrieve information about IdP errors.</summary>
		/// <value>A concatenation of <see cref="BaseUrl"/> and <see cref="IdentityProviderErrorPath"/>.</value>
		public string IdentityProviderErrorUrl => $"{BaseUrl}{IdentityProviderErrorPath}";
	}
}