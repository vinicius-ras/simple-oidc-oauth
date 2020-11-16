import Lodash from 'lodash';
import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { AppState } from '../redux/AppStoreCreation';


/** Identifies what is to be extracted from the returned API errors that needs to be displayed as the error's text. */
export enum ErrorDisplayMode {
	/** Displays the {@link ValidationProblemDetails.detail} field of the Problem Details response. */
	DETAIL = "DETAIL",
	/** Displays the {@link ValidationProblemDetails.title} field of the Problem Details response. */
	TITLE = "TITLE",
	/** Displays either the {@link ValidationProblemDetails.detail} field (if present) or the {@link ValidationProblemDetails.title} field. */
	DETAIL_OR_TITLE = "DETAIL_OR_TITLE",
	/** Displays either the {@link ValidationProblemDetails.title} field (if present) or the {@link ValidationProblemDetails.detail} field. */
	TITLE_OR_DETAIL = "TITLE_OR_DETAIL",
	/** Displays the error(s) associated to a given error key. */
	ERROR_KEY = "ERROR_KEY",
};


/** Represents a range of HTTP Status Codes. */
export type StatusCodesRange = {
	/** The minimum, inclusive HTTP Status Code in the range. */
	minCode: number;
	/** The maximum, inclusive HTTP Status Code in the range. */
	maxCode: number;
};


/** Base properties used by the {@link ErrorText} component. */
type BaseErrorTextProps = {
	/** One or more HTTP status codes which will trigger the displaying of this {@link ErrorText}.
	 * This can be used to render this component only when specific HTTP codes are sent by the back-end server.
	 * Acceptable values are:
	 * <ul>
	 *     <li>A single number representing the only HTTP Status Code which triggers the component's rendering.</li>
	 *     <li>A single {@link StatusCodesRange} representing the minimum and maximum HTTP Status Code values which trigger the component's rendering.</li>
	 *     <li>An array containing a mix of the previous acceptable values (numbers and/or {@link StatusCodesRange}).</li>
	 * </ul> */
	statusCodes?: number | StatusCodesRange | Array<number|StatusCodesRange>;
	/** A callback than can be specified to inform parent components whenever the collection of error text messages change. */
	onErrorTextMessagesChanged?: (newErrorTextMessages?: string[]) => void;
};


/** Complimentary properties for the {@link ErrorText} component for displaying errors that are not based on error keys. */
type ErrorTextPropsWithoutErrorKey = BaseErrorTextProps & {
	/** Specifies what needs to be extracted from the Problem Details response and
	 * displayed as the error message. */
	displayMode: Exclude<ErrorDisplayMode, ErrorDisplayMode.ERROR_KEY>;
}


/** Complimentary properties for the {@link ErrorText} component for displaying errors that are based on error keys. */
type ErrorTextPropsWithErrorKey = BaseErrorTextProps & {
	/** Specifies what needs to be extracted from the Problem Details response and
	 * displayed as the error message. */
	displayMode: ErrorDisplayMode.ERROR_KEY,
	/** The key to be extracted from the {@link ValidationProblemDetails.errors} map of errors. */
	errorKey: string,
}


/** Props for the {@link ErrorText} functional component. */
export type ErrorTextProps = (ErrorTextPropsWithoutErrorKey|ErrorTextPropsWithErrorKey);



/** Displays error messages which are stored in the Redux Store. */
function ErrorText(props: ErrorTextProps) {
	const lastErrorResponse = useSelector((state: AppState) => state.errorResponse);
	const [lastErrorMessages, setLastErrorMessages] = useState<string[]|null|undefined>();
	const {
		displayMode,
		statusCodes,
		onErrorTextMessagesChanged,
	} = props;

	const errorKey = (props.displayMode === ErrorDisplayMode.ERROR_KEY)
		? props.errorKey
		: null;


	// EFFECT: whenever the error text changes, inform the parent (if there's a callback defined via props for that)
	useEffect(() => {
		onErrorTextMessagesChanged?.(lastErrorMessages ?? undefined);
	}, [lastErrorMessages, onErrorTextMessagesChanged]);


	// Verify which error text to display (if any)
	const pushIfPresent = (array: string[], value?: string|string[]) => {
		if (!value)
			return;
		else if (Array.isArray(value))
			value.forEach(elem => array.push(elem));
		else
			array.push(value);
	};


	let errorMessagesToDisplay: string[] = [];
	switch (displayMode) {
		case ErrorDisplayMode.DETAIL:
			pushIfPresent(errorMessagesToDisplay, lastErrorResponse?.detail);
			break;
		case ErrorDisplayMode.TITLE:
			pushIfPresent(errorMessagesToDisplay, lastErrorResponse?.title);
			break;
		case ErrorDisplayMode.DETAIL_OR_TITLE:
			pushIfPresent(errorMessagesToDisplay, lastErrorResponse?.detail ?? lastErrorResponse?.title);
			break;
		case ErrorDisplayMode.TITLE_OR_DETAIL:
			pushIfPresent(errorMessagesToDisplay, lastErrorResponse?.title ?? lastErrorResponse?.detail);
			break;
		case ErrorDisplayMode.ERROR_KEY:
			pushIfPresent(errorMessagesToDisplay, lastErrorResponse?.errors?.[errorKey!]);
			break;
		default:
			throw new Error(`Cannot display error of type "${displayMode}": not implemented.`);
	}


	// Verify if the last HTTP Response's Status Code one of the required values for displaying the error
	if (statusCodes) {
		// If specific status codes are required to render this component, then it won't be rendered if no status codes have been returned
		if (!(lastErrorResponse?.status))
			errorMessagesToDisplay = [];
		else {
			/** Takes numbers and {@link StatusCodesRange} objects and normalizes them to the form of {@link StatusCodesRange} objects
			 * for further processing.
			 * @param {number|StatusCodesRange} value The value to be normalized for further processing.
			 * @returns {StatusCodesRange}
			 *     If the input value was a number, outputs a {@link StatusCodesRange} object whose minimum and maximum status code
			 *     values are equal to the input number.
			 *     If the input value is already a {@link StatusCodesRange} object, that same object is returned straight away. */
			const convertToStatusCodesRange = (value: number|StatusCodesRange) : StatusCodesRange => {
				if (typeof(value) === "number")
					return {minCode: value, maxCode: value};
				return value;
			}


			// Transform the user-defined status codes in an array of ranges [minStatusCode, maxStatusCode]
			let statusCodesRanges: StatusCodesRange[];
			if (typeof(statusCodes) === "number")
				statusCodesRanges = [convertToStatusCodesRange(statusCodes)];
			else if (Array.isArray(statusCodes))
				statusCodesRanges = statusCodes.map(elem => convertToStatusCodesRange(elem));
			else
				statusCodesRanges = [statusCodes];

			// Sanity checking: are the ranges specified correctly?
			statusCodesRanges.forEach(range => {
				if (range.minCode > range.maxCode)
					throw new Error(`Invalid range [${range.minCode}-${range.maxCode}] defined for ${ErrorText.name} component.`);
			});

			// Verify if the last response's HTTP Status Code is within one of the valid range to display the error message
			const isLastResponseStatusCodeValid = statusCodesRanges.some(range => lastErrorResponse?.status >= range.minCode && lastErrorResponse?.status <= range.maxCode);
			if (isLastResponseStatusCodeValid === false)
				errorMessagesToDisplay = [];
		}
	}


	// Update text's data when necessary, and render the component
	if (Lodash.isEqual(errorMessagesToDisplay, lastErrorMessages) === false)
		setLastErrorMessages(errorMessagesToDisplay);

	if (!errorMessagesToDisplay || errorMessagesToDisplay.length === 0)
		return null;

	return (
		<span className="component-ErrorText">
			{(() => {
				if (errorMessagesToDisplay.length === 1)
					return errorMessagesToDisplay[0];
				return (
					<ul className="list-disc list-inside">
						{errorMessagesToDisplay.map((message, index) => <li key={`MSG_${index}`}>{message}</li>)}
					</ul>
				)
			})()}
		</span>
	);
}


export default ErrorText;