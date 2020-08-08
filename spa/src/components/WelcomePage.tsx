import { faSignInAlt, faUserPlus } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";
import ButtonLink from "./ButtonLink";

/** The application's home page.
 *
 * This is the page opened whenever the user accesses the root URL of the application. */
function WelcomePage() {
	return (
		<div className="welcome-page">
			<header>
				<div>OAuth 2.0 + OpenID Connect Provider</div>
				<div className="subtext">By Vinicius R. A. Silva</div>
			</header>
			<main className="mt-8 text-sm">
				<div>You are currently not logged in.</div>
				<div className="flex flex-row content-between mt-2">
					<ButtonLink href="/" className="mr-2 flex-grow">
						<FontAwesomeIcon icon={faSignInAlt} className="mr-2" />
						<span>Log in</span>
					</ButtonLink>
					<ButtonLink href="/" className="flex-grow">
						<FontAwesomeIcon icon={faUserPlus} className="mr-2"/>
						<span>Sign up</span>
					</ButtonLink>
				</div>
			</main>
			<footer>
			</footer>
		</div>
	);
}

export default WelcomePage;