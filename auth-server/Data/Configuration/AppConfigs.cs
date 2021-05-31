using IdentityModel;
using IdentityServer4;

namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>
	///     A class representing the configurations related to the auth server's Single Page Application (SPA).
	///     The SPA is the web application which is displays the front end interfaces to allow user's
	///     to interact with the auth server.
	/// </summary>
	public class AppConfigs
	{
		// CONSTANTS
		/// <summary>
		///     The key in the configuration file which holds all of the configuration data to be loaded
		///     into an <see cref="AppConfigs"/>  instance.
		/// </summary>
		public const string ConfigKey = "App";
		/// <summary>
		///     The key to be queried into the "ConnectionStrings" configuration section in order to obtain
		///     the connection string used to connect to the users' store.
		/// </summary>
		public const string ConnectionStringIdentityServerUsers = "IdentityServerUsers";
		/// <summary>
		///     The key to be queried into the "ConnectionStrings" configuration section in order to obtain
		///     the connection string used to connect to the IdentityServer's configuration store.
		/// </summary>
		public const string ConnectionStringIdentityServerConfiguration = "IdentityServerConfigs";
		/// <summary>
		///     The key to be queried into the "ConnectionStrings" configuration section in order to obtain
		///     the connection string used to connect to the IdentityServer's operational store.
		/// </summary>
		public const string ConnectionStringIdentityServerOperational = "IdentityServerOperational";
		/// <summary>
		///     Media type and HTTP content header value for the content type that carries a JSON formatted problem
		///     report, according to RFC 7807 ("Problem Details for HTTP APIs").
		/// </summary>
		public const string MediaTypeApplicationProblemJson = "application/problem+json";
		/// <summary>A set of scheme names which are accepted by the IdP in Post-Login/Logout Redirect URLs and CORs Origins for the registered Client Applications.</summary>
		public static readonly string[] AcceptableClientRedirectionUrlSchemes = { "http", "https" };
		/// <summary>The set of Grant Types supported for registering the clients with this IdP.</summary>
		public static readonly string[] AllowedClientRegistrationGrantTypes =
		{
			OidcConstants.GrantTypes.AuthorizationCode,
			OidcConstants.GrantTypes.ClientCredentials,
			OidcConstants.GrantTypes.Implicit,
			OidcConstants.GrantTypes.Password,
			OidcConstants.GrantTypes.RefreshToken
		};
		/// <summary>The set of Client Secret types that are currently supported by the IdP.</summary>
		public static readonly string[] SupportedClientSecretTypes = {
			IdentityServerConstants.SecretTypes.SharedSecret,
		};





		// PROPERTIES
		/// <summary>Configurations related to the IdP Back End server.</summary>
		/// <value>An object holding all of the auth server's back end related configurations.</value>
		public AuthServerConfigs AuthServer { get; set; }
		/// <summary>Configurations for the Single Page Application which displays the auth server's User Interface.</summary>
		/// <value>An object holding all of the auth server's SPA configurations.</value>
		public SpaConfigs Spa { get; set; }
		/// <summary>Configurations for the <see cref="Services.IEmbeddedResourcesService" />.</summary>
		/// <value>An object holding all of the configurations for the <see cref="Services.IEmbeddedResourcesService" />.</value>
		public EmbeddedResourcesConfigs EmbeddedResources { get; set; }
		/// <summary>Configurations for the <see cref="Services.IEmailService" />.</summary>
		/// <value>An object holding all of the configurations for the <see cref="Services.IEmailService" />.</value>
		public EmailConfigs Email { get; set; }
		/// <summary>
		///     <para>Data used to initialize the database during application's startup.</para>
		///     <para>Using this configuration is not recommended for production systems, as it was designed for testing purposes only.</para>
		/// </summary>
		/// <value>
		///     <para>A set of properties containing predefined application data (users, clients, API scopes, etc) used to initialize the database when the IdP server starts.</para>
		///     <para>This data is optional, and can be <c>null</c> if no predefined data is needed. Nested properties can also be set to <c>null</c> if not needed.</para>
		/// </value>
		public DatabaseInitializationConfigs DatabaseInitialization { get; set; }
		/// <summary>Configurations for Swagger - an OpenAPI implementation provided by the Swashbuckle library.</summary>
		public SwaggerConfigs Swagger { get; set; }
	}
}