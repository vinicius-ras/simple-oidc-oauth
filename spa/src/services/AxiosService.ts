import Axios, { AxiosError, AxiosInstance } from "axios";
import HttpStatusCode from "http-status-codes";
import UserInfoService from "./UserInfoService";

/** A class which provides default configurations and events when using the Axios library for performing
 * requests to remote services.
 *
 * This class integrates with the {@link UserInfoService} in order to perform authenticated requests once
 * the user is logged-in. It also provides a default event treatment for when the remote server responds
 * with HTTP 401 (Unauthorized) responses, allowing the app to know when the user has been
 * unauthenticated (e.g., in cases of session expiration). */
class AxiosServiceType {
	// INSTANCE FIELDS
	private _axiosInstance: AxiosInstance;

	// INSTANCE METHODS
	/** Constructor. */
	constructor() {
		this._axiosInstance = Axios.create();


		// Add a Request Interceptor which sends credentials to the remote server
		// if the user is currently logged-in
		this._axiosInstance.interceptors.request.use(requestConfigs => {
			// Credentials will not be added if the request's configurations have explicitly
			// set a value specifying whether the credentials should be sent or not
			if (requestConfigs.withCredentials)
				return requestConfigs;

			// Whenever credentials are not specified, send them (e.g., to persist credential changes when
			// receiving an HTTP 401 response)
			requestConfigs.withCredentials = true;
			return requestConfigs;
		});


		// Add a Response Interceptor to treat HTTP 401 (Unauthorized) responses as
		// if the user's session has been expired, triggering the UserInfoService's appropriate methods.
		// This will also indirectly trigger an event fired by the UserInfoService informing subscribed
		// clients that the user has been disconnected ("logged out") from the remote server.
		// NOTE: only responses other than HTTP 200 (Ok) will be processed in the code below.
		this._axiosInstance.interceptors.response.use(
			http2xxResponse => {
				return http2xxResponse;
			},
			httpNotOkResponse => {
				// If the response code was HTTP 401 (Unauthorized) and the app is currently treating the
				// user as "logged-in", then we should inform the UserInfoService that the user has been
				// logged out.
				const axiosError: AxiosError = httpNotOkResponse;
				if (axiosError.response?.status === HttpStatusCode.UNAUTHORIZED) {
					const isUserLoggedIn = !!(UserInfoService.getUserInfo());
					if (isUserLoggedIn)
						UserInfoService.clearUserInfo();
				}
				return Promise.reject(httpNotOkResponse);
			});
	}


	/** Retrieves the Axios instance object, which can be used to perform requests.
	 * @returns {AxiosInstance}
	 *     A pre-configured Axios instance object which can be used to perform requests and
	 *     retrieve their responses. */
	getInstance() {
		return this._axiosInstance;
	}
}


/** A service providing access to a pre-configured Axios instance to be used by the application. */
const AxiosService = new AxiosServiceType();
export default AxiosService;