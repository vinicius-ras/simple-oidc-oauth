import React from 'react';
import { Link, LinkProps } from 'react-router-dom';
import History from 'history';

/** Props for the {@link ButtonLink} functional component. */
interface ButtonLinkProps<S = History.LocationState> extends LinkProps<S> {
	[restProps: string]: any;
}


/** An anchor component which renders to a button-like UI element.
 * @param {ButtonLinkProps} props The props to be used by this functional component. */
function ButtonLink(props: ButtonLinkProps) {
	const { className = '', ...restProps } = props;
	return (
		<Link className={`component-ButtonLink ${className}`} {...restProps} />
	);
}

export default ButtonLink;