import createAppStore from "./AppStoreCreation";

/** The Redux Store instance used by the application, when the app is running
 * in the user's web browser. */
const AppStore = createAppStore();
export default AppStore;