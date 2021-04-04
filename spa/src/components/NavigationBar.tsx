import { faBars, faBoxes, faCoins, faSignOutAlt, faUser, faUsers } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon, FontAwesomeIconProps } from '@fortawesome/react-fontawesome';
import React, { useState } from 'react';
import { useSelector } from 'react-redux';
import { Link } from 'react-router-dom';
import AuthServerClaimTypes from '../data/AuthServerClaimTypes';
import SerializableClaim from '../data/SerializableClaim';
import { AppState } from '../redux/AppStoreCreation';
import ButtonLinkWithIcon, { ButtonLinkWithIconProps } from './ButtonLinkWithIcon';

/** Props for the {@link NavigationBar} functional component. */
export interface NavigationBarProps
{
}


/** The application's navigation bar, displayed when the user has access to the IdP Management Interface. */
function NavigationBar(props: NavigationBarProps) {
	const userInfo = useSelector((state: AppState) => state.userInfo);
	const [isExpanded, setExpanded] = useState(false);

	// Initialize data which will be used to render the navigation bar
	const clientManagementClaims = [AuthServerClaimTypes.CanViewClients, AuthServerClaimTypes.CanViewAndEditUsers],
		userManagementClaims = [AuthServerClaimTypes.CanViewUsers, AuthServerClaimTypes.CanViewAndEditUsers],
		resourceManagementClaims = [AuthServerClaimTypes.CanViewResources, AuthServerClaimTypes.CanViewAndEditResources];

	const userHasAnyClaims = (userClaims: (SerializableClaim[] | undefined), targetClaims: AuthServerClaimTypes[]) => !!(userClaims ?? []).find(claim => targetClaims.includes(claim.type as AuthServerClaimTypes));
	const hasAnyClientManagementClaims = userHasAnyClaims(userInfo?.claims, clientManagementClaims),
		hasAnyUserManagementClaims = userHasAnyClaims(userInfo?.claims, userManagementClaims),
		hasAnyResourceManagementClaims = userHasAnyClaims(userInfo?.claims, resourceManagementClaims);

	// Renders the navigation bar
	const renderNavigationItem = (condition: boolean, icon: FontAwesomeIconProps["icon"], text: JSX.Element | string, targetLocation: ButtonLinkWithIconProps["to"]) => {
		if (!condition)
			return null;
		return (
			<ButtonLinkWithIcon to={targetLocation} icon={icon} className="block mb-1 rounded-none bg-blue-600 hover:bg-blue-400" onClick={() => { setExpanded(false) }}>
				<span className="ml-2">{text}</span>
			</ButtonLinkWithIcon>
		);
	};
	return (
		<nav className="component-NavigationBar bg-blue-600 flex text-white h-12 shadow-lg">
			{(() => {
				if (userInfo)
					return (
						<button className="w-8 h-8 self-center focus:outline-none" onClick={() => setExpanded(expanded => !expanded)} title="Menu">
							<FontAwesomeIcon icon={faBars} />
						</button>
					)
				return null;
			})()}
			<Link to="/" className="ml-4">
				<div>OAuth 2.0 + OpenID Connect Provider</div>
				<div className="subtext">By Vinicius R. A. Silva</div>
			</Link>
			<div className="absolute mt-12 bg-blue-600 rounded-b-xl pb-1 shadow-2xl">
				{
					userInfo && isExpanded
					?
						<React.Fragment>
							{renderNavigationItem(hasAnyUserManagementClaims, faUsers, "Users", "/management/users")}
							{renderNavigationItem(hasAnyClientManagementClaims, faBoxes, "Clients", "/management/clients")}
							{renderNavigationItem(hasAnyResourceManagementClaims, faCoins, "Resources", "/management/resources")}
							<hr />
							{renderNavigationItem(true, faUser, <React.Fragment><b>Current user:</b> {userInfo.name}</React.Fragment>, "/user-profile")}
							{renderNavigationItem(true, faSignOutAlt, "Log out", "/logout")}
						</React.Fragment>
					: null
				}
			</div>
		</nav>
	);
}

export default NavigationBar;