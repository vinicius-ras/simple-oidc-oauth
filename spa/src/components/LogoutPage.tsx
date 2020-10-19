import { faSignOutAlt } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { AxiosResponse } from 'axios';
import React, { useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import { useLocation } from 'react-router-dom';
import userInfoSlice from '../redux/slices/userInfoSlice';
import AppConfigurationService from '../services/AppConfigurationService';
import AxiosService from '../services/AxiosService';
import ButtonLink from './ButtonLink';

/** Props for the {@link LogoutPage} functional component. */
interface LogoutPageProps
{
}


/** Models the response that is returned by the sign-out endpoint from the auth-server. */
interface LogoutPostOutputModel
{
	/** The URL which needs to be loaded up in an IFrame in order to finish logging the user out. */
	signOutIFrameUrl: string;
	/** The URI to which the user should be redirected once sign-out procedures are completed.
	 * The sign-out procedures are considered to be completed after the Sign-Out IFrame has been
	 * rendered and finished loading it's target URL. */
	postLogoutRedirectUri: string;
}


/** Page wich deals with user's sign-out logic, including sign-out consent (when needed), and
 * calling the appropriate sign-out endpoints from the auth-server. */
function LogoutPage(props: LogoutPageProps) {
	const dispatch = useDispatch();
	const location = useLocation();

	const [iframeUrl, setIframeUrl] = useState<string|null>(null);
	const [redirectUrl, setRedirectUrl] = useState<string|null>(null);
	const [performRedirect, setPerformRedirect] = useState(false);




	// EFFECT: redirect the user agent to the target redirection URL when necessary
	useEffect(() => {
		if (!performRedirect)
			return;
		window.location.href = redirectUrl ?? window.location.origin;
	}, [redirectUrl, performRedirect]);



	/** Function which performs a call to the endpoint which will log the user out. */
	const callLogoutEndpoint = async (evt?: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		evt?.preventDefault();

		try
		{
			const queryParamLogoutId = "logoutId";

			// Read the auth-server logout ID.
			// The auth-server generates this ID at the End Session Endpoint, and it stores
			// information related to the logout process' context, like: the user's session identifier,
			// client app, post-logout redirect URL, etc.
			const currentPageQueryParams = new URLSearchParams(location.search);
			const logoutIdValue = currentPageQueryParams.get(queryParamLogoutId);
			if (!logoutIdValue)
				throw new Error("Missing LOGOUT ID in current page's URL");

			const queryParams = new URLSearchParams();
			queryParams.append(queryParamLogoutId, logoutIdValue);


			// Call the target endpoint
			const targetEndpointUrl = `${AppConfigurationService.Endpoints.Logout}?${queryParams.toString()}`;
			const response: AxiosResponse<LogoutPostOutputModel> = await AxiosService.getInstance()
				.post(targetEndpointUrl);

			setIframeUrl(response.data.signOutIFrameUrl ?? null);
			setRedirectUrl(response.data.postLogoutRedirectUri ?? null);

			// If the logout endpoint didn't return an IFrame URI, the logout page should be
			// redirected straight away (there's no need to render a "Sign-out IFrame" before redirecting)
			if (!response.data.signOutIFrameUrl)
				setPerformRedirect(true);

			// Dispatch a logout event
			dispatch(userInfoSlice.actions.clearUserInfo());
		} catch (err) {
			console.error(err);
		}
	};


	return (
		<div className="component-LogoutPage">
			{(() => {
				// Renders the IFrame which will perform a sign-out on the client apps
				if (iframeUrl)
					return <iframe title={"Client sign-out IFrame"} width={0} height={0} src={iframeUrl} onLoad={() => setPerformRedirect(true)} />
				return null;
			})()}
			<ButtonLink to="/" onClick={callLogoutEndpoint}>
				<FontAwesomeIcon icon={faSignOutAlt} />
				<span className="ml-2">Confirm logout</span>
			</ButtonLink>
		</div>
	);
}

export default LogoutPage;