import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { AppState } from '../redux/AppStoreCreation';
import AlertBox, { AlertBoxProps } from './AlertBox';
import ErrorText, { ErrorTextProps } from './ErrorText';


/** Props for the {@link ErrorAlert} functional component. */
export type ErrorAlertProps = {
	/** Props to be passed to the internal {@link AlertBox} component. */
	alertBox: AlertBoxProps;
	/** Props to be passed to the internal {@link ErrorText} component. */
	errorText: ErrorTextProps;
};


/** A component which wraps an {@link ErrorText} within an {@link AlertBox}, and is mainly
 * designed to display form or application-wide errors. */
function ErrorAlert(props: ErrorAlertProps) {
	const [alertBoxProps, setAlertBoxProps] = useState<AlertBoxProps>({...props.alertBox});
	const [errorTextProps] = useState<ErrorTextProps>({
		...props.errorText,
		onErrorTextMessagesChanged: newErrorText => {
			// If the ErrorText component has fired an event to inform that the error text has changed,
			// update the ErrorAlert component's visibility accordingly
			const expectedAlertBoxVisibility = (!!newErrorText && newErrorText.length > 0);
			const actualAlertBoxVisibility = alertBoxProps.isVisible ?? true;
			if (expectedAlertBoxVisibility !== actualAlertBoxVisibility)
				setAlertBoxProps(curAlertBoxProps => ({...curAlertBoxProps, isVisible: expectedAlertBoxVisibility}));
		}
	});
	const lastErrorResponse = useSelector((state: AppState) => state.errorResponse);


	// EFFECT: the alert box must be hidden whenever the interface is cleared from errors
	useEffect(() => {
		setAlertBoxProps(curAlertBoxProps => ({...curAlertBoxProps, isVisible: (!!lastErrorResponse)}));
	}, [lastErrorResponse]);


	// Render the component
	return (
		<AlertBox {...alertBoxProps}>
			<ErrorText {...errorTextProps} />
		</AlertBox>
	);
}

export default ErrorAlert;