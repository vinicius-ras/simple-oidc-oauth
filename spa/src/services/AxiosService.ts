import Axios, { AxiosError, AxiosInstance } from "axios";
import HttpStatusCode from "http-status-codes";
import AppStore from "../redux/AppStore";
import errorResponseSlice, { ValidationProblemDetails } from "../redux/slices/errorResponseSlice";
import userInfoSlice from "../redux/slices/userInfoSlice";
import AppConfigurationService from "./AppConfigurationService";

/** A class which provides default configurations and events when using the Axios library for performing
 * requests to remote services.
 *
 * This class integrates with the {@link UserInfoService} in order to perform authenticated requests once
 * the user is logged-in. It also provides a default event treatment for when the remote server responds
 * with HTTP 401 (Unauthorized) responses, allowing the app to know when the user has been
 * unauthenticated (e.g., in cases of session expiration). */
class AxiosServiceType {
	// INSTANCE FIELDS
	/** The Axios instance that the application uses to send HTTP requests and receive responses,
	 * and to automate the treatment of some specific responses. */
	private _axiosInstance: AxiosInstance;





	// INSTANCE METHODS
	/** Constructor. */
	constructor() {
		this._axiosInstance = Axios.create();


		// Add a Request Interceptor which sends credentials to the remote server
		// if the user is currently logged-in
		this._axiosInstance.interceptors.request.use(requestConfigs => {
			// Sending a new request will reset UI errors
			AppStore.dispatch(errorResponseSlice.actions.clearError());

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
				const axiosError: AxiosError = httpNotOkResponse;
				const axiosResponse = axiosError.response;
				if (axiosResponse) {
					// If the response code was HTTP 401 (Unauthorized) and the app is currently treating the
					// user as "logged-in", then we should inform the UserInfoService that the user has been
					// logged out.
					if (axiosResponse.status === HttpStatusCode.UNAUTHORIZED) {
						const isUserLoggedIn = !!(AppStore.getState()?.userInfo);
						if (isUserLoggedIn)
							AppStore.dispatch(userInfoSlice.actions.clearUserInfo());
					}

					// If the response was a "Problem Details" response, update application's error states
					let responseContentTypeFull = ((axiosResponse.headers['content-type'] ?? '') as string).toLowerCase();
					const hasProblemDetailsContentType = responseContentTypeFull.split(';')
						.map(part => part.trim())
						.filter(part => !!part)
						.some(part => part === AppConfigurationService.CONTENT_TYPE_PROBLEM_DETAILS)
					if (hasProblemDetailsContentType) {
						const problemDetailsData = axiosResponse.data as ValidationProblemDetails;
						AppStore.dispatch(errorResponseSlice.actions.setError(problemDetailsData));
					}
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