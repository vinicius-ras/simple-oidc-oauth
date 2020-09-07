import { faSignInAlt, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import React, { useState } from 'react';
import ButtonLinkWithIcon from './ButtonLinkWithIcon';
import WorkerButtonLinkWithIcon from './WorkerButtonLinkWithIcon';
import AppConfigurationService from '../services/AppConfigurationService';
import UserInfoServiceType, { UserInfoData } from '../services/UserInfoService';
import UserInfoService from '../services/UserInfoService';
import { UserInfo } from 'os';
import { Redirect } from 'react-router-dom';

/** Props for the {@link UserCredentialsPage} functional component. */
interface UserCredentialsPageProps
{
}


/** A component which allows users to enter their credentials in order to perform a login,
 * or to register themselves. */
function UserCredentialsPage(props: UserCredentialsPageProps) {
	const loggedInUserInfo = UserInfoService.getUserInfo();

	const [userEmail, setUserEmail] = useState(loggedInUserInfo?.email || '');
	const [userPassword, setUserPassword] = useState('');
	const [isWaitingLogin, setIsWaitingLogin] = useState(false);
	const [isUserLoggedIn, setUserLoggedIn] = useState(!!loggedInUserInfo);


	/** Called when the user clicks the "login" button, to submit his/her credentials.
	 * @param event Object containing information about the click event. */
	const onLoginButtonClick = async (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		event.preventDefault();

		setIsWaitingLogin(true);

		try
		{
			const response = await fetch(AppConfigurationService.Endpoints.Login, {
				method: "POST",
				headers: {
					"Content-type": "application/json; charset=UTF-8"
				},
				body: JSON.stringify({
					"email": userEmail,
					"password": userPassword,
				}),
				credentials: "include",
			});

			if (response.ok) {
				const jsonResponse = (await response.json()) as UserInfoData;
				UserInfoService.updateUserInfo(jsonResponse);
				setUserLoggedIn(true);
			}
			else
				UserInfoService.clearUserInfo();
		} catch (err) {
			console.error("Login attempt failed", err);
		}

		setIsWaitingLogin(false);
	}


	// Render the component
	const extraButtonClasses = (isWaitingLogin || isUserLoggedIn)
		? 'cursor-not-allowed opacity-50'
		: '';

	return (
		<div className="component-UserCredentialsPage">
			<div className="flex flex-col">
				<input type="email" placeholder="E-mail" value={userEmail} onChange={(evt) => setUserEmail(evt.target.value)} className="border border-gray-500 rounded-lg p-2" disabled={isUserLoggedIn} />
				<input type="password" placeholder="Password" value={userPassword} onChange={(evt) => setUserPassword(evt.target.value)} className="border border-gray-500 rounded-lg p-2 mt-2" disabled={isUserLoggedIn} />
				<WorkerButtonLinkWithIcon to="/" icon={faSignInAlt} isBusy={isWaitingLogin} className={`mt-2 self-end ${extraButtonClasses}`} onClick={onLoginButtonClick}>
					<span>Log in</span>
				</WorkerButtonLinkWithIcon>
			</div>
			<div className="flex flex-row items-center my-2">
				<hr className="flex-grow" />
				<span className="mx-2">OR</span>
				<hr className="flex-grow" />
			</div>
			<div className="flex flex-col">
				<ButtonLinkWithIcon to="/" icon={['fab', 'facebook-square']} className={`my-2 ${extraButtonClasses}`}>
					<span>Login with Facebook</span>
				</ButtonLinkWithIcon>
				<ButtonLinkWithIcon to="/" icon={['fab', 'google']} className={extraButtonClasses}>
					<span>Login with Google</span>
				</ButtonLinkWithIcon>
			</div>
			<div className="flex flex-row items-center my-2">
				<hr className="flex-grow" />
				<span className="mx-2">OR</span>
				<hr className="flex-grow" />
			</div>
			<div className="flex flex-col">
				<WorkerButtonLinkWithIcon to="/" icon={faUserPlus} className={`mt-2 ${extraButtonClasses}`}>
					<span>Sign up</span>
				</WorkerButtonLinkWithIcon>
			</div>


			{
				/* Redirects the user whenever he/she is already logged in. */
				isUserLoggedIn

				?
				<Redirect to="/" />

				: ''
			}
		</div>
	);
}

export default UserCredentialsPage;