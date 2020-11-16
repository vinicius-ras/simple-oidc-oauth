import { configureStore } from "@reduxjs/toolkit";
import errorResponseSlice from "./slices/errorResponseSlice";
import userInfoSlice from "./slices/userInfoSlice";

/** Called to create a new {@link AppStore} instance.
 * This method is used both in the web app code and in the tests code
 * to instantiate a Redux Store that can be used by the application. */
const createAppStore = () => configureStore({
	reducer: {
		userInfo: userInfoSlice.reducer,
		errorResponse: errorResponseSlice.reducer,
	},
});


/** The type representing the application's state, which can be extracted from
 * the application's Redux Store. */
export type AppState = ReturnType<(ReturnType<typeof createAppStore>)["getState"]>;
export default createAppStore;