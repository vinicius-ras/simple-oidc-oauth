import { faSpinner } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { AxiosError } from 'axios';
import HttpStatusCode from "http-status-codes";
import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import userInfoSlice, { UserInfoData } from '../redux/slices/userInfoSlice';
import AppConfigurationService from '../services/AppConfigurationService';
import AxiosService from '../services/AxiosService';
import ButtonLink from './ButtonLink';

// CONSTANTS
/** The initial waiting time that should be applied when the app fails to verify the user's
 * session state with the authentication server. The wait time increases with each attempt,
 * until it reaches the maximum wait time (defined by {@link MAXIMUM_WAIT_TIME_BEFORE_RETRY_MS}). */
const INITIAL_WAIT_TIME_BEFORE_RETRY_MS = 5000;

/** The maximum waiting time that should be applied when the app fails to verify the user's session state.
 * See {@link INITIAL_WAIT_TIME_BEFORE_RETRY_MS} for more information. */
const MAXIMUM_WAIT_TIME_BEFORE_RETRY_MS = 30000;





// TYPE DEFINITIONS
/** Props for the {@link RequireAppInitialization} functional component. */
interface RequireAppInitializationProps
{
	children: JSX.Element|JSX.Element[];
}





// COMPONENT
/** A component which performs the app's initialization, while also blocking it's children from
 * being rendered until the app is fully initialized. */
function RequireAppInitialization(props: RequireAppInitializationProps) {
	const dispatch = useDispatch();
	const [isAppInitialized, setIsAppInitialized] = useState(false);
	const [failedAttempts, setFailedAttempts] = useState(0);
	const [secondsBeforeRetry, setSecondsBeforeRetry] = useState(0);

	const [waitRetryPromiseData, setWaitRetryPromiseData] = useState<{resolve: ((value?: unknown) => void)}|null>(null);


	/** Called after waiting for a while to setup the component to retry retrieving user's information from
	 * the auth server. */
	const retryConnection = useCallback(() => {
		setSecondsBeforeRetry(0);
		setWaitRetryPromiseData(null);
		waitRetryPromiseData?.resolve();
	}, [waitRetryPromiseData]);



	// Initialization: verifies whether the user is currently logged in or not
	useEffect(() => {
		const verifyUserCurrentSignInState = async () => {
			// If we're currently retrying to query the auth server, wait for some time
			// before reattempting communication with the auth server
			if (failedAttempts >= 1) {
				await new Promise(resolve => {
					setWaitRetryPromiseData({resolve: resolve});
				});
			}

			// Query the appropriate endpoint to verify if the user is currently logged in
			let appInitialized = false;
			try
			{
				const response = await AxiosService.getInstance()
					.get<UserInfoData>(
						AppConfigurationService.Endpoints.CheckLogin
					);

				// HTTP 200 responses indicate the user is currently logged in, with an active session
				dispatch(userInfoSlice.actions.setUserInfo(response.data));
				appInitialized = true;
			} catch (err) {
				const axiosError: AxiosError = err;

				// HTTP 401 status codes indicate the user is currently not logged in (or session has expired)
				if (axiosError.response?.status === HttpStatusCode.UNAUTHORIZED) {
					dispatch(userInfoSlice.actions.clearUserInfo());
					appInitialized = true;
				}
			}

			if (appInitialized)
				setIsAppInitialized(true);
			else
				setFailedAttempts(numAttempts => numAttempts + 1);
		};
		verifyUserCurrentSignInState();
	}, [dispatch, failedAttempts]);


	// Implements the timers/intervals necessary to retry the connection with the auth server and update the UI accordingly
	useEffect(() => {
		if (waitRetryPromiseData === null)
			return;

		// Start a timeout which will be used to perform the retry
		const timeToWaitMs = Math.min(
			failedAttempts * INITIAL_WAIT_TIME_BEFORE_RETRY_MS,
			MAXIMUM_WAIT_TIME_BEFORE_RETRY_MS
		);

		setSecondsBeforeRetry(timeToWaitMs / 1000);
		const timeoutId = setTimeout(retryConnection, timeToWaitMs);

		// Enable an interval which updates the UI timer's countdown
		const intervalId = setInterval(() => {
			setSecondsBeforeRetry(curDisplayedSeconds => Math.max(curDisplayedSeconds - 1, 0))
		}, 1000);

		// Effect's clean up code
		return () => {
			clearInterval(intervalId);
			clearTimeout(timeoutId);
		};
	}, [waitRetryPromiseData, failedAttempts, retryConnection]);


	// If the app is initialized, render the children directly
	if (isAppInitialized)
		return (
			<React.Fragment>{props.children}</React.Fragment>
		);


	// If the app is NOT initialized, render a loading screen
	return (
		<div className="flex flex-col items-center pt-6">
			<FontAwesomeIcon icon={faSpinner} className="mr-2" spin={true} size="6x" />
			{(() => {
				if (failedAttempts < 1)
					return <span>Loading, please wait...</span>;
				return (
					<React.Fragment>
						{(() => {
							if (waitRetryPromiseData)
								return (
									<React.Fragment>
										<span className="mt-4">Retrying in {`${secondsBeforeRetry} second${secondsBeforeRetry > 1 ? 's' : ''}`}</span>
										<ButtonLink to="/" onClick={() => {retryConnection()}} className="text-xs p-1 mt-1">Retry now</ButtonLink>
									</React.Fragment>
								);
							return (
								<React.Fragment>
									<span>Retrying connection to server...</span>
									<div className="text-gray-400 text-xs">(attempt #{failedAttempts+1})</div>
								</React.Fragment>
							);
						})()}
					</React.Fragment>
				)
			})()}
		</div>
	);
}

export default RequireAppInitialization;