import React from 'react';
import { AlertColor } from './AlertBox';
import ErrorAlert, { ErrorAlertProps } from './ErrorAlert';
import { ErrorDisplayMode } from './ErrorText';

/** Props for the {@link InputElement} functional component. */
export type InputElementProps =
	React.InputHTMLAttributes<HTMLInputElement>
	& {
		/** Props to be passed to the internal {@link AlertBox} component. */
		alertBox?: ErrorAlertProps["alertBox"];
		/** Props to be passed to the internal {@link ErrorText} component. */
		errorText?: ErrorAlertProps["errorText"];
		/** Extra classes to be applied to the container which wraps the input element. */
		containerClassName?: React.InputHTMLAttributes<HTMLDivElement>["className"];
	};


/** Represents an input element, with support to displaying errors whenever necessary. */
function InputElement(props: InputElementProps) {
	const {
		containerClassName = '',
		name,
		alertBox = {color: AlertColor.ERROR},
		errorText = name ? {displayMode: ErrorDisplayMode.ERROR_KEY, errorKey: name} : undefined,
		...remainingInputProps
	} = props;
	return (
		<div className={`component-InputElement ${containerClassName}`}>
			{(() => {
				if (name && errorText)
					return <ErrorAlert alertBox={alertBox} errorText={errorText} />
				return null;
			})()}
			<input name={name} {...remainingInputProps} />
		</div>
	);
}


export default InputElement;