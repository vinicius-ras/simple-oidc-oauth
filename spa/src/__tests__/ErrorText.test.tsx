import { cleanup, render, screen } from '@testing-library/react';
import React from 'react';
import { Provider } from 'react-redux';
import ErrorText, { ErrorDisplayMode, ErrorTextProps, StatusCodesRange } from '../components/ErrorText';
import createAppStore from '../redux/AppStoreCreation';
import errorResponseSlice, { ValidationProblemDetails } from '../redux/slices/errorResponseSlice';



/** Converts objects that can be received in the {@link ErrorTextProps.statusCodes} property to
 * human-readable string representations.
 * @param {ErrorTextProps["statusCodes"]} statusCodeValue The value to be converted to a human-readable string.
 * @return {string} Returns a human-readable representation of the given status code(s) value. */
const convertStatusCodesToString = (statusCodeValue: ErrorTextProps["statusCodes"]) : string => {
	if (!statusCodeValue)
		return "<EMPTY>";
	else if (typeof(statusCodeValue) === "number")
		return statusCodeValue.toString();
	else if (Array.isArray(statusCodeValue)) {
		const arrayElementsStr = statusCodeValue.map(elem => convertStatusCodesToString(elem))
			.join(", ");
		return `[${arrayElementsStr}]`;
	}

	return `{${statusCodeValue.minCode}-${statusCodeValue.maxCode}}`;
};



/** Compares two status codes for sorting purposes.
 * @param s1 The first status code(s) descriptor to be compared.
 * @param s2 The second status code(s) descriptor to be compared.
 * @return
 *     Returns a negative number if argument "e1" should appear before argument "e2".
 *     Returns a positive number if argument "e1" should appear after argument "e2".
 *     Returns zero if arguments "e1" and "e2" are to be considered equal during sorting procedures. */
const compareStatusCodes = (s1: ErrorTextProps["statusCodes"], s2: ErrorTextProps["statusCodes"]): number => {
	/** Compares two {@link StatusCodesRange} objects for sorting purposes.
	 * @param s1 The first {@link StatusCodesRange} to be compared.
	 * @param s2 The second {@link StatusCodesRange} to be compared.
	 * @return
	 *     Returns a negative number if argument "s1" should appear before argument "s2".
	 *     Returns a positive number if argument "s1" should appear after argument "s2".
	 *     Returns zero if arguments "s1" and "s2" are to be considered equal during sorting procedures. */
	const compareStatusCodesRange = (s1: StatusCodesRange, s2: StatusCodesRange) => {
		return (s1.minCode !== s2.minCode)
			? (s1.minCode - s2.minCode)
			: (s1.maxCode !== s2.maxCode)
				? (s1.maxCode - s2.maxCode)
				: 0;
	};


	/** Compares two status codes Array objects for sorting purposes.
	 * @param arr1 The first Array to be compared.
	 * @param arr2 The second Array to be compared.
	 * @return
	 *     Returns a negative number if argument "arr1" should appear before argument "arr2".
	 *     Returns a positive number if argument "arr1" should appear after argument "arr2".
	 *     Returns zero if arguments "arr1" and "arr2" are to be considered equal during sorting procedures. */
	const compareStatusCodesArray = (arr1: (number | StatusCodesRange)[], arr2: (number | StatusCodesRange)[]) : number => {
		for (let c = 0; c < arr1.length && c < arr2.length; c++) {
			const elementComparisonResult = compareStatusCodes(arr1[c], arr2[c]);
			if (elementComparisonResult !== 0)
				return elementComparisonResult;
		}
		return (arr1.length - arr2.length);
	}


	// Perform the comparisons between possible values
	if (s1 === undefined)
		return (s2 === undefined)
			? 0
			: -1;
	else if (typeof(s1) === "number")
		return (s2 === undefined)
			? 1
			: (typeof(s2) === "number")
				? (s1 - s2)
				: -1;
	else if (Array.isArray(s1))
		return (s2 === undefined || typeof(s2) === "number")
			? 1
			: (Array.isArray(s2))
				? compareStatusCodesArray(s1, s2)
				: -1;
	else
		return (s2 === undefined || typeof(s2) === "number" || Array.isArray(s2))
			? 1
			: compareStatusCodesRange(s1, s2);
};



afterEach(() => { cleanup() });


describe('displays the right error texts', () => {
	// Prepare test data
	type ValidationProblemDetailsFieldsToTest = (keyof Omit<ValidationProblemDetails, "status">);
	const testFieldsValidationProblemDetails: ValidationProblemDetailsFieldsToTest[] = [
		"detail",
		"errors",
		"instance",
		"title",
		"type",
	];
	const testStatusCodeNumbers = [400, 500, 200];
	const errorKeyWithMultipleErrors = "errorKey3_multipleErrors";
	const errorKeyWithoutErrors = "errorKey4_noErrors";
	const testErrorKeys = [undefined, "errorKey1", "errorKey2", errorKeyWithMultipleErrors, errorKeyWithoutErrors];



	// Generate the test status codes combinations which will be used in the tests
	testStatusCodeNumbers.sort()

	const testStatusCodes: ErrorTextProps["statusCodes"][] = [];
	for (let c1 = 0; c1 < testStatusCodeNumbers.length; c1++) {
		const code1 = testStatusCodeNumbers[c1];
		testStatusCodes.push(code1);

		for (let c2 = c1; c2 < testStatusCodeNumbers.length; c2++) {
			const code2 = testStatusCodeNumbers[c2];
			testStatusCodes.push({minCode: code1, maxCode: code2});
			testStatusCodes.push([code1, code2]);
		}
	}
	testStatusCodes.push([{minCode: 200, maxCode: 400}, 500]);
	testStatusCodes.push([500, {minCode: 200, maxCode: 400}]);

	testStatusCodes.sort((e1, e2) => compareStatusCodes(e1, e2));
	testStatusCodes.unshift(undefined);



	// Generate ValidationProblemDetails to be used for the tests
	const testValidationProblemDetailsInstances: ValidationProblemDetails[] = [];
	const totalInstancesToGenerate = Math.trunc(Math.pow(2, testFieldsValidationProblemDetails.length));
	for (let caseNumber = 0; caseNumber < totalInstancesToGenerate; caseNumber++) {
		// Basic test instance
		const validationProblemDetailsInstance: ValidationProblemDetails = {
			status: 0,
			detail: "test detail",
			instance: "test instance",
			title: "test title",
			type: "test type",
			errors: {}
		};

		// Generate some mock errors for the ValidationProblemDetails instance, as necessary
		for (const errorKey of testErrorKeys) {
			if (errorKey) {
				const numberOfErrors = (errorKey === errorKeyWithoutErrors)
					? 0
					: (errorKey === errorKeyWithMultipleErrors)
						? 3
						: 1;
				const errorsArray: string[] = [];
				for (let errorNumber = 1; errorNumber <= numberOfErrors; errorNumber++) {
					errorsArray.push(`Error #${errorNumber} for error key "${errorKey}"`);
				}
				if (errorsArray.length > 0)
					validationProblemDetailsInstance.errors![errorKey] = errorsArray;
			}
		}

		// Supress one or more fields from the generated ValidationProblemDetails instance, based on the current "caseNumber"
		const caseNumberBinaryStr = caseNumber.toString(2)
			.padStart(testFieldsValidationProblemDetails.length, '0');
		for (let c = 0; c < caseNumberBinaryStr.length; c++) {
			if (caseNumberBinaryStr[c] === "1") {
				const fieldName = testFieldsValidationProblemDetails[c];
				delete validationProblemDetailsInstance[fieldName];
			}
		}

		// Add to the collection of tests
		testValidationProblemDetailsInstances.push(validationProblemDetailsInstance);
	}


	// Generate test scenarios
	for (const displayModeStr in ErrorDisplayMode) {
		// Select a function to be used to extract the resulting text, based on the DISPLAY MODE
		const displayMode = displayModeStr as ErrorDisplayMode;
		let extractTextFunction: (data: ValidationProblemDetails, errorKey?: string) => (string[]|undefined);
		switch (displayMode) {
			case ErrorDisplayMode.DETAIL:
				extractTextFunction = data => data.detail ? [data.detail] : undefined;
				break;
			case ErrorDisplayMode.TITLE:
				extractTextFunction = data => data.title ? [data.title] : undefined;
				break;
			case ErrorDisplayMode.DETAIL_OR_TITLE:
				extractTextFunction = data => data.detail
					? [data.detail]
					: data.title
						? [data.title]
						: undefined;
				break;
			case ErrorDisplayMode.TITLE_OR_DETAIL:
				extractTextFunction = data => data.title
				? [data.title]
				: data.detail
					? [data.detail]
					: undefined;
				break;
			case ErrorDisplayMode.ERROR_KEY:
				extractTextFunction = (data, errorKey) => data.errors?.[errorKey!];
				break;
			default:
				throw new Error(`Test has not been implemented for display mode "${displayMode}".`);
		}



		for (const errorKey of testErrorKeys) {
			// SCENARIO EXCLUSION: error keys are NOT TO BE USED for display modes other than "ErrorDisplayMode.ERROR_KEY"
			if (displayMode !== ErrorDisplayMode.ERROR_KEY && errorKey)
				continue;

			// SCENARIO EXCLUSION: error keys are REQUIRED if the display mode is set to "ErrorDisplayMode.ERROR_KEY"
			if (displayMode === ErrorDisplayMode.ERROR_KEY && !errorKey)
				continue;



			for (const statusCode of testStatusCodes) {
				// Perform the test
				test(`TestCase(displayMode=${displayMode}, statusCode=${convertStatusCodesToString(statusCode)}, errorKey=${errorKey ?? '<EMPTY>'})`, () => {
					// For all test instances of ValidationProblemDetails objects that have been generated for the tests..
					for (const validationProblemDetails of testValidationProblemDetailsInstances) {
						// Go through all status code numbers that have been selected for our tests...
						for (const validationProblemDetailsStatusCode of testStatusCodeNumbers) {
							// Update current problem detail's status code before the test is performed
							validationProblemDetails.status = validationProblemDetailsStatusCode;

							// Render the component as appropriate
							const reduxStore = createAppStore();
							const renderResults = render(
								<Provider store={reduxStore}>
									{(() => {
										if (displayMode !== ErrorDisplayMode.ERROR_KEY)
											return <ErrorText displayMode={displayMode} statusCodes={statusCode} />
										return <ErrorText displayMode={displayMode} statusCodes={statusCode} errorKey={errorKey!} />
									})()}
								</Provider>
							);

							// Initially, no messages are expected to be displayed
							expect(document.body.textContent).toHaveLength(0);

							// Dispatch an action to set the current ValidationProblemDetails errors
							reduxStore.dispatch(errorResponseSlice.actions.setError(validationProblemDetails));

							// Retrieve the expected error messages and check if they have been rendered accordingly
							let expectedErrorMessages = (displayMode !== ErrorDisplayMode.ERROR_KEY)
								? extractTextFunction(validationProblemDetails)
								: extractTextFunction(validationProblemDetails, errorKey);


							// If the current test has a status code descriptor, let's check if the ValidationProblemDetails have a status code
							// which matches that descriptor
							if (statusCode) {
								/** Checks a numeric status code against a status code descriptor (as passed to the {@link ErrorTextProps.statusCodes} field)
								 * to verify if the given numeric status code matches the given descriptor.
								 * @param numericStatusCode The numeric status code to be checked against the descriptor.
								 * @param statusCodesDescriptor
								 *     The status codes descriptor, describing a specific numeric status code, a range of status codes, or
								 *     an array combining a set of possibilities from the aforementioned types.
								 * @returns Returns a flag indicating if the given numeric status code matches against the specified descriptor. */
								const statusCodeMatches = (numericStatusCode: number, statusCodesDescriptor: ErrorTextProps["statusCodes"]): boolean => {
									if (statusCodesDescriptor === undefined)
										return true;
									else if (typeof(statusCodesDescriptor) === "number") {
										return (numericStatusCode === statusCodesDescriptor);
									} else if (Array.isArray(statusCodesDescriptor)) {
										return statusCodesDescriptor.some(subDescriptor => statusCodeMatches(numericStatusCode, subDescriptor));
									} else {
										return (numericStatusCode >= statusCodesDescriptor.minCode && numericStatusCode <= statusCodesDescriptor.maxCode);
									}
								}

								// If the current ValidationProblemDetails test instance's status code doesn't match the current test's status code descriptor,
								// then the tested component is expected to display NO MESSAGES.
								if (statusCodeMatches(validationProblemDetails.status, statusCode) === false)
									expectedErrorMessages = [];
							}


							// Check if the expected error messages have been rendered
							if (expectedErrorMessages && expectedErrorMessages.length > 0) {
								// All expected messages should be rendered exactly once
								expectedErrorMessages.forEach(errorMsg => expect(screen.queryAllByText(errorMsg)).toHaveLength(1));
							} else {
								// No expected error messages? Then the rendered result should be empty.
								expect(document.body.textContent).toHaveLength(0);
							}


							// Reset rendered commponents
							cleanup();
						}
					}
				});
			}
		}
	}
});
