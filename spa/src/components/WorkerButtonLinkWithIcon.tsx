import React from 'react';
import History from 'history';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IconProp, IconDefinition } from '@fortawesome/fontawesome-svg-core';
import ButtonLink from './ButtonLink';
import { ButtonLinkWithIconProps } from './ButtonLinkWithIcon';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';


/** Props for the {@link WorkerButtonLinkWithIcon} functional component. */
interface WorkerButtonLinkWithIconProps<S = History.LocationState> extends ButtonLinkWithIconProps<S> {
	/** A flag indicating if the button needs to display its "busy/working" icon. */
	isBusy?: boolean;
	/** The classes to be applied to the button once it is in the "busy" state. */
	busyClassName?: string;
}


/** A {@link ButtonLink} to be used for operations that might take a long time to complete.
 * The {@link WorkerButtonLinkWithIconProps#isBusy} prop can be used to activate this component's "busy" state, in which
 * it displays a spinning icon for the user as a visual cue that it is working on some background/async operation.
 * @param {WorkerButtonLinkWithIconProps} props The props to be used by this functional component. */
function WorkerButtonLinkWithIcon(props: WorkerButtonLinkWithIconProps) {
	const {
		children,
		className = '',
		isBusy = false,
		busyClassName = 'bg-gray-500 cursor-not-allowed',
		icon,
		...restProps
	} = props;

	let actualIcon : (IconDefinition| IconProp);
	if (isBusy)
		actualIcon = faSpinner;
	else
		actualIcon = icon;

	let extraBusyClassName = isBusy ? busyClassName : '';

	return (
		<ButtonLink className={`component-ButtonLink ${className} ${extraBusyClassName}`} {...restProps}>
			<FontAwesomeIcon icon={actualIcon} className="mr-2" spin={isBusy} />
			{children}
		</ButtonLink>
	);
}

export default WorkerButtonLinkWithIcon;