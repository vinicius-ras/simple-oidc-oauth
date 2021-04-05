/** A service providing access to all of the application's configuration values during runtime.
 * Most of these configuration values are initialized through the use of environment variables.
 * See the project's ".env" file for a listing of these variables and their default values used
 * for development environments. */
export default abstract class AppConfigurationService {
	// CONSTANTS
	/** The Content-Type header value which identifies a JSON-serialized "Problem Details" (RFC 7807) response from the
	 * server. */
	public static readonly CONTENT_TYPE_PROBLEM_DETAILS = "application/problem+json";





	// CONFIGURABLE VALUES (EXTRACTED OR DERIVED FROM ENVIRONMENT VARIABLES)
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
		public static readonly Login = `${AppConfigurationService.AuthServerUrl}/api/account/login`;
		/** The endpoint used to log the user out, by sending a POST request. */
		public static readonly Logout = `${AppConfigurationService.AuthServerUrl}/api/account/logout`;
		/** The endpoint used to check if the user has a valid session with the authentication/authorization server, by sending
		 * a GET request. */
		public static readonly CheckLogin = `${AppConfigurationService.AuthServerUrl}/api/account/check-login`;
		/** The endpoint used to retrieve all of the Client Applications registered in the IdP Server. */
		public static readonly GetAllRegisteredClients = `${AppConfigurationService.AuthServerUrl}/api/management/clients`;
		/** The endpoint used to retrieve a specific Client Application's data from the IdP Server. */
		public static readonly GetRegisteredClient = (clientId: string) => `${AppConfigurationService.AuthServerUrl}/api/management/clients/${clientId}`;
		/** The endpoint used to retrieve the Grant Types which are allowed for Client Application registration in the IdP Server. */
		public static readonly GetAllowedClientRegistrationGrantTypes = `${AppConfigurationService.AuthServerUrl}/api/management/clients/allowed-grant-types`;
		/** The endpoint used to retrieve the Resources (API Scopes, API Resources and Identity Resources) which are currently available for Client Application
		 * registration in the IdP Server. */
		public static readonly GetAvailableClientRegistrationResources = `${AppConfigurationService.AuthServerUrl}/api/management/clients/available-resources`;
		/** The endpoint used to register a new Client Application. */
		public static readonly CreateNewClientApplication = `${AppConfigurationService.AuthServerUrl}/api/management/clients`;
		/** The endpoint used to update an existing Client Application. */
		public static readonly UpdateClientApplication = (clientId: string) => `${AppConfigurationService.AuthServerUrl}/api/management/clients/${clientId}`;
	 }
}
