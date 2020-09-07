/** The format of the data that is returned to this application once the user is logged in. */
export type UserInfoData = {
	/** A unique identifier associated to the logged-in user. This value does not change for a given user, even across multiple logins. */
	id: string;
	/** The username of the logged-in user. */
	name: string;
	/** The email of the logged-in user. */
	email: string;
}


/** Class defining a service for accessing the currently logged-in user's informations. */
class UserInfoServiceType {
	// CONSTANTS
	/** The key to be used to store the user's information at the browser's Local Storage. */
	private static readonly LOCAL_STORAGE_KEY = 'user-info';





	// INSTANCE FIELDS
	/** The list of callbacks representing the clients currently subscribed to this service. */
	private callbackSubscriptions : Array<(newUserData: UserInfoData|null) => void>;





	// INSTANCE METHODS
	/** Constructor. */
	constructor() {
		this.callbackSubscriptions = new Array<()=>void>();
	}


	/** Update the currently logged in user's information that is stored in the browser's Local Storage.
	 * @param newUserInfo An object containing the user's information to be stored in the browser's Local Storage. */
	updateUserInfo(newUserInfo: UserInfoData) : void {
		localStorage.setItem(UserInfoServiceType.LOCAL_STORAGE_KEY, JSON.stringify(newUserInfo));
		this.callbackSubscriptions.forEach(callback => callback(newUserInfo));
	}


	/** Clears the currently logged in user's information from the browser's Local Storage. */
	clearUserInfo() : void {
		localStorage.removeItem(UserInfoServiceType.LOCAL_STORAGE_KEY);
		this.callbackSubscriptions.forEach(callback => callback(null));
	}


	/** Retrieves information about the currently logged in user.
	 * @returns {UserInfoData | null}
	 *     In case of success, returns the user information as a {@link UserInfoData} object.
	 *     In case of failure, returns `null`. */
	getUserInfo() : UserInfoData | null {
		const locallySavedData = localStorage.getItem(UserInfoServiceType.LOCAL_STORAGE_KEY);
		if (!locallySavedData)
			return null;

		try
		{
			const result : UserInfoData = JSON.parse(locallySavedData);
			return result;
		} catch (err) {
			console.error(err);

			// Local storage entry is probably corrupted, so we should remove it
			this.clearUserInfo();
		}
		return null;
	}


	/** Subscribes a callback function to be called whenever there is any update to the currently
	 * logged in user's informations.
	 * @param callback
	 *     The callback method to be called when there is any update.
	 *     When the user performs a login or when the user's information gets updated, subscribed callbacks
	 *     will be called with a {@link UserInfoData} object containing the new/updated user's informations.
	 *     When the user is logged out, subscribed callbacks will be called with a `null` argument.
	 * @see {@link unsubscribe}
	 */
	subscribe(callback: (newUserData: UserInfoData|null) => void) {
		this.callbackSubscriptions.push(callback);
	}


	/** Unsubscribes a callback function, so that it won't be called anymore whenever a logged in user's
	 * information gets updated.
	 * @param callback
	 * @see {@link subscribe}
	 */
	unsubscribe(callback: (newUserData: UserInfoData|null) => void) : boolean {
		const callbackIndex = this.callbackSubscriptions.indexOf(callback);
		if (callbackIndex >= 0)
			this.callbackSubscriptions.splice(callbackIndex, 1);
		return (callbackIndex >= 0);
	}
};


/** An object which allows the application to access the currently logged-in user's information. */
const UserInfoService = new UserInfoServiceType();
export default UserInfoService;