import { faCheck, faCheckCircle, faCopy, faEdit, faKey, faPlusCircle, faSpinner, faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon, FontAwesomeIconProps } from '@fortawesome/react-fontawesome';
import { DateTime } from 'luxon';
import React, { useContext } from 'react';
import { v4 as uuidv4 } from 'uuid';
import IdentityServerConstants from '../data/IdentityServerConstants';
import SerializableSecret from '../data/SerializableSecret';
import ButtonLinkWithIcon from './ButtonLinkWithIcon';
import CustomDatePicker from './CustomDatePicker';
import InputElement from './InputElement';



/** Props for the {@link SecretsList} functional component. */
export interface SecretsListProps
{
	context: React.Context<SecretsListContext>,
}


/** The type of the context which can be used to control the {@link SecretsList} by an external component. */
export type SecretsListContext = {
	/** The actual data of the client secrets. */
	clientSecrets: SerializableSecret[],
	/** Setter for {@link clientSecrets}. */
	setClientSecrets?: React.Dispatch<React.SetStateAction<SerializableSecret[]>>,
	/** A flag for each of the client secrets in the list, indicating if it is currently being edited. */
	isClientSecretBeingEdited: boolean[],
	/** Setter for {@link isClientSecretBeingEdited}. */
	setIsClientSecretBeingEdited?: React.Dispatch<React.SetStateAction<boolean[]>>,
	/** A value indicating the state of a "copy-to-clipboard" operation for the secret's value. */
	copyStates: ('idle' | 'processing' | 'resetting')[];
	/** Setter for {@link copyStates}. */
	setCopyStates?: React.Dispatch<React.SetStateAction<('idle' | 'processing' | 'resetting')[]>>,
};









// TODO: DOCUMENTAR SecretListEntry abaixo!!




/** Used to create a secrets list's context, which is shared between the {@link SecretsList}'s owner component,
 * the {@link SecretsList} itself, and any other children element. */
export function createSecretsListContext(defaultValue: SecretsListContext) : React.Context<SecretsListContext> {
	return React.createContext(defaultValue);
}


/** A list which is specialized in displaying and allowing the edition of {@link SerializableSecret} instances. It is necessary to create a context in the component that uses
 * the {@link SecretsList} component as a child, so that edited secret entries within the {@link SecretsList} are made available to the parent component. Contexts can be created
 * by calling {@link createSecretsListContext}. */
function SecretsList(props: SecretsListProps) {
	const {
		clientSecrets,
		setClientSecrets,
		isClientSecretBeingEdited,
		setIsClientSecretBeingEdited,
		copyStates,
		setCopyStates,
	} = useContext(props.context);


	/** Called when the user clicks the "Add another secret" button.
	 * @param {React.MouseEvent<HTMLElement, MouseEvent>} evt Event which generated the call to this handler.
	 * @param {SerializableSecret[]} newSecret The newly added secret's data. */
	function onAddSecret(evt: React.MouseEvent<HTMLElement, MouseEvent>, newSecret: SerializableSecret) {
		evt?.preventDefault();
		setClientSecrets?.(secrets => [...secrets, newSecret]);
		setIsClientSecretBeingEdited?.(secretsBeingEdited => [...secretsBeingEdited, false]);
		setCopyStates?.(copyStates => [...copyStates, "idle"]);
	}


	/** Called when the user clicks the "Remove secret" button (trash icon).
	 * @param {React.MouseEvent<HTMLElement, MouseEvent>} evt Event which generated the call to this handler.
	 * @param {number} secretIndex The index of the removed secret. */
	function onRemoveSecret(evt: React.MouseEvent<HTMLElement, MouseEvent>, secretIndex: number) {
		evt?.preventDefault();
		setClientSecrets?.(secrets => secrets.filter((_, index) => index !== secretIndex));
		setIsClientSecretBeingEdited?.(secretsBeingEdited => secretsBeingEdited.filter((_, index) => index !== secretIndex));
		setCopyStates?.(copyStates => copyStates.filter((_, index) => index !== secretIndex));
	}


	/** Utility method for updating the data of a Secret entry though a set of well-defined steps.
	 * @param targetSecretIndex The index of the Secret entry to be updated.
	 * @param produceNewData A function which takes the old Secret's data, and produces new/updated data for the Secret. */
	function updateSecretData(targetSecretIndex: number, produceNewData: (oldSecretData: SerializableSecret) => SerializableSecret) {
		setClientSecrets?.(oldSecrets => {
			if (targetSecretIndex < 0 || targetSecretIndex >= oldSecrets.length)
				return oldSecrets;

			const newSecrets = oldSecrets.map((oldSecret, oldSecretIndex) =>
				oldSecretIndex !== targetSecretIndex
					? oldSecret
					: produceNewData(oldSecret));
			return newSecrets;
		});
	}


	/** Called when the user clicks any of the "Edit Secret" (pencil icon) buttons.
	 * @param {number} targetSecretIndex The index of the secret to be edited. */
	function onEditSecretButtonClicked(targetSecretIndex: number) {
		setIsClientSecretBeingEdited?.(oldEditFlags => {
			const newEditFlags = [...oldEditFlags];
			newEditFlags[targetSecretIndex] = !newEditFlags[targetSecretIndex];
			return newEditFlags;
		});
	}


	/** Called when a property of the secret being edited is modified.
	 * @param {React.ChangeEvent<HTMLInputElement>} evt Event which generated the call to this handler.
	 * @param {number} targetSecretIndex The index of the secret which has been edited.
	 * @param {keyof SerializableSecret} targetProperty The property which was modified. */
	function onSecretPropertyChanged(evt: React.ChangeEvent<HTMLInputElement>, targetSecretIndex: number, targetProperty: keyof SerializableSecret) {
		updateSecretData(targetSecretIndex, (oldSecretData) => ({...oldSecretData, [targetProperty]: evt.target.value }));
	}


	/** Called when the "expiration" field of the secret being edited is modified.
	 * @param {number} targetSecretIndex The index of the secret which has been edited.
	 * @param {Date} date The new expiration date. */
	function onSecretExpirationChanged(targetSecretIndex: number, date: Date|null) {
		updateSecretData(targetSecretIndex, (oldSecretData) => ({...oldSecretData, expiration: date ?? undefined }));
	}


	/** Called when the user clicks the "Copy secret to clipboard" (copy icon) button.
	 * @param {number} targetSecretIndex The index of the secret which has been edited. */
	async function onCopySecretToClipboardClicked(targetSecretIndex: number) {
		// Verify if the user is currently able to click the button
		if (targetSecretIndex < 0
			|| targetSecretIndex >= clientSecrets.length
			|| !clientSecrets[targetSecretIndex].value
			|| copyStates[targetSecretIndex] !== 'idle')
			return;


		// Update button to "processing" state, and perform an async write to the clipboard
		const updateCopyStateTo = (targetState: SecretsListContext['copyStates'][0]) => setCopyStates?.(oldCopyStates => {
			const newCopyStates = [...oldCopyStates];
			newCopyStates[targetSecretIndex] = targetState;
			return newCopyStates;
		});
		updateCopyStateTo('processing');

		const clipboardPromise = navigator.clipboard.writeText(clientSecrets[targetSecretIndex].value!);
		await clipboardPromise;


		// Update the secret entry to the "resetting" state, which will display an "ok" icon to the user (indicating the data was copied to the clipboard).
		// Then, wait for some seconds so that the user can see this.
		updateCopyStateTo('resetting')
		await new Promise(resolve => setTimeout(() => resolve(null), 3000));


		// Finally, set the button's icon back to normal ("copy" icon)
		updateCopyStateTo('idle');
	}




	// Render the component
	return (
		<div className="component-SecretsList">
			<ul className="flex flex-col gap-2">
				{clientSecrets.map((secret, secretIndex) =>
					<li key={secretIndex} className="border border-gray-400 rounded-lg p-3 flex gap-4">
						<div className="flex-grow flex flex-wrap gap-1 items-center">
							<span className="font-bold w-28">Description:</span>
							<span className="flex-grow">
								{(() => {
									if (isClientSecretBeingEdited[secretIndex])
										return <InputElement value={secret.description} onChange={evt => onSecretPropertyChanged(evt, secretIndex, 'description')} />;
									else
										return secret.description
											? secret.description
											: <span className="italic">No description provided.</span>
								})()}
							</span>
							<span className="w-full"/>

							<span className="font-bold w-28">Expiration:</span>
							<span className="flex-grow">
								{(() => {
									if (isClientSecretBeingEdited[secretIndex])
									{
										let secretExpirationDate : Date | null;
										if (secret.expiration)
											secretExpirationDate = new Date(secret.expiration);
										else
											secretExpirationDate = null;
										return <CustomDatePicker selected={secretExpirationDate} onChange={date => onSecretExpirationChanged(secretIndex, date as Date|null)} isClearable placeholderText="Never expires." />
									}
									else
										return secret.expiration
											? DateTime.fromJSDate(secret.expiration).toFormat("yyyy-MM-dd")
											: <span className="italic">Never expires.</span>
								})()}
							</span>
							<span className="w-full"/>

							<span className="font-bold w-28">Value:</span>
							<span className="flex-grow">
								{
									secret.isValueHashed
										?
											<React.Fragment>
												<FontAwesomeIcon icon={faKey} className="mr-2 text-sha" />
												<span className="italic">Encrypted value (redacted).</span>
											</React.Fragment>
										: secret.value
								}
							</span>
						</div>
						<div className="border-l border-gray-400"></div>
						<div className="flex items-center gap-2">
							<div title="Copy value to clipboard"
								className={secret.isValueHashed ? "opacity-50" : "cursor-pointer"}
								onClick={() => secret.isValueHashed ? (() => {}) : onCopySecretToClipboardClicked(secretIndex)}>
									{ (() => {
										let copyIcon: FontAwesomeIconProps['icon'],
											copyIconShouldSpin: boolean;
										switch (copyStates[secretIndex])
										{
											case 'idle':
												copyIcon = faCopy;
												copyIconShouldSpin = false;
												break;
											case 'processing':
												copyIcon = faSpinner;
												copyIconShouldSpin = true;
												break;
											case 'resetting':
												copyIcon = faCheck;
												copyIconShouldSpin = false;
												break;
											default:
												throw new Error(`Unknown copy state: ${copyStates[secretIndex]}`);
										}
										return <FontAwesomeIcon icon={copyIcon} spin={copyIconShouldSpin} />;
									})()}
							</div>
							<div className="cursor-pointer" onClick={() => onEditSecretButtonClicked(secretIndex)}>
								<FontAwesomeIcon icon={isClientSecretBeingEdited[secretIndex] ? faCheckCircle : faEdit} />
							</div>
							<div className="cursor-pointer" onClick={evt => onRemoveSecret(evt, secretIndex)}>
								<FontAwesomeIcon icon={faTrash} />
							</div>
						</div>
					</li>
				)}
			</ul>
			<div className="flex justify-end mt-4">
				<ButtonLinkWithIcon icon={faPlusCircle} to='/' onClick={evt => onAddSecret(evt, {
					description: `New secret ${uuidv4()}`,
					isValueHashed: false,
					type: IdentityServerConstants.SecretTypes.SharedSecret,
					value: uuidv4(),
				})}>
					Add another secret
				</ButtonLinkWithIcon>
			</div>
		</div>
	);
}


export default SecretsList;