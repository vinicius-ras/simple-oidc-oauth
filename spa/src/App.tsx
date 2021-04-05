import { library as FontAwesomeIconsLibrary } from '@fortawesome/fontawesome-svg-core';
import { fab } from '@fortawesome/free-brands-svg-icons';
import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import { AlertColor } from './components/AlertBox';
import ClientsManagementPage from './components/ClientsManagementPage';
import ErrorAlert from './components/ErrorAlert';
import { ErrorDisplayMode } from './components/ErrorText';
import LogoutPage from './components/LogoutPage';
import NavigationBar from './components/NavigationBar';
import RequireAppInitialization from './components/RequireAppInitialization';
import SignInListener from './components/SignInListener';
import UserCredentialsPage from './components/UserCredentialsPage';
import WelcomePage from './components/WelcomePage';
import './css/build/app.css';
import AppStore from './redux/AppStore';

FontAwesomeIconsLibrary.add(fab);

function App() {
	return (
		<Provider store={AppStore}>
			<Router>
				<div className="App">
					<div className="w-screen max-w-md md:max-w-6xl">
						<NavigationBar />
						<header className="mb-4 px-4">
							{/* A component to display application-wide errors (HTTP Status Codes from 500 to 599). */}
							<ErrorAlert alertBox={{color: AlertColor.ERROR, className: "mt-4"}} errorText={{displayMode: ErrorDisplayMode.DETAIL_OR_TITLE, statusCodes: ({minCode: 500, maxCode: 599})}} />
						</header>
						<main className="px-4">
							<RequireAppInitialization>
								<SignInListener />
								<Switch>
									{/* Public pages. */}
									<Route path="/" exact={true} component={WelcomePage} />
									<Route path="/login" component={UserCredentialsPage} />
									<Route path="/logout" component={LogoutPage} />


									{/* IdP Management Interface. */}
									<Route path="/management/clients" component={ClientsManagementPage} />

								</Switch>
							</RequireAppInitialization>
						</main>
						<footer className="mb-80" />
					</div>
				</div>
			</Router>
		</Provider>
	);
}

export default App;
