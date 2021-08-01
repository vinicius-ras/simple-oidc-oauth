import { faCheck, faCopy, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon, FontAwesomeIconProps } from '@fortawesome/react-fontawesome';
import React, { useState } from 'react';
import toast from 'react-hot-toast';

/** Props for the {@link CopyToClipboardButton} functional component. */
export type CopyToClipboardButtonProps = {
	/** Extra class names for the component. */
	className?: string
	/** The contents which will be copied when the user clicks the button. */
	contentsToCopy: string;
	/** An optional text to be displayed after the copy to clipboard has been performed. */
	copySuccessToast?: string;
	/** An optional title for the HTML element. */
	title?: string;
}


/** Indicates the possible states the {@link CopyToClipboardButton} can assume. */
type CopyButtonState = 'idle' | 'copying' | 'resetting';


/** A simple reusable button which can be clicked to copy contents to the user's clipboard. */
function CopyToClipboardButton(props: CopyToClipboardButtonProps) {
	const [buttonState, setButtonState] = useState<CopyButtonState>('idle');

	const {
		className: extraClassNames = '',
		contentsToCopy,
		copySuccessToast,
		title = 'Copy to clipboard'
	} = props;


	/** The time it takes for the component to reset back to the "idle" state after a successful copy-to-clipboard operation. */
	const timeToResetToIdleMs = 3000;


	/** Called to actually perform the copy-to-clipboard operation. */
	async function performCopy()
	{
		// Perform the copy
		setButtonState('copying');
		const clipboardPromise = navigator.clipboard.writeText(contentsToCopy);
		await clipboardPromise;


		// Update the secret entry to the "resetting" state, which will display an "ok" icon to the user (indicating the data was copied to the clipboard).
		// Then, wait for some seconds so that the user can see this.
		setButtonState('resetting');
		if (copySuccessToast)
			toast.success(copySuccessToast);
		await new Promise(resolve => setTimeout(() => resolve(null), timeToResetToIdleMs));


		// Finally, set the button's icon back to normal ("copy" icon)
		setButtonState('idle');
	}


	// Decide how the button's icon should look like
	let copyIcon: FontAwesomeIconProps['icon'],
		copyIconShouldSpin: boolean;
	switch (buttonState)
	{
		case 'idle':
			copyIcon = faCopy;
			copyIconShouldSpin = false;
			break;
		case 'copying':
			copyIcon = faSpinner;
			copyIconShouldSpin = true;
			break;
		case 'resetting':
			copyIcon = faCheck;
			copyIconShouldSpin = false;
			break;
		default:
			throw new Error(`Unknown copy state: ${buttonState}`);
	}

	// Render the component
	return (
		<FontAwesomeIcon className={`component-CopyToClipboardButton ${extraClassNames}`} icon={copyIcon} spin={copyIconShouldSpin} onClick={() => performCopy()} title={title} />
	);
}

export default CopyToClipboardButton;