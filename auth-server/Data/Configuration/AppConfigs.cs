namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>
	///     A class representing the configurations related to the auth server's Single Page Application (SPA).
	///     The SPA is the web application which is displays the front end interfaces to allow user's
	///     to interact with the auth server.
	/// </summary>
	class AppConfigs
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





		// PROPERTIES
		/// <summary>The URL where the SPA is running.</summary>
		/// <value>
		///     An URL which refers to the location where the SPA used by the auth server is running.
		/// </value>
		public string SpaUrl { get; set; }
		/// <summary>The name of the cookie used to store user's session.</summary>
		/// <value>
		///     A name for the application cookie, which will be storing the user's session information.
		///     Beware: changing this cookie's name after users have logged in will lead to disconnection
		///     of these users, as their sessions will be invalidated.
		/// </value>
		public string ApplicationCookieName { get; set; }
	}
}