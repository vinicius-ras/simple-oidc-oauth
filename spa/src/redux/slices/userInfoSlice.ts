import { Action, createSlice, PayloadAction } from "@reduxjs/toolkit";


/** The format of the data that is returned to this application once the user is logged in. */
export type UserInfoData = {
	/** A unique identifier associated to the logged-in user. This value does not change for a given user, even across multiple logins. */
	id: string;
	/** The username of the logged-in user. */
	name: string;
	/** The email of the logged-in user. */
	email: string;
	/** A list of claims the user has. */
	claims: SerializableClaim[];
}


/** Represents a claim, in its serializable format. */
export type SerializableClaim = {
	/** The type of claim represented by this object. */
	type: string;
	/** The value of the claim. */
	value: string;
}


/** Redux Slice representing the logged-in user's informations. */
const userInfoSlice = createSlice({
	name: "userInfo",
	initialState: null as (UserInfoData|null),
	reducers: {
		/** Updates the data about the currently logged-in user.
		 * @param state The current data about the user.
		 * @param action
		 *     The action which caused the update on the Redux Store's state.
		 *     This action should have a payload containing the new data about the user. */
		setUserInfo(state, action: PayloadAction<(UserInfoData|null)>) {
			return action.payload;
		},


		/** Clears the data about the currently logged-in user.
		 * This action should be fired once the user logs out.
		 * @param state The current data about the user.
		 * @param action The action which caused the update on the Redux Store's state. */
		clearUserInfo(state, action: Action) {
			return null;
		}
	}
});

export default userInfoSlice;