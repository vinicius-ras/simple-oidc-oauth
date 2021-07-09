import { faCheck, faCheckCircle, faCopy, faEdit, faKey, faPlusCircle, faSpinner, faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon, FontAwesomeIconProps } from '@fortawesome/react-fontawesome';
import { DateTime } from 'luxon';
import React, { useEffect, useState } from 'react';
import { v4 as uuidv4 } from 'uuid';
import IdentityServerConstants from '../data/IdentityServerConstants';
import SerializableSecret from '../data/SerializableSecret';
import ButtonLinkWithIcon from './ButtonLinkWithIcon';
import CustomDatePicker from './CustomDatePicker';
import InputElement from './InputElement';



/** A type which extends Serializable Secrets with data to allow edition of the secrets' data. */
export type EditableSerializableSecret = SerializableSecret & {
	/** A flag which indicates if an item is in "editing-mode" (true) or in view-only mode (false). */
	isEditing: boolean;
	/** A value indicating the state of a "copy-to-clipboard" operation for the secret's value. */
	copyState: 'idle' | 'processing' | 'resetting';
}



/** Props for the {@link SecretsList} functional component. */
export interface SecretsListProps
{
	/** The list of secrets that are being displayed by this component. */
	secrets: SerializableSecret[],
	/** Called when the user adds or removes secrets to or from the list. */
	onChange: (newSecrets: SerializableSecret[]) => void,
}


/** A list which is specialized in displaying and allowing the edition of {@link SerializableSecret} instances. */
function SecretsList(props: SecretsListProps) {
	const [internalSecrets, setInternalSecrets] = useState<EditableSerializableSecret[]>([]);


	// EFFECT: whenever the externally-provided secrets change, update the internally-kept secrets
	useEffect(() => {
		const newInternalSecrets: EditableSerializableSecret[] = props.secrets.map(secret => ({
			...secret,
			isEditing: false,
			copyState: 'idle',
		}));
		setInternalSecrets(newInternalSecrets);
	}, [props.secrets]);


	/** Called when the user clicks the "Add another secret" or "Remove secret" (trash icon) buttons.
	 * @param {React.MouseEvent<HTMLElement, MouseEvent>} evt Event which generated the call to this handler.
	 * @param {SerializableSecret[]} newSecrets The new list of secrets to be used. */
	function onAddOrRemoveSecret(evt: React.MouseEvent<HTMLElement, MouseEvent>, newSecrets: SerializableSecret[]) {
		evt?.preventDefault();
		props.onChange(newSecrets);
	}


	/** Utility method for updating the data of a Secret entry though a set of well-defined steps.
	 * @param targetSecretIndex The index of the Secret entry to be updated.
	 * @param produceNewData A function which takes the old Secret's data, and produces new/updated data for the Secret. */
	function updateSecretData(targetSecretIndex: number, produceNewData: (oldSecretData: EditableSerializableSecret) => EditableSerializableSecret) {
		setInternalSecrets(oldSecrets => {
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
		updateSecretData(targetSecretIndex, (oldSecretData) => ({...oldSecretData, isEditing: !oldSecretData.isEditing }));
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
			|| targetSecretIndex >= internalSecrets.length
			|| !internalSecrets[targetSecretIndex].value
			|| internalSecrets[targetSecretIndex].copyState !== 'idle')
			return;


		// Update button to "processing" state, and perform an async write to the clipboard
		updateSecretData(targetSecretIndex, (oldSecretData) => ({
			...oldSecretData,
			copyState: 'processing' as EditableSerializableSecret['copyState']
		}));

		const clipboardPromise = navigator.clipboard.writeText(internalSecrets[targetSecretIndex].value!);
		await clipboardPromise;


		// Update the secret entry to the "resetting" state, which will display an "ok" icon to the user (indicating the data was copied to the clipboard).
		// Then, wait for some seconds so that the user can see this.
		updateSecretData(targetSecretIndex, (oldSecretData) => ({
			...oldSecretData,
			copyState: 'resetting' as EditableSerializableSecret['copyState']
		}));

		await new Promise(resolve => setTimeout(() => resolve(null), 3000));


		// Finally, set the button's icon back to normal ("copy" icon)
		updateSecretData(targetSecretIndex, (oldSecretData) => ({
			...oldSecretData,
			copyState: 'idle' as EditableSerializableSecret['copyState']
		}));
	}




	// Render the component
	return (
		<div className="component-SecretsList">
			<ul className="flex flex-col gap-2">
				{internalSecrets.map((secret, secretIndex) =>
					<li key={secretIndex} className="border border-gray-400 rounded-lg p-3 flex gap-4">
						<div className="flex-grow flex flex-wrap gap-1 items-center">
							<span className="font-bold w-28">Description:</span>
							<span className="flex-grow">
								{(() => {
									if (secret.isEditing)
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
									if (secret.isEditing)
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
										switch (secret.copyState)
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
										}
										return <FontAwesomeIcon icon={copyIcon} spin={copyIconShouldSpin} />;
									})()}
							</div>
							<div className="cursor-pointer" onClick={() => onEditSecretButtonClicked(secretIndex)}>
								<FontAwesomeIcon icon={secret.isEditing ? faCheckCircle : faEdit} />
							</div>
							<div className="cursor-pointer" onClick={evt => onAddOrRemoveSecret(evt, internalSecrets.filter((_, filterIndex) => filterIndex !== secretIndex))}>
								<FontAwesomeIcon icon={faTrash} />
							</div>
						</div>
					</li>
				)}
			</ul>
			<div className="flex justify-end mt-4">
				<ButtonLinkWithIcon icon={faPlusCircle} to='/' onClick={evt => onAddOrRemoveSecret(evt, [...internalSecrets, {
					description: `New secret ${uuidv4()}`,
					isValueHashed: false,
					type: IdentityServerConstants.SecretTypes.SharedSecret,
					value: uuidv4(),
				}])}>
					Add another secret
				</ButtonLinkWithIcon>
			</div>
		</div>
	);
}


export default SecretsList;