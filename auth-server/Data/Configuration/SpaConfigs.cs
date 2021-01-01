namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>
	///     Models the configurations for the Single Page Application which displays
	///     the auth server's user interface.
	/// </summary>
	public class SpaConfigs
	{
		/// <summary>The base URL for the auth server's SPA.</summary>
		/// <value>The base URL where the auth server's SPA should be running.</value>
		public string BaseUrl { get; set; }
		/// <summary>The path to the SPA's login page.</summary>
		/// <value>
		///     <para>The path where auth server's SPA displays its login screen.</para>
		///     <para>Paths should start with a slash ('/') character.</para>
		/// </value>
		public string LoginPath { get; set; }
		/// <summary>The full URL to the SPA's Login page.</summary>
		/// <value>A concatenation of <see cref="BaseUrl"/> and <see cref="LoginPath"/>.</value>
		public string LoginUrl => $"{BaseUrl}{LoginPath}";

		/// <summary>The path to the SPA's logout page.</summary>
		/// <value>
		///     <para>The path where auth server's SPA displays its logout screen.</para>
		///     <para>Paths should start with a slash ('/') character.</para>
		/// </value>
		public string LogoutPath { get; set; }
		/// <summary>The full URL to the SPA's Logout page.</summary>
		/// <value>A concatenation of <see cref="BaseUrl"/> and <see cref="LogoutPath"/>.</value>
		public string LogoutUrl => $"{BaseUrl}{LogoutPath}";

	}
}