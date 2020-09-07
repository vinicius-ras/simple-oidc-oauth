/** A service providing access to all of the application's configuration values during runtime.
 * Most of these configuration values are initialized through the use of environment variables.
 * See the project's ".env" file for a listing of these variables and their default values used
 * for development environments. */
export default abstract class AppConfigurationService {
	/** The URL pointing to the authentication/authorization server.
	 * This URL should contain the scheme (HTTP or HTTPS), the domain (and subdomain, if applicable) name, and optionally
	 * the port to be used to communicate with the authentication/authorization server.
	 * If the port is not specified, it defaults to port 80.
	 * Example: "https://my-auth-server.example.com:1234" */
	public static readonly AuthServerUrl = process.env.REACT_APP_AUTH_SERVER_URL || '';

	/** The name of the cookie to be used to detect if the user has logged in to the server. */
	public static readonly AuthenticationCookieName = process.env.REACT_APP_AUTHENTICATION_COOKIE_NAME || '';

	/** Contains URLs to the endpoints that this application communicates with. */
	public static Endpoints = class {
		/** The endpoint used to log the user in, by sending his/her credentials as a POST request. */
		public static readonly Login = `${AppConfigurationService.AuthServerUrl}/Account/Login`;
		/** The endpoint used to log the user out, by sending a POST request. */
		public static readonly Logout = `${AppConfigurationService.AuthServerUrl}/Account/Logout`;
	}
}
