import { faSignInAlt, faUserPlus } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";
import ButtonLink from "./ButtonLink";

/** The application's home page.
 *
 * This is the page opened whenever the user accesses the root URL of the application. */
function WelcomePage() {
	return (
		<div className="component-WelcomePage text-sm">
			<div>You are currently not logged in.</div>
			<div className="flex flex-row content-between mt-2">
				<ButtonLink to="/login" className="mr-2 flex-grow">
					<FontAwesomeIcon icon={faSignInAlt} className="mr-2" />
					<span>Log in</span>
				</ButtonLink>
				<ButtonLink to="/login" className="flex-grow">
					<FontAwesomeIcon icon={faUserPlus} className="mr-2"/>
					<span>Sign up</span>
				</ButtonLink>
			</div>
		</div>
	);
}

export default WelcomePage;