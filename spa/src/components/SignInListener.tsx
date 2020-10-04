import Lodash from 'lodash';
import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AppState } from '../redux/AppStore';
import userInfoSlice, { UserInfoData } from '../redux/slices/userInfoSlice';

/** Props for the {@link SignInListener} functional component. */
interface SignInListenerProps
{
}


/** A component which provides sign-in/sign-out information to multiple tabs of the user's browser. */
function SignInListener(props: SignInListenerProps) {
	const LOCAL_STORAGE_KEY = 'logged-in-user-info';

	const userInfo = useSelector((state: AppState) => state.userInfo);
	const dispatch = useDispatch();

	// Initialization: subscribe to receive Local Storage update events from other browser tabs (NOTE: the
	// current browser tab does NOT receive Local Storage update events, as per the Local Storage specification)
	useEffect(() => {
		/** Function which handles Local Storage update events.
		 * @param evtData Data about the event that was fired. */
		const localStorageEventHandler = (evtData: StorageEvent) => {
			// Process only Local Storage events (ignoring Session Storage), and only those
			// related to the key which stores user's login information
			if (evtData.storageArea !== localStorage || evtData.key !== LOCAL_STORAGE_KEY)
				return;

			// Dispatch a login/logout message to be broadcasted by the Redux Store
			const locallySavedData = localStorage.getItem(LOCAL_STORAGE_KEY);
			if (locallySavedData) {
				try
				{
					const result : UserInfoData = JSON.parse(locallySavedData);
					dispatch(userInfoSlice.actions.setUserInfo(result));
				} catch (err) {
					console.error("Failed to parse logged-in user's informations from Local Storage!", err);
					return;
				}
			}
			else
				dispatch(userInfoSlice.actions.clearUserInfo());
		};

		// Subscription and unsubscription to Local Storage events
		window.addEventListener("storage", localStorageEventHandler);
		return () => window.removeEventListener("storage", localStorageEventHandler);
	}, [dispatch]);

	// Update the Local Storage value whenever the user's data changes
	const localStorageContents = localStorage.getItem(LOCAL_STORAGE_KEY);
	if (userInfo) {
		// Verify if the contents of the Local Storage is different from the current user's information
		let updateLocalStorage = false;
		if (!localStorageContents)
			updateLocalStorage = true;
		else {
			try
			{
				const parsedLocalStorageContents: UserInfoData = JSON.parse(localStorageContents);
				if (!Lodash.isEqual(userInfo, parsedLocalStorageContents))
					updateLocalStorage = true;
			} catch (err) {
				console.error("Failed to parse user's data from Local Storage", err);
			}
		}

		// If contents are different, update the Local Storage.
		// This will fire Local Storage "update" events on other browser tabs which are running this app.
		if (updateLocalStorage)
			localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(userInfo));
	} else if (localStorageContents) {
		// If the user has logged out, update the Local Storage.
		// This will fire Local Storage "update" events on other browser tabs which are running this app.
		localStorage.removeItem(LOCAL_STORAGE_KEY);
	}

	// This component doesn't need to render anything
	return null;
}

export default SignInListener;