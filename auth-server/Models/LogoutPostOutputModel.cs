namespace SimpleOidcOauth.Models
{
	/// <summary>Output model representing response data returned by the <see cref="SimpleOidcOauth.Controllers.AccountController" /> endpoint.</summary>
	public class LogoutPostOutputModel
	{
		/// <summary>The POST-logout redirection URL.</summary>
		/// <value>
		///     A URL to where the user will be redirected after the logout happened.
		///     This URL should take the user's agent back to the client application.
		/// </value>
		public string PostLogoutRedirectUri { get; set; }
		/// <summary>
		///     The URL to be rendered in an IFrame in the logout page, in order to log the user
		///     out from the client application.
		/// </summary>
		/// <value>
		///     A URL which needs to be rendered in the auth-server SPA's logout page, and which will
		///     trigger the logout of the user in the client application.
		/// </value>
		public string SignOutIFrameUrl { get; set; }
	}
}