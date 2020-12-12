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
		/// <summary>The SPA's login page URL.</summary>
		/// <value>The URL where auth server's SPA displays its login screen.</value>
		public string LoginUrl { get; set; }
		/// <summary>The SPA's logout page URL.</summary>
		/// <value>The URL where auth server's SPA displays its logout screen.</value>
		public string LogoutUrl { get; set; }
		/// <summary>The SPA's error page URL.</summary>
		/// <value>
		///     The URL where auth server's SPA displays a screen provinding error
		///     feedbacks to the user.
		/// </value>
		public string ErrorUrl { get; set; }
	}
}