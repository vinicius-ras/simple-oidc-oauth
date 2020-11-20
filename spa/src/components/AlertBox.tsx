import React from 'react';

/** Available color modes to be used for the {@link AlertBox} component. */
export enum AlertColor {
	INFO = "INFO",
	WARNING = "WARNING",
	ERROR = "ERROR",
	SUCCESS = "SUCCESS",
}


/** Props for the {@link AlertBox} functional component. */
export interface AlertBoxProps {
	/** Children element(s) to be rendered. */
	children?: React.DetailedHTMLProps<React.HTMLAttributes<HTMLDivElement>, HTMLDivElement>["children"];
	/** Extra classes to be included for the component. */
	className?: string;
	/** The color to be used as the alert's background.
	 * For using a custom color, leave this prop unspecified, and
	 * apply the necessary background/foreground/border color classes. */
	color?: AlertColor;
	/** Flag to specify if the component should be rendered or not.
	 * @defaultValue true */
	isVisible?: boolean;
}


/** A colored box that might be used to draw alert messages. */
function AlertBox(props: AlertBoxProps) {
	const {
		children=null,
		className='',
		isVisible=true,
	} = props;

	if (!isVisible)
		return null;

	const colorClass = (props.color ?? '').toLowerCase();
	return (
		<div className={`component-AlertBox ${colorClass} ${className}`}>
			{children}
		</div>
	);
}


export default AlertBox;