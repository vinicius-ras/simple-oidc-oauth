import React from 'react';
import History from 'history';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IconProp, IconDefinition } from '@fortawesome/fontawesome-svg-core';
import ButtonLink, { ButtonLinkProps } from './ButtonLink';

/** Props for the {@link ButtonLinkWithIcon} functional component. */
export interface ButtonLinkWithIconProps<S = History.LocationState> extends ButtonLinkProps<S> {
	/** The Font Awesome icon to be rendered within the button. */
	icon: IconProp|IconDefinition;
}


/** A {@link ButtonLink} component which contains text and a Font Awesome icon.
 * @param {ButtonLinkWithIconProps} props The props to be used by this functional component. */
function ButtonLinkWithIcon(props: ButtonLinkWithIconProps) {
	const { className = '', children, icon, ...restProps } = props;
	return (
		<ButtonLink className={`component-ButtonLink ${className}`} {...restProps}>
			<FontAwesomeIcon icon={icon} className="mr-2" />
			{children}
		</ButtonLink>
	);
}

export default ButtonLinkWithIcon;