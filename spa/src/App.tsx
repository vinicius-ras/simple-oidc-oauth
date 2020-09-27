import { library as FontAwesomeIconsLibrary } from '@fortawesome/fontawesome-svg-core';
import { fab } from '@fortawesome/free-brands-svg-icons';
import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter as Router, Link, Route, Switch } from 'react-router-dom';
import MultipleTabsSignInListener from './components/MultipleTabsSignInListener';
import UserCredentialsPage from './components/UserCredentialsPage';
import WelcomePage from './components/WelcomePage';
import './css/build/app.css';
import AppStore from './redux/AppStore';

FontAwesomeIconsLibrary.add(fab);

function App() {
	return (
		<Provider store={AppStore}>
			<MultipleTabsSignInListener />
			<Router>
				<div className="App">
					<header className="mb-4">
						<Link to="/">
							<div>OAuth 2.0 + OpenID Connect Provider</div>
							<div className="subtext">By Vinicius R. A. Silva</div>
						</Link>
					</header>
					<main>
						<Switch>
							<Route path="/" exact={true} component={WelcomePage} />
							<Route path="/login" component={UserCredentialsPage} />
						</Switch>
					</main>
					<footer></footer>
				</div>
			</Router>
		</Provider>
	);
}

export default App;
