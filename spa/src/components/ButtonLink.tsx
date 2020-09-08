import React from 'react';
import { Link, LinkProps } from 'react-router-dom';
import History from 'history';

/** Props for the {@link ButtonLink} functional component. */
export interface ButtonLinkProps<S = History.LocationState> extends LinkProps<S> {
	/** A flag indicating if the button is to be rendered as disabled. */
	disabled?: boolean;
}


/** An anchor component which renders to a button-like UI element.
 * @param {ButtonLinkProps} props The props to be used by this functional component. */
function ButtonLink(props: ButtonLinkProps) {
	const { className = '', disabled = false, ...restProps } = props;
	const extraDisabledClasses = disabled
		? 'cursor-not-allowed opacity-50'
		: '';
	return (
		<Link className={`component-ButtonLink ${className} ${extraDisabledClasses}`} {...restProps} />
	);
}


export default ButtonLink;