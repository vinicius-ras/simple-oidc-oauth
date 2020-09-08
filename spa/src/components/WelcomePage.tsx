import { faSignInAlt, faUserPlus, faSpinner } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useState, useEffect } from "react";
import AppConfigurationService from "../services/AppConfigurationService";
import UserInfoService, { UserInfoData } from "../services/UserInfoService";
import ButtonLink from "./ButtonLink";
import WorkerButtonLinkWithIcon from "./WorkerButtonLinkWithIcon";

/** The application's home page.
 *
 * This is the page opened whenever the user accesses the root URL of the application. */
export default function WelcomePage() {
	const [isInitializing, setIsInitializing] = useState(true);
	const [isWaitingLogout, setIsWaitingLogout] = useState(false);

	const userInfo = UserInfoService.getUserInfo();


	// Initialization: verifies if the user is currently logged in
	useEffect(() => {
		const initializeWelcomePageAsync = async () => {
			// Is there any information about the user stored in Local Storage?
			if (userInfo) {
				// Verify if the user is really logged in, or if the data stored in Local Storage
				// is actually from an old/expired session
				let loginSessionVerified = true;
				try
				{
					const checkLoginResponse = await fetch(AppConfigurationService.Endpoints.CheckLogin, {
						credentials: "include",
					});
					if (checkLoginResponse.ok) {
						const retrievedUserData : UserInfoData = await checkLoginResponse.json();
						UserInfoService.updateUserInfo(retrievedUserData);
					}

					loginSessionVerified = checkLoginResponse.ok;
				} catch (err) {
					console.error(err);
					loginSessionVerified = false;
				}

				if (loginSessionVerified === false)
					UserInfoService.clearUserInfo();
			}

			// Finished initialization
			setIsInitializing(false);
		};
		initializeWelcomePageAsync();
	}, [userInfo]);


	/** Called when the user clicks the "Logout" button.
	 * @param event Object containing information about the click event. */
	const onLogoutButtonClick = async (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		event.preventDefault();

		setIsWaitingLogout(true);

		try {
			const response = await fetch(AppConfigurationService.Endpoints.Logout, {
				method: "POST",
				credentials: "include",
			});
			if (response.ok) {
				UserInfoService.clearUserInfo();
			}
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
									if (userInfo)
										return <span>Logged in as: <b>{userInfo.name}</b></span>
									else
										return <span>You are currently not logged in.</span>
								})()}
							</div>
							<div className="flex flex-row content-between mt-2">
								{(() => {
									if (userInfo)
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
									if (!userInfo)
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
