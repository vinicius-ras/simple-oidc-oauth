import { faSave, faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import Lodash from 'lodash';
import React, { useEffect, useState } from 'react';
import Select from 'react-select';
import CreatableSelect from 'react-select/creatable';
import { GrantTypes } from '../data/IdentityModel/OidcConstants';
import SerializableClient from '../data/SerializableClient';
import SerializableResource from '../data/SerializableResource';
import SerializableSecret from '../data/SerializableSecret';
import AppConfigurationService from '../services/AppConfigurationService';
import AxiosService from '../services/AxiosService';
import { AlertColor } from './AlertBox';
import ButtonLink from './ButtonLink';
import CheckBox from './CheckBox';
import ErrorAlert from './ErrorAlert';
import { ErrorDisplayMode } from './ErrorText';
import InputElement from './InputElement';
import SecretsList from './SecretsList';
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

	const [allGrantTypeDescriptors, setAllGrantTypeDescriptors] = useState<ReadonlyArray<GrantTypeDescriptor>>([]);
	const [availableResources, setAvailableResources] = useState<ReadonlyArray<SerializableResource>>([]);
	const [isSubmittingClientData, setIsSubmittingClientData] = useState(false);


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


	/** Called when a Client Secret is added/removed from the list.
	 * @param secretEntries The entries of the select component which holds the Client Secrets. */
	function onClientSecretChange(secretEntries: SerializableSecret[]) {
		// Unhashed secrets will have their descriptions cleared, preventing the "description" field in the database
		// from containing the plaintext password typed for the secret
		secretEntries.forEach(secret => {
			if (secret.isValueHashed === false)
				delete secret.description;
		});

		setSelectedClientEntry(curData => ({
			...curData,
			clientSecrets: [...secretEntries]
		}));
	}


	/** Submits the data for the currently edited client to the back-end, so that it can be saved.
	 * @param evt Object representing the click on the "save client" button. */
	async function saveClient(evt: React.MouseEvent<HTMLAnchorElement, MouseEvent>) {
		evt.preventDefault();

		setIsSubmittingClientData(true);
		try {
			// Decide which endpoint to call: either PUT (update existing client) or POST (create new client)
			const response = (selectedClientEntry?.clientId)
				? await AxiosService.getInstance()
					.put<SerializableClient>(AppConfigurationService.Endpoints.UpdateClientApplication(selectedClientEntry.clientId), selectedClientEntry)
				: await AxiosService.getInstance()
					.post<SerializableClient>(AppConfigurationService.Endpoints.CreateNewClientApplication, selectedClientEntry);

			setSelectedClientEntry(response.data);
		} catch (error) {
			console.error("Failed to save client's data: ", error);
		}
		setIsSubmittingClientData(false);
	}


	// Initialization: request the list of available clients
	useEffect(() => {
		/** Performs a request to the IdP to retrieve a list of all registered Client Applications. */
		async function retrieveRegisteredClientsApplications() {
			try {
				const response = await AxiosService.getInstance()
					.get<SerializableClient[]>(AppConfigurationService.Endpoints.GetAllRegisteredClients);
				setAvailableClients(response.data);
			} catch (error) {
				console.error(`Failed to retrieve registered Client Applications`, error);
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
				console.error(`Failed to retrieve available Grant Types`, error);
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
				console.error(`Failed to retrieve available Resources`, error);
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
		if (!availableClients?.[0])
			setSelectedClientEntry(null);
		else
			setSelectedClientEntry({ ...availableClients[0] });
	}, [availableClients]);



	// Render the component
	const allPostLoginRedirectUris = selectedClientEntry?.redirectUris?.map(url => ({url} as RedirectUrlDescriptor)) ?? [],
		allPostLogoutRedirectUris = selectedClientEntry?.postLogoutRedirectUris?.map(url => ({url} as RedirectUrlDescriptor)) ?? [],
		allCorsOrigins = selectedClientEntry?.allowedCorsOrigins?.map(url => ({url} as RedirectUrlDescriptor)) ?? [];

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



							{/* Allowed Redirect URIs after login/logout. */}
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
								<SecretsList secrets={selectedClientEntry.clientSecrets ?? []} onChange={newSecrets => setSelectedClientEntry(curData => ({ ...curData, clientSecrets: newSecrets}))} />
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
						</section>
					)
					: null
			}
		</div>
	);
}

export default ClientsManagementPage;