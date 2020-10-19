import { faSignInAlt, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Redirect, useLocation } from 'react-router-dom';
import { AppState } from '../redux/AppStore';
import userInfoSlice, { UserInfoData } from '../redux/slices/userInfoSlice';
import AppConfigurationService from '../services/AppConfigurationService';
import AxiosService from '../services/AxiosService';
import ButtonLinkWithIcon from './ButtonLinkWithIcon';
import WorkerButtonLinkWithIcon from './WorkerButtonLinkWithIcon';

/** Props for the {@link UserCredentialsPage} functional component. */
export interface UserCredentialsPageProps
{
}


/** Response data sent by the endpoint upon a successful user sign-in. */
type LoginResponseData = UserInfoData & { returnUrl: string };


/** A component which allows users to enter their credentials in order to perform a login,
 * or to register themselves. */
export default function UserCredentialsPage(props: UserCredentialsPageProps) {
	const loggedInUserInfo = useSelector((state: AppState) => state.userInfo);
	const dispatch = useDispatch();
	const location = useLocation();

	const [userEmail, setUserEmail] = useState(loggedInUserInfo?.email || '');
	const [userPassword, setUserPassword] = useState('');
	const [redirectUrl, setRedirectUrl]= useState<string|null>(null);
	const [isWaitingLogin, setIsWaitingLogin] = useState(false);

	const isUserLoggedIn = (!!loggedInUserInfo);

	let isLocalRedirectUrl = true;
	try {
		if (redirectUrl)
			isLocalRedirectUrl = (new URL(redirectUrl).origin === window.location.origin);
	} catch (err) {
		console.error(`Error parsing login page's redirect URL: `, err);
	}



	/** Called when the user clicks the "login" button, to submit his/her credentials.
	 * @param event Object containing information about the click event. */
	const onLoginButtonClick = async (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		event.preventDefault();

		setIsWaitingLogin(true);

		try
		{
			// Extract the "return URL" from the current URL's query string parameters
			const urlSearchParams = new URLSearchParams(location.search);
			const returnUrl = urlSearchParams.get("ReturnUrl") ?? "/";

			// Try to perform the login, keeping any returned credentials
			const axiosResponse = await AxiosService.getInstance()
				.post<LoginResponseData>(
					AppConfigurationService.Endpoints.Login,
					{
						"email": userEmail,
						"password": userPassword,
						returnUrl
					},
					{withCredentials: true},
				);

			// If the user needs to be redirected to another origin (scheme + host + port), do it here.
			// Else, if the use needs to be redirected to a local URL in our app or if there's no
			// redirection URL defined, we will render a Redirect component that will perform a local
			// redirect to another app's page
			const responseData = axiosResponse.data;
			setRedirectUrl(responseData.returnUrl);

			// Update logged user's information
			dispatch(userInfoSlice.actions.setUserInfo(responseData));
			return;
		} catch (err) {
			if (isUserLoggedIn)
				dispatch(userInfoSlice.actions.clearUserInfo());
		}

		setIsWaitingLogin(false);
	}


	// EFFECT: redirects the user to an external page, whenever necessary
	useEffect(() => {
		if (isLocalRedirectUrl || !redirectUrl)
			return;
		window.location.href = redirectUrl;
	}, [redirectUrl, isLocalRedirectUrl]);


	// Render the component
	const disableButtons = (isWaitingLogin || isUserLoggedIn);

	return (
		<div className="component-UserCredentialsPage">
			<div className="flex flex-col">
				<input type="email" placeholder="E-mail" value={userEmail} onChange={(evt) => setUserEmail(evt.target.value)} className="border border-gray-500 rounded-lg p-2" disabled={isWaitingLogin} />
				<input type="password" placeholder="Password" value={userPassword} onChange={(evt) => setUserPassword(evt.target.value)} className="border border-gray-500 rounded-lg p-2 mt-2" disabled={isWaitingLogin} />
				<WorkerButtonLinkWithIcon to="/" icon={faSignInAlt} isBusy={isWaitingLogin} className="mt-2 self-end" disabled={disableButtons} onClick={onLoginButtonClick}>
					<span>Log in</span>
				</WorkerButtonLinkWithIcon>
			</div>
			<div className="flex flex-row items-center my-2">
				<hr className="flex-grow" />
				<span className="mx-2">OR</span>
				<hr className="flex-grow" />
			</div>
			<div className="flex flex-col">
				<ButtonLinkWithIcon to="/" icon={['fab', 'facebook-square']} className="my-2" disabled={disableButtons}>
					<span>Login with Facebook</span>
				</ButtonLinkWithIcon>
				<ButtonLinkWithIcon to="/" icon={['fab', 'google']} disabled={disableButtons}>
					<span>Login with Google</span>
				</ButtonLinkWithIcon>
			</div>
			<div className="flex flex-row items-center my-2">
				<hr className="flex-grow" />
				<span className="mx-2">OR</span>
				<hr className="flex-grow" />
			</div>
			<div className="flex flex-col">
				<WorkerButtonLinkWithIcon to="/" icon={faUserPlus} className="mt-2" disabled={disableButtons}>
					<span>Sign up</span>
				</WorkerButtonLinkWithIcon>
			</div>


			{
				// Redirects the user whenever a local redirect must be performed
				(isUserLoggedIn && isLocalRedirectUrl)
					? <Redirect to={redirectUrl ?? '/'} />
					: null
			}
		</div>
	);
}
