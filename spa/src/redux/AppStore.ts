import { configureStore } from "@reduxjs/toolkit";
import userInfoSlice from "./slices/userInfoSlice";

/** The Redux Store instance used by the application. */
const AppStore = configureStore({
	reducer: {
		userInfo: userInfoSlice.reducer,
	},
});

/** The type representing the application's state. */
export type AppState = ReturnType<typeof AppStore.getState>;
export default AppStore;