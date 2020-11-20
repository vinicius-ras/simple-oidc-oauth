import { faSignInAlt, faUserPlus } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { AppState } from "../redux/AppStoreCreation";
import userInfoSlice from "../redux/slices/userInfoSlice";
import AppConfigurationService from "../services/AppConfigurationService";
import AxiosService from "../services/AxiosService";
import ButtonLink from "./ButtonLink";
import WorkerButtonLinkWithIcon from "./WorkerButtonLinkWithIcon";

/** The application's home page.
 *
 * This is the page opened whenever the user accesses the root URL of the application. */
export default function WelcomePage() {
	const loggedUserInfo = useSelector((state: AppState) => state.userInfo);
	const dispatch = useDispatch();

	const [isWaitingLogout, setIsWaitingLogout] = useState(false);


	/** Called when the user clicks the "Logout" button.
	 * @param event Object containing information about the click event. */
	const onLogoutButtonClick = async (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		event.preventDefault();

		setIsWaitingLogout(true);

		try {
			AxiosService.getInstance()
				.post(AppConfigurationService.Endpoints.Logout);
			dispatch(userInfoSlice.actions.clearUserInfo());
		} catch(err) {
			console.error("Logout failed", err);
		}

		setIsWaitingLogout(false);
	}


	// Render the component
	return (
		<div className="component-WelcomePage text-sm">
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
		</div>
	);
}
