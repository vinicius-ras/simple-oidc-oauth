import React from 'react';

/** Props for the {@link ButtonLink} functional component. */
interface ButtonLinkProps {
	href: string | undefined;
	children: Array<JSX.Element|string> | JSX.Element | string;
	[restProps: string]: any;
}


/** An anchor component which renders to a button-like UI element.
 * @param {ButtonLinkProps} props The props to be used by this functional component. */
function ButtonLink(props: ButtonLinkProps) {
	const { children, href, className, ...restProps } = props;
	return (
		<a className={`component-ButtonLink ${className}`} href={href} {...restProps}>
			{children}
		</a>
	);
}

export default ButtonLink;