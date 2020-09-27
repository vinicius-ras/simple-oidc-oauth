import { faSignInAlt, faSpinner, faUserPlus } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { AxiosError } from "axios";
import HttpStatusCode from "http-status-codes";
import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { AppState } from "../redux/AppStore";
import userInfoSlice, { UserInfoData } from "../redux/slices/userInfoSlice";
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

	const [isInitializing, setIsInitializing] = useState(true);
	const [isWaitingLogout, setIsWaitingLogout] = useState(false);


	// Initialization: verifies if the user is currently logged in
	useEffect(() => {
		const initializeWelcomePageAsync = async () => {
			// Verify if the user is really logged in, or if the data stored in Local Storage
			// is actually from an old/expired session
			try
			{
				const response = await AxiosService.getInstance()
					.get<UserInfoData>(
						AppConfigurationService.Endpoints.CheckLogin
					);

				dispatch(userInfoSlice.actions.setUserInfo(response.data));
			} catch (err) {
				const axiosError: AxiosError = err;
				if (axiosError.response?.status !== HttpStatusCode.UNAUTHORIZED) {
					// TODO: an error that was not HTTP 200 (Ok) or HTTP 401 (Unauthorized) has occurred.
					// Here, we are considering that the user should be "logged out", but that is incorrect behavior.
					// Ideally, if any response that is not HTTP 200 or HTTP 401 is returned by the API, the request
					// should be retried (because the auth server might be offline, or currently in an error state).
					dispatch(userInfoSlice.actions.clearUserInfo());
				}
			}

			// Finished initialization
			setIsInitializing(false);
		};
		initializeWelcomePageAsync();
	}, [dispatch]);


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
