import React from 'react';
import ButtonLink from './ButtonLink';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

/** Props for the {@link UserCredentials} functional component. */
interface UserCredentialsProps
{
}


/** A component which allows users to enter their credentials in order to perform a login,
 * or to register themselves. */
function UserCredentials(props: UserCredentialsProps) {
	return (
		<div className="component-UserCredentials">
			<div className="flex flex-col">
				<input type="email" placeholder="E-mail" className="border border-gray-500 rounded-lg p-2" />
				<input type="password" placeholder="Password" className="border border-gray-500 rounded-lg p-2 mt-2" />
			</div>
			<div className="flex flex-row items-center my-2">
				<hr className="flex-grow" />
				<span className="mx-2">OR</span>
				<hr className="flex-grow" />
			</div>
			<div className="flex flex-col">
				<ButtonLink to="/" className="my-2">
					<FontAwesomeIcon icon={['fab', 'facebook-square']} size="lg" className="mr-2" />
					<span>Login with Facebook</span>
				</ButtonLink>
				<ButtonLink to="/">
					<FontAwesomeIcon icon={['fab', 'google']} size="lg" className="mr-2" />
					<span>Login with Google</span>
				</ButtonLink>
			</div>
		</div>
	);
}

export default UserCredentials;