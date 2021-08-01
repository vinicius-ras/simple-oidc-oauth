import { faExclamationTriangle, faSave, faTimes, faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import Lodash from 'lodash';
import React, { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import Select from 'react-select';
import CreatableSelect from 'react-select/creatable';
import { GrantTypes } from '../data/IdentityModel/OidcConstants';
import SerializableClient from '../data/SerializableClient';
import SerializableResource from '../data/SerializableResource';
import SerializableSecret from '../data/SerializableSecret';
import AppConfigurationService from '../services/AppConfigurationService';
import AxiosService from '../services/AxiosService';
import PromiseExecutorArgs from '../utilities/PromiseExecutorArgs';
import { AlertColor } from './AlertBox';
import AppModal from './AppModal';
import ButtonLink from './ButtonLink';
import ButtonLinkWithIcon from './ButtonLinkWithIcon';
import CheckBox from './CheckBox';
import CopyToClipboardButton from './CopyToClipboardButton';
import ErrorAlert from './ErrorAlert';
import { ErrorDisplayMode } from './ErrorText';
import InputElement from './InputElement';
import SecretsList, { createSecretsListContext, SecretsListContext } from './SecretsList';
import WorkerButtonLinkWithIcon from './WorkerButtonLinkWithIcon';

/** Maps the name of a Grant Type to a corresponding "friendlier" name. */
const GrantTypeFriendlyNames = {
	[GrantTypes.Password]: "Resource Owner Password Credentials Grant",
	[GrantTypes.ClientCredentials]: "Client Credentials Grant",
	[GrantTypes.AuthorizationCode]: "Authorization Code Grant",
	[GrantTypes.Implicit]: "Implicit Grant",
	[GrantTypes.RefreshToken]: "OIDC Refresh Token",
};


/** An object describing an available Grant Type that the IdP has reported to our application. */
type GrantTypeDescriptor = {
	/** The actual string representation of the Grant Type, used in communications with the IdP. */
	grantType: string;
	/** A friendly name for the Grant Type. This is displayed to the user, and is obtained by
	 * using the actual grant type (@see grantType) value as a key in a friendly names map (@see GrantTypeFriendlyNames). */
	friendlyName: string;
};


/** An object describing a Post-Login/Logout Redirect Url. */
type RedirectUrlDescriptor = {
	/** An URL which is the redirection target for the Client Application after login/logout in the IdP server. */
	url: string;
	/** An optional label to be used as the URL's entry text in select boxes. */
	label?: string;
};


/** Props for the {@link ClientsManagementPage} functional component. */
export interface ClientsManagementPageProps
{
}


/** The page which allows registering, updating and managing IdP Client Applications. */
function ClientsManagementPage(props: ClientsManagementPageProps) {
	const [availableClients, setAvailableClients] = useState<SerializableClient[]>([]);
	const [selectedClientEntry, setSelectedClientEntry] = useState<SerializableClient|null>(null);

	const [clientSecrets, setClientSecrets] = useState<SerializableSecret[]>([]);
	const [isClientSecretBeingEdited, setIsClientSecretBeingEdited] = useState<boolean[]>([]);
	const [SecretsListContextObject] = useState(createSecretsListContext({
		clientSecrets,
		setClientSecrets,
		isClientSecretBeingEdited,
		setIsClientSecretBeingEdited,
	}));

	const [allGrantTypeDescriptors, setAllGrantTypeDescriptors] = useState<ReadonlyArray<GrantTypeDescriptor>>([]);
	const [availableResources, setAvailableResources] = useState<ReadonlyArray<SerializableResource>>([]);
	const [isSubmittingClientData, setIsSubmittingClientData] = useState(false);

	const [modalConfirmSaveSecrets, setModalConfirmSaveSecrets] = useState<SerializableSecret[]>([]);
	const [modalConfirmSaveResult, setModalConfirmSaveResult] = useState<PromiseExecutorArgs<boolean>>();


	/** Called when the CORS Origins list is changed.
	 * @param corsSelectEntries The entries of the select component which holds the CORS Origins. */
	function onCorsOriginsChange(corsSelectEntries: RedirectUrlDescriptor[]) {
		// Only accept inputs which are URLs, while mapping them to their respective Origin components only (stripping URL paths, query strings, fragments, etc)
		const validCorsEntries = corsSelectEntries
			.filter(urlDescriptorEntry => {
				try {
					return !!(new URL(urlDescriptorEntry.url).origin);
				} catch (error) {
					return false;
				}
			})
			.map(urlDescriptorEntry => new URL(urlDescriptorEntry.url).origin);

			const uniqueEntries = Lodash.uniq(validCorsEntries);
			setSelectedClientEntry(curData => ({...curData, allowedCorsOrigins: uniqueEntries}));
	}


	/** Called when the Post-Login/Post-Logout Redirect URI list is changed.
	 * @param uriEntries The entries of the select component which holds the Redirect URIs.
	 * @param clientUrisProperty
	 *     The name of the property in the {@link SerializableClient} type which holds
	 *     the Post-Login/Post-Logout Redirect URIs. */
	function onRedirectUrisChange(uriEntries: RedirectUrlDescriptor[], clientUrisProperty: keyof Pick<SerializableClient, "redirectUris" | "postLogoutRedirectUris">) {

		// Only accept inputs which are URLs, discarding the query string and fragment parts.
		// Also, there must be only distinct elements in the list of URIs.
		const validRedirectUriEntries = uriEntries
			.filter(urlDescriptorEntry => {
				try {
					return !!(new URL(urlDescriptorEntry.url));
				} catch (error) {
					return false;
				}
			})
			.map(urlDescriptorEntry => {
				const targetUrl = new URL(urlDescriptorEntry.url);
				return `${targetUrl.origin}${targetUrl.pathname}`;
			});

		const uniqueEntries = Lodash.uniq(validRedirectUriEntries);
		setSelectedClientEntry(curData => ({...curData, [clientUrisProperty]: uniqueEntries}));
	}


	/** Called when one of the buttons on the "Confirm Save Client" modal is clicked.
	 * @param evt The event that generated a call to this method.
	 * @param confirmSaving A flag indicating if the user has confirmed or canceled saving the Client Application's data. */
	function onModalConfirmSaveClientButtonClicked(evt: React.MouseEvent<HTMLElement, MouseEvent>, confirmSaving: boolean) {
		evt?.preventDefault();
		modalConfirmSaveResult?.resolve(confirmSaving);
	}


	/** Displays a modal asking the user to confirm saving the client, and informing that the secrets should be written down because there
	 * will be no way to recover their plain text versions after saving.
	 * @param newSecrets The list of newly-added secrets, that should be written down by the user.
	 * @returns {Promise<boolean>} Returns a promise, wrapping a flag which specifies if the user has confirmed saving the client secrets or not. */
	async function confirmSaveClientModal(newSecrets: SerializableSecret[]): Promise<boolean> {
		setModalConfirmSaveSecrets(newSecrets);
		if (newSecrets.length <= 0)
			return true;

		const modalResultPromise = new Promise<boolean>((resolve, reject) => {
			setModalConfirmSaveResult({resolve, reject});
		});
		const modalResult = await modalResultPromise;
		return modalResult;
	}


	/** Submits the data for the currently edited client to the back-end, so that it can be saved.
	 * @param evt Object representing the click on the "save client" button. */
	async function saveClient(evt: React.MouseEvent<HTMLAnchorElement, MouseEvent>) {
		const isUpdatingExistingApplication = !!(selectedClientEntry?.clientId);
		try
		{
			evt.preventDefault();
			setIsSubmittingClientData(true);


			// Displays a modal asking for the user to confirm saving
			const dataToSend : SerializableClient = {
				...selectedClientEntry,
				clientSecrets,
			};
			const newlyCreatedSecrets = clientSecrets.filter(secret => secret.isValueHashed === false);
			const confirmationModalResult = await confirmSaveClientModal(newlyCreatedSecrets);
			if (!confirmationModalResult)
				return;


			// Decide which endpoint to call: either PUT (update existing client) or POST (create new client)
			const response = isUpdatingExistingApplication
				? await AxiosService.getInstance()
					.put<SerializableClient>(AppConfigurationService.Endpoints.UpdateClientApplication(selectedClientEntry!.clientId!), dataToSend)
				: await AxiosService.getInstance()
					.post<SerializableClient>(AppConfigurationService.Endpoints.CreateNewClientApplication, dataToSend);

			setAvailableClients(oldClients => oldClients.map(client =>
				client.clientId === selectedClientEntry?.clientId
					? SerializableClient.fixJsonDeserialization(response.data)
					: client));
			setSelectedClientEntry(response.data);

			toast.success(isUpdatingExistingApplication
				? "Client Application successfuly updated."
				: "Client Application has been registered.")
		} catch (error) {
			const errorMsg = isUpdatingExistingApplication
				? "Failed to update the Client Application. Please, verify the form for errors."
				: "Failed to create the new Client Application. Please, verify the form for errors.";
			toast.error(errorMsg);
			console.error("Failed to save client's data: ", error);
		} finally {
			setIsSubmittingClientData(false);
		}
	}


	// Initialization: request the list of available clients
	useEffect(() => {
		/** Performs a request to the IdP to retrieve a list of all registered Client Applications. */
		async function retrieveRegisteredClientsApplications() {
			try {
				const response = await AxiosService.getInstance()
					.get<SerializableClient[]>(AppConfigurationService.Endpoints.GetAllRegisteredClients);
				const returnedData : SerializableClient[] = (response.data ?? [])
					.map(client => SerializableClient.fixJsonDeserialization(client));
				setAvailableClients(returnedData);
			} catch (error) {
				const errorMsg = "Failed to retrieve registered Client Applications. Please, verify your connection and if you are currently logged-in.";
				toast.error(errorMsg);
				console.error(errorMsg, error);
				setAvailableClients([]);
			}
		}

		/** Performs a request to the IdP to retrieve a list of all Grant Types available for registering new clients. */
		async function retrieveAvailableGrantTypes() {
			try {
				const response = await AxiosService.getInstance()
					.get<string[]>(AppConfigurationService.Endpoints.GetAllowedClientRegistrationGrantTypes);
				const receivedGrantTypes: ReadonlyArray<GrantTypeDescriptor> = response.data.map(receivedGrantType => ({
					grantType: receivedGrantType,
					friendlyName: GrantTypeFriendlyNames[receivedGrantType as keyof typeof GrantTypeFriendlyNames] ?? receivedGrantType
				}));
				setAllGrantTypeDescriptors(receivedGrantTypes);
			} catch (error) {
				const errorMsg = "Failed to retrieve available Grant Types. Please, verify your connection and if you are currently logged-in.";
				toast.error(errorMsg);
				console.error(errorMsg, error);
				setAllGrantTypeDescriptors([]);
			}
		}

		/** Performs a request to the IdP to retrieve a list of all Resources (API Scopes, API Resources and Identity Resources) currently
		 * available for registering new clients. */
		async function retrieveAvailableResources() {
			try {
				const response = await AxiosService.getInstance()
					.get<SerializableResource[]>(AppConfigurationService.Endpoints.GetAvailableClientRegistrationResources);
				setAvailableResources(response.data);
			} catch (error) {
				const errorMsg = "Failed to retrieve available Resources. Please, verify your connection and if you are currently logged-in.";
				toast.error(errorMsg);
				console.error(errorMsg, error);
				setAvailableResources([]);
			}
		}

		// Retrieve the available Grant Types and Resources data in parallel, and after that retrieve all of the registered Client Applications' data
		const availableGrantsPromise = retrieveAvailableGrantTypes(),
			availableResourcesPromise = retrieveAvailableResources();
		Promise.all([availableGrantsPromise, availableResourcesPromise])
			.then(() => retrieveRegisteredClientsApplications());
	}, []);


	// Effect: when the Clients list gets updated, update the currently selected client to the first one in the list
	useEffect(() => {
		if (selectedClientEntry)
			return;

		if (!availableClients?.[0])
			setSelectedClientEntry(null);
		else
			setSelectedClientEntry({ ...availableClients[0] });
	}, [availableClients, selectedClientEntry]);


	// Effect: when the client currently being edited is changed, update the context used to render the Client Secrets list
	useEffect(() => {
		const secretsData = selectedClientEntry?.clientSecrets ?? [];
		setClientSecrets(secretsData);
		setIsClientSecretBeingEdited(Lodash.times(secretsData.length, Lodash.constant(false)));
	}, [selectedClientEntry]);



	// Render the component
	const allPostLoginRedirectUris = selectedClientEntry?.redirectUris?.map(url => ({url} as RedirectUrlDescriptor)) ?? [],
		allPostLogoutRedirectUris = selectedClientEntry?.postLogoutRedirectUris?.map(url => ({url} as RedirectUrlDescriptor)) ?? [],
		allCorsOrigins = selectedClientEntry?.allowedCorsOrigins?.map(url => ({url} as RedirectUrlDescriptor)) ?? [];

	const secretsListData : SecretsListContext = {
		clientSecrets,
		setClientSecrets,
		isClientSecretBeingEdited,
		setIsClientSecretBeingEdited,
	};

	const shouldDisableControls = (!availableClients || isSubmittingClientData);
	return (
		<div className="component-ClientsManagementPage">
			<section className="flex flex-row flex-wrap items-end max-w-6xl">
				<h1 className="font-bold block w-full mb-2">Pick or create a client</h1>
				<CreatableSelect
					className="flex-grow"
					options={availableClients}
					getOptionValue={serializableClient => serializableClient.clientId ?? ""}
					getOptionLabel={serializableClient => serializableClient.clientName ?? ""}
					value={selectedClientEntry}
					onChange={serializableClient => setSelectedClientEntry(serializableClient ? ({...serializableClient}) : null)}
					isDisabled={shouldDisableControls} />
				<ButtonLink to="/" className="bg-red-600 ml-2" disabled={shouldDisableControls} title="Delete client application">
					<FontAwesomeIcon icon={faTrash} />
					<span className="ml-2 hidden sm:inline">Delete client</span>
				</ButtonLink>
			</section>
			{
				selectedClientEntry
					? (
						<section className="mt-10">
							{/* Basic options. */}
							<h1 className="font-bold">Client configurations</h1>

							<input type="hidden" value={selectedClientEntry.clientId} />
							<label className="block mt-2">
								Name:
								<InputElement name="ClientName" className="outline-none" value={selectedClientEntry.clientName} onChange={evt => setSelectedClientEntry(curData => ({ ...curData, clientName: evt.target.value }))} disabled={shouldDisableControls} />
							</label>
							<label className="block mt-2">
								Allowed OAuth/OIDC Grant Types:
								<ErrorAlert alertBox={{ color: AlertColor.ERROR }} errorText={{ displayMode: ErrorDisplayMode.ERROR_KEY, errorKey: "AllowedGrantTypes" }} />
								<Select isMulti
									options={allGrantTypeDescriptors}
									getOptionValue={grantTypeDescriptor => grantTypeDescriptor.grantType}
									getOptionLabel={grantTypeDescriptor => grantTypeDescriptor.friendlyName}
									value={selectedClientEntry.allowedGrantTypes?.map(grantType => ({
										grantType,
										friendlyName: GrantTypeFriendlyNames[grantType as keyof typeof GrantTypeFriendlyNames] ?? grantType
									} as GrantTypeDescriptor))}
									onChange={selectedGrantTypeDescriptors => setSelectedClientEntry(curData => ({ ...curData, allowedGrantTypes: selectedGrantTypeDescriptors.map(idpGrantType => idpGrantType.grantType) }))}
									isDisabled={shouldDisableControls} />
							</label>
							<label className="block mt-2">
								Allowed scopes:
								<ErrorAlert alertBox={{ color: AlertColor.ERROR }} errorText={{ displayMode: ErrorDisplayMode.ERROR_KEY, errorKey: "AllowedScopes" }} />
								<Select isMulti
									options={availableResources}
									getOptionValue={resource => resource.name}
									getOptionLabel={resource => resource.displayName ?? resource.name}
									value={(availableResources).filter(resourceEntry => selectedClientEntry.allowedScopes?.includes(resourceEntry.name!))}
									onChange={selectedEntriesArray => setSelectedClientEntry(curData => ({ ...curData, allowedScopes: [...selectedEntriesArray.map(resource => resource.name)] }))}
									isDisabled={shouldDisableControls} />
							</label>
							<CheckBox
								text="Requires users to explicitly consent access"
								className="mt-2"
								checked={selectedClientEntry.requireConsent ?? false}
								onChange={evt => setSelectedClientEntry(curData => ({ ...curData, requireConsent: evt.target.checked }))}
								disabled={shouldDisableControls} />



							{/* Allowed Redirect URIs after login/logout. */}
							<h1 className="font-bold mt-10">Post login/logout redirection URIs</h1>
							<label className="block mt-2">
								Allowed post-login redirect URIs:
								<ErrorAlert alertBox={{ color: AlertColor.ERROR }} errorText={{ displayMode: ErrorDisplayMode.ERROR_KEY, errorKey: "RedirectUris" }} />
								<CreatableSelect isMulti
									placeholder="Click to add a Redirect URI."
									noOptionsMessage={() => "Type a Redirect URI to add it."}
									formatCreateLabel={() => `Click here or press ENTER/TAB to add this Redirect URI.`}
									options={allPostLoginRedirectUris}
									getOptionValue={urlDescriptor => urlDescriptor.url}
									getOptionLabel={urlDescriptor => urlDescriptor.label ?? urlDescriptor.url}
									getNewOptionData={(inputText, label) => ({url: inputText, label: label} as RedirectUrlDescriptor)}
									value={allPostLoginRedirectUris}
									onChange={entryValues => onRedirectUrisChange([...entryValues], "redirectUris")}
									isDisabled={shouldDisableControls} />
							</label>
							<label className="block mt-2">
								Allowed post-logout redirect URIs:
								<ErrorAlert alertBox={{ color: AlertColor.ERROR }} errorText={{ displayMode: ErrorDisplayMode.ERROR_KEY, errorKey: "PostLogoutRedirectUris" }} />
								<CreatableSelect isMulti
									placeholder="Click to add a Post-Logout Redirect URI."
									noOptionsMessage={() => "Type a Post-Logout Redirect URI to add it."}
									formatCreateLabel={() => `Click here or press ENTER/TAB to add this Post-Logout Redirect URI.`}
									options={allPostLogoutRedirectUris}
									getOptionValue={urlDescriptor => urlDescriptor.url}
									getOptionLabel={urlDescriptor => urlDescriptor.label ?? urlDescriptor.url}
									getNewOptionData={(inputText, label) => ({url: inputText, label: label} as RedirectUrlDescriptor)}
									value={allPostLogoutRedirectUris}
									onChange={entryValues => onRedirectUrisChange([...entryValues], "postLogoutRedirectUris")}
									isDisabled={shouldDisableControls} />
							</label>



							{/* Options related to Client secrets. */}
							<h1 className="font-bold mb-2 mt-10">Client secrets</h1>
							<CheckBox
								text="Require client app to be authenticated"
								className="mt-2"
								checked={selectedClientEntry.requireClientSecret ?? false}
								onChange={evt => setSelectedClientEntry(curData => ({ ...curData, requireClientSecret: evt.target.checked }))}
								disabled={shouldDisableControls} />
							<label className="block mt-2">
								Registered client secrets:
								<ErrorAlert alertBox={{ color: AlertColor.ERROR }} errorText={{ displayMode: ErrorDisplayMode.ERROR_KEY, errorKey: "ClientSecrets" }} />
								<SecretsListContextObject.Provider value={secretsListData}>
									<SecretsList context={SecretsListContextObject} />
								</SecretsListContextObject.Provider>
							</label>



							{/* Advanced options. */}
							<h1 className="font-bold mt-10">Advanced client configurations</h1>
							<CheckBox
								text="Allow access tokens via browser"
								className="mt-2"
								checked={selectedClientEntry.allowAccessTokensViaBrowser ?? false}
								onChange={evt => setSelectedClientEntry(curData => ({ ...curData, allowAccessTokensViaBrowser: evt.target.checked }))}
								disabled={shouldDisableControls} />
							<CheckBox
								text="Enforce PKCE usage"
								className="mt-2"
								checked={selectedClientEntry.requirePkce ?? false}
								onChange={evt => setSelectedClientEntry(curData => ({ ...curData, requirePkce: evt.target.checked }))}
								disabled={shouldDisableControls} />
							<label className="block mt-2">
								Allowed CORS Origins:
								<CreatableSelect isMulti
									placeholder="Click to add an Allowed CORS Origin."
									noOptionsMessage={() => "Type a CORS Origin to add it."}
									formatCreateLabel={() => `Click here or press ENTER/TAB to add this CORS Origin.`}
									options={allCorsOrigins}
									getOptionValue={urlDescriptor => urlDescriptor.url}
									getOptionLabel={urlDescriptor => urlDescriptor.label ?? urlDescriptor.url}
									getNewOptionData={(inputText, creationLabel) => ({url: inputText, label: creationLabel} as RedirectUrlDescriptor)}
									value={allCorsOrigins}
									onChange={entryValues => onCorsOriginsChange([...entryValues])}
									isDisabled={shouldDisableControls} />
							</label>



							{/* Submit button. */}
							<div className="mt-10">
								<WorkerButtonLinkWithIcon to="/" icon={faSave} className="mt-10" onClick={evt => saveClient(evt)} isBusy={isSubmittingClientData}>
									Save
								</WorkerButtonLinkWithIcon>
							</div>


							{ /* MODAL: displays the list of new secrets and a warning for the user to keep the new secrets' plaintext versions safe, as they won't be retrievable anymore. */}
							<AppModal isOpen={isSubmittingClientData && modalConfirmSaveSecrets.length > 0} contentLabel="Warning: store your secrets in a safe place.">
								<div className="flex justify-center text-6xl text-yellow-500">
									<FontAwesomeIcon icon={faExclamationTriangle} />
								</div>
								<div className="space-y-4 mt-8">
									<p>
										<span className="font-bold">Attention: </span>
										Write down the following newly-created secrets.
									</p>
									<p>After saving, they will be encrypted and stored in a database, and you will not be able to recover their plaintext versions in the future.</p>
								</div>
								<ul className="mt-8">
									{
										modalConfirmSaveSecrets
											.map((secret, secretIndex) =>
												<li key={`${secretIndex}|${secret.description ?? 'no-description'}`}>
													{secretIndex === 0 ? null : <div className="border-t border-gray-500 my-4" />}
													<div className="flex flex-col">
														<span className="font-bold">Secret: </span>
														<span className="truncate" title={secret.description}>{secret.description}</span>
														<span className="font-bold">Value: </span>
														<div className="space-x-4 truncate">
															<CopyToClipboardButton contentsToCopy={secret.value!} title="Copy secret to clipboard" copySuccessToast="Secret copied to clipboard." />
															<span className="truncate" title={secret.value}>{secret.value}</span>
														</div>
													</div>
												</li>
										)
									}
								</ul>
								<div className="flex justify-between mt-8">
									<ButtonLinkWithIcon to="/" icon={faTimes} onClick={evt => onModalConfirmSaveClientButtonClicked(evt, false)}>Cancel</ButtonLinkWithIcon>
									<ButtonLinkWithIcon to="/" icon={faSave} className="bg-yellow-600" onClick={evt => onModalConfirmSaveClientButtonClicked(evt, true)}>Proceed</ButtonLinkWithIcon>
								</div>
							</AppModal>
						</section>
					)
					: null
			}
		</div>
	);
}

export default ClientsManagementPage;
