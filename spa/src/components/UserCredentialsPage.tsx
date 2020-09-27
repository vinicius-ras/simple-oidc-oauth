import { faSignInAlt, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Redirect } from 'react-router-dom';
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


/** A component which allows users to enter their credentials in order to perform a login,
 * or to register themselves. */
export default function UserCredentialsPage(props: UserCredentialsPageProps) {
	const loggedInUserInfo = useSelector((state: AppState) => state.userInfo);
	const dispatch = useDispatch();

	const [userEmail, setUserEmail] = useState(loggedInUserInfo?.email || '');
	const [userPassword, setUserPassword] = useState('');
	const [isWaitingLogin, setIsWaitingLogin] = useState(false);

	const isUserLoggedIn = (!!loggedInUserInfo);




	/** Called when the user clicks the "login" button, to submit his/her credentials.
	 * @param event Object containing information about the click event. */
	const onLoginButtonClick = async (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		event.preventDefault();

		setIsWaitingLogin(true);

		try
		{
			// Try to perform the login, keeping any returned credentials
			const axiosResponse = await AxiosService.getInstance()
				.post<UserInfoData>(
					AppConfigurationService.Endpoints.Login,
					{
						"email": userEmail,
						"password": userPassword,
					},
					{withCredentials: true},
				);

			// Update logged user's information
			dispatch(userInfoSlice.actions.setUserInfo(axiosResponse.data));
			return;
		} catch (err) {
			if (isUserLoggedIn)
				dispatch(userInfoSlice.actions.clearUserInfo());
		}

		setIsWaitingLogin(false);
	}


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
				/* Redirects the user whenever he/she is already logged in. */
				isUserLoggedIn

				?
				<Redirect to="/" />

				: ''
			}
		</div>
	);
}
