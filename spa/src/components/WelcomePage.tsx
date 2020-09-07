import { faSignInAlt, faUserPlus } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useState } from "react";
import AppConfigurationService from "../services/AppConfigurationService";
import UserInfoService from "../services/UserInfoService";
import ButtonLink from "./ButtonLink";
import WorkerButtonLinkWithIcon from "./WorkerButtonLinkWithIcon";

/** The application's home page.
 *
 * This is the page opened whenever the user accesses the root URL of the application. */
function WelcomePage() {
	const [isWaitingLogout, setIsWaitingLogout] = useState(false);

	const userInfo = UserInfoService.getUserInfo();


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
			<div>
				{userInfo
					?
					<span>Logged in as: <b>{userInfo.name}</b></span>

					:
					<span>You are currently not logged in.</span>
				}
			</div>
			<div className="flex flex-row content-between mt-2">
				{userInfo
					?
					<WorkerButtonLinkWithIcon to="/" icon={faSignInAlt} isBusy={isWaitingLogout} className="mr-2 bg-yellow-600" onClick={onLogoutButtonClick}>
						<span>Log out</span>
					</WorkerButtonLinkWithIcon>

					:
					<ButtonLink to="/login" className="mr-2 flex-grow">
						<FontAwesomeIcon icon={faSignInAlt} className="mr-2" />
						<span>Log in</span>
					</ButtonLink>
				}
				{
					!userInfo

					?
					<ButtonLink to="/login" className="flex-grow">
						<FontAwesomeIcon icon={faUserPlus} className="mr-2"/>
						<span>Sign up</span>
					</ButtonLink>

					: null
				}
			</div>
		</div>
	);
}

export default WelcomePage;