import React from 'react';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import UserCredentials from './components/UserCredentials';
import WelcomePage from './components/WelcomePage';
import { library as FontAwesomeIconsLibrary } from '@fortawesome/fontawesome-svg-core';
import './css/build/app.css';
import { fab } from '@fortawesome/free-brands-svg-icons';

FontAwesomeIconsLibrary.add(fab);

function App() {
  return (
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
                <Route path="/login" component={UserCredentials} />
              </Switch>
          </main>
          <footer></footer>
        </div>
    </Router>
  );
}

export default App;
