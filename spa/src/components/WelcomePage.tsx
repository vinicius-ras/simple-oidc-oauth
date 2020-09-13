import { faSignInAlt, faSpinner, faUserPlus } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { AxiosError } from "axios";
import React, { useEffect, useState } from "react";
import AppConfigurationService from "../services/AppConfigurationService";
import AxiosService from "../services/AxiosService";
import UserInfoService, { UserInfoData } from "../services/UserInfoService";
import ButtonLink from "./ButtonLink";
import WorkerButtonLinkWithIcon from "./WorkerButtonLinkWithIcon";

/** The application's home page.
 *
 * This is the page opened whenever the user accesses the root URL of the application. */
export default function WelcomePage() {
	const [isInitializing, setIsInitializing] = useState(true);
	const [isWaitingLogout, setIsWaitingLogout] = useState(false);
	const [loggedUserInfo, setLoggedUserInfo] = useState(UserInfoService.getUserInfo());


	// Initialization: subscribe to events fired when the user changes his/her login state
	useEffect(() => {
		/** Called for any events where the user's login state changes.
		 * @param newUserData New, updated data about the user. */
		const onUserLoginDataChanged = (newUserData: UserInfoData | null) => {
			setLoggedUserInfo(newUserData);
		};


		// Subscription and cleanup (when component is unmounted) for receiving user login related callbacks
		UserInfoService.subscribe(onUserLoginDataChanged);
		return () => {
			UserInfoService.unsubscribe(onUserLoginDataChanged)
		};
	}, []);


	// Initialization: verifies if the user is currently logged in
	useEffect(() => {
		const initializeWelcomePageAsync = async () => {
			// Is there any information about the user stored in Local Storage?
			const hasLocalStorageUserData = !!UserInfoService.getUserInfo();
			if (hasLocalStorageUserData) {
				// Verify if the user is really logged in, or if the data stored in Local Storage
				// is actually from an old/expired session
				let loginSessionIsValid = true;
				try
				{
					const response = await AxiosService.getInstance()
						.get<UserInfoData>(
							AppConfigurationService.Endpoints.CheckLogin
						);

					UserInfoService.updateUserInfo(response.data);
				} catch (err) {
					const axiosError: AxiosError = err;
					if (!!axiosError.response) {
						console.error("Failed to check login", err);
						loginSessionIsValid = false;
					}
				}

				if (loginSessionIsValid === false)
					UserInfoService.clearUserInfo();
			}

			// Finished initialization
			setIsInitializing(false);
		};
		initializeWelcomePageAsync();
	}, []);


	/** Called when the user clicks the "Logout" button.
	 * @param event Object containing information about the click event. */
	const onLogoutButtonClick = async (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		event.preventDefault();

		setIsWaitingLogout(true);

		try {
			AxiosService.getInstance()
				.post(AppConfigurationService.Endpoints.Logout);
			UserInfoService.clearUserInfo();
		} catch(err) {
			console.error("Logout failed", err);
		}

		setIsWaitingLogout(false);
	}


	// Render the component
	return (
		<div className="component-WelcomePage text-sm">
			{(() => {
				if (isInitializing)
					return (
						<div className="flex flex-col items-center pt-6">
							<FontAwesomeIcon icon={faSpinner} className="mr-2" spin={true} size="6x" />
							<span className="mt-2">Loading, please wait...</span>
						</div>
					);
				else
					return (
						<React.Fragment>
							<div>
								{(() => {
									if (loggedUserInfo)
										return <span>Logged in as: <b>{loggedUserInfo.name}</b></span>
									else
										return <span>You are currently not logged in.</span>
								})()}
							</div>
							<div className="flex flex-row content-between mt-2">
								{(() => {
									if (loggedUserInfo)
										return (
											<WorkerButtonLinkWithIcon to="/" icon={faSignInAlt} isBusy={isWaitingLogout} className="mr-2 bg-yellow-600" onClick={onLogoutButtonClick}>
												<span>Log out</span>
											</WorkerButtonLinkWithIcon>
										);
									else
										return (
											<ButtonLink to="/login" className="mr-2 flex-grow">
												<FontAwesomeIcon icon={faSignInAlt} className="mr-2" />
												<span>Log in</span>
											</ButtonLink>
										);
								})()}
								{(() => {
									if (!loggedUserInfo)
										return (
											<ButtonLink to="/login" className="flex-grow">
												<FontAwesomeIcon icon={faUserPlus} className="mr-2"/>
												<span>Sign up</span>
											</ButtonLink>
										);
									else
										return null;
								})()}
							</div>
						</React.Fragment>
					);
			})()}
		</div>
	);
}
