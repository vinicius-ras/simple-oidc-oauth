import { faSave, faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import Lodash from 'lodash';
import React, { useEffect, useState } from 'react';
import Select, { OptionsType, OptionTypeBase } from 'react-select';
import CreatableSelect from 'react-select/creatable';
import { GrantTypes } from '../data/IdentityModel/OidcConstants';
import IdentityServerConstants from '../data/IdentityServerConstants';
import SerializableClient from '../data/SerializableClient';
import SerializableResource from '../data/SerializableResource';
import AppConfigurationService from '../services/AppConfigurationService';
import AxiosService from '../services/AxiosService';
import ButtonLink from './ButtonLink';
import CheckBox from './CheckBox';
import InputElement from './InputElement';
import WorkerButtonLinkWithIcon from './WorkerButtonLinkWithIcon';

/** Maps the name of a Grant Type to a corresponding "friendlier" name. */
const GrantTypeFriendlyNames = {
	[GrantTypes.Password]: "Resource Owner Password Credentials Grant",
	[GrantTypes.ClientCredentials]: "Client Credentials Grant",
	[GrantTypes.AuthorizationCode]: "Authorization Code Grant",
	[GrantTypes.Implicit]: "Implicit Grant",
	[GrantTypes.RefreshToken]: "OIDC Refresh Token",
};


/** Props for the {@link ClientsManagementPage} functional component. */
export interface ClientsManagementPageProps
{
}


/** The page which allows registering, updating and managing IdP Client Applications. */
function ClientsManagementPage(props: ClientsManagementPageProps) {
	const [availableClients, setAvailableClients] = useState<SerializableClient[]|null>(null);
	const [selectedClientEntry, setSelectedClientEntry] = useState<OptionTypeBase|null>(null);
	const [editedClientData, setEditedClientData] = useState<SerializableClient|null>({});

	const [availableGrantTypes, setAvailableGrantTypes] = useState<ReadonlyArray<OptionTypeBase>|null>(null);
	const [availableResources, setAvailableResources] = useState<ReadonlyArray<OptionTypeBase>|null>(null);
	const [isSubmittingClientData, setIsSubmittingClientData] = useState(false);

	/** Updates the currently selected client, displaying its data in the page's form for edition.
	 * @param selectedClient The option representing the selected client. */
	function updateSelectedClientDataForm(selectedClient: OptionTypeBase|null) {
		// Update displayed data
		setSelectedClientEntry(selectedClient);
		if (!selectedClient) {
			setEditedClientData(null);
			return;
		}

		const serializableClient = (availableClients ?? []).find(client => client.clientId === selectedClient.value);
		if (!serializableClient) {
			setEditedClientData(null);
			return;
		}

		setEditedClientData(serializableClient);
	};


	/** Called when the CORS Origins list is changed.
	 * @param corsSelectEntries The entries of the select component which holds the CORS Origins. */
	function onCorsOriginsChange(corsSelectEntries: OptionsType<OptionTypeBase>) {
		// Only accept inputs which are URLs, while mapping them to their respective Origin components only (stripping URL paths, query strings, fragments, etc)
		const validCorsEntries = corsSelectEntries
			.filter(entry => {
				try {
					return !!(new URL(entry.value).origin);
				} catch (error) {
					return false;
				}
			})
			.map(entry => new URL(entry.value).origin);

			const uniqueEntries = Lodash.uniq(validCorsEntries);
			setEditedClientData(curData => ({...curData, allowedCorsOrigins: uniqueEntries}));
	}


	/** Called when the Post-Login/Post-Logout Redirect URI list is changed.
	 * @param uriEntries The entries of the select component which holds the Redirect URIs.
	 * @param clientUrisProperty
	 *     The name of the property in the {@link SerializableClient} type which holds
	 *     the Post-Login/Post-Logout Redirect URIs. */
	function onRedirectUrisChange(uriEntries: OptionsType<OptionTypeBase>, clientUrisProperty: keyof Pick<SerializableClient, "redirectUris" | "postLogoutRedirectUris">) {

		// Only accept inputs which are URLs, discarding the query string and fragment parts.
		// Also, there must be only distinct elements in the list of URIs.
		const validRedirectUriEntries = uriEntries
			.filter(entry => {
				try {
					return !!(new URL(entry.value));
				} catch (error) {
					return false;
				}
			})
			.map(entry => {
				const targetUrl = new URL(entry.value);
				return `${targetUrl.origin}${targetUrl.pathname}`;
			});

		const uniqueEntries = Lodash.uniq(validRedirectUriEntries);
		setEditedClientData(curData => ({...curData, [clientUrisProperty]: uniqueEntries}));
	}


	/** Submits the data for the currently edited client to the back-end, so that it can be saved.
	 * @param evt Object representing the click on the "save client" button. */
	async function saveClient(evt: React.MouseEvent<HTMLAnchorElement, MouseEvent>) {
		evt.preventDefault();

		setIsSubmittingClientData(true);
		try {
			await AxiosService.getInstance()
				.post<SerializableClient>(AppConfigurationService.Endpoints.CreateNewClientApplication, editedClientData);
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
				setAvailableGrantTypes(response.data.map(grantTypeName => ({
					label: GrantTypeFriendlyNames[grantTypeName as keyof typeof GrantTypeFriendlyNames] ?? grantTypeName,
					value: grantTypeName
				})));
			} catch (error) {
				console.error(`Failed to retrieve available Grant Types`, error);
				setAvailableGrantTypes([]);
			}
		}

		/** Performs a request to the IdP to retrieve a list of all Resources (API Scopes, API Resources and Identity Resources) currently
		 * available for registering new clients. */
		async function retrieveAvailableResources() {
			try {
				const response = await AxiosService.getInstance()
					.get<SerializableResource[]>(AppConfigurationService.Endpoints.GetAvailableClientRegistrationResources);
				setAvailableResources(response.data.map(resource => ({
					label: resource.displayName ?? resource.name,
					value: resource.name,
				})));
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
			setSelectedClientEntry( {value: availableClients[0].clientId, label: availableClients[0].clientName });
	}, [availableClients]);


	// Effect: when the currently selected Client entry changes, update the form with the data that is being edited
	useEffect(() => {
		// Try to find the selected client's data
		if (!availableClients || !selectedClientEntry) {
			setEditedClientData(null);
			return;
		}
		const foundClient = availableClients.find(client => client.clientId === selectedClientEntry.value);
		setEditedClientData(foundClient ?? null);
	}, [availableClients, selectedClientEntry]);



	// Render the component
	const optionsToDisplay = (availableClients ?? []).map(serializableClient => ({
		value: serializableClient.clientId,
		label: serializableClient.clientName
	}));

	const allPostLoginRedirectUris = editedClientData?.redirectUris?.map(uri => ({label: uri, value: uri})) ?? [],
		allPostLogoutRedirectUris = editedClientData?.postLogoutRedirectUris?.map(uri => ({label: uri, value: uri})) ?? [],
		allCorsOrigins = editedClientData?.allowedCorsOrigins?.map(corsOrigin => ({label: corsOrigin, value: corsOrigin})) ?? [];

	const shouldDisableControls = (!availableClients || isSubmittingClientData);
	return (
		<div className="component-ClientsManagementPage">
			<section className="flex flex-row flex-wrap items-end max-w-6xl">
				<h1 className="font-bold block w-full mb-2">Pick or create a client</h1>
				<CreatableSelect
					className="flex-grow"
					options={optionsToDisplay}
					value={selectedClientEntry}
					onChange={newValue => updateSelectedClientDataForm(newValue)}
					isDisabled={shouldDisableControls} />
				<ButtonLink to="/" className="bg-red-600 ml-2" disabled={shouldDisableControls} title="Delete client application">
					<FontAwesomeIcon icon={faTrash} />
					<span className="ml-2 hidden sm:inline">Delete client</span>
				</ButtonLink>
			</section>
			{
				editedClientData
				? (
					<section className="mt-10">
						{/* Basic options. */}
						<h1 className="font-bold">Client configurations</h1>

						<input type="hidden" value={editedClientData.clientId} />
						<label className="block mt-2">
							Name:
							<InputElement className="outline-none" value={editedClientData.clientName} onChange={evt => setEditedClientData(curData => ({...curData, clientName: evt.target.value}))} disabled={shouldDisableControls} />
						</label>
						<label className="block mt-2">
							Allowed OAuth/OIDC Grant Types:
							<Select isMulti
								onChange={selectedEntriesArray => setEditedClientData(curData => ({...curData, allowedGrantTypes: selectedEntriesArray.map(entry => entry.value)}))}
								options={availableGrantTypes ?? []}
								value={(availableGrantTypes ?? []).filter(grantTypeEntry => editedClientData.allowedGrantTypes?.includes(grantTypeEntry.value))}
								isDisabled={shouldDisableControls} />
						</label>
						<label className="block mt-2">
							Allowed scopes:
							<Select isMulti
								onChange={selectedEntriesArray => setEditedClientData(curData => ({...curData, allowedScopes: selectedEntriesArray.map(entry => entry.value)}))}
								options={availableResources ?? []}
								value={(availableResources ?? []).filter(resourceEntry => editedClientData.allowedScopes?.includes(resourceEntry.value))}
								isDisabled={shouldDisableControls} />
						</label>
						<CheckBox
							text="Requires users to explicitly consent access"
							className="mt-2"
							checked={editedClientData.requireConsent ?? false}
							onChange={evt => setEditedClientData(curData => ({...curData, requireConsent: evt.target.checked}))}
							disabled={shouldDisableControls} />



						{/* Allowed Redirect URIs after login/logout. */}
						<h1 className="font-bold mt-10">Post login/logout redirection URIs</h1>
						<label className="block mt-2">
							Allowed post-login redirect URIs:
							<CreatableSelect isMulti
								placeholder="Click to add a Redirect URI."
								noOptionsMessage={() => "Type a Redirect URI to add it."}
								formatCreateLabel={inputValue => `Click here or press ENTER/TAB to add this Redirect URI.`}
								onChange={entryValues => onRedirectUrisChange(entryValues, "redirectUris")}
								options={allPostLoginRedirectUris}
								value={allPostLoginRedirectUris}
								isDisabled={shouldDisableControls} />
						</label>
						<label className="block mt-2">
							Allowed post-logout redirect URIs:
							<CreatableSelect isMulti
								placeholder="Click to add a Post-Logout Redirect URI."
								noOptionsMessage={() => "Type a Post-Logout Redirect URI to add it."}
								formatCreateLabel={inputValue => `Click here or press ENTER/TAB to add this Post-Logout Redirect URI.`}
								onChange={entryValues => onRedirectUrisChange(entryValues, "postLogoutRedirectUris")}
								options={allPostLogoutRedirectUris}
								value={allPostLogoutRedirectUris}
								isDisabled={shouldDisableControls} />
						</label>



						{/* Allowed Redirect URIs after login/logout. */}
						<h1 className="font-bold mb-2 mt-10">Client secrets</h1>
						<CheckBox
							text="Require client app to be authenticated"
							className="mt-2"
							checked={editedClientData.requireClientSecret ?? false}
							onChange={evt => setEditedClientData(curData => ({...curData, requireClientSecret: evt.target.checked}))}
							disabled={shouldDisableControls} />
						<label className="block mt-2">
							Registered client secrets:
							<CreatableSelect isMulti
								placeholder="Click to add a Client App's Secret."
								noOptionsMessage={() => "Type a Client App's Secret to add it."}
								formatCreateLabel={inputValue => `Click here or press ENTER/TAB to add this Client App's Secret.`}
								onChange={selectedEntriesArray => setEditedClientData(curData => ({
									...curData,
									clientSecrets: selectedEntriesArray.map(entry => ({
										type: IdentityServerConstants.SecretTypes.SharedSecret,
										value: entry.value,
										isValueHashed: false,
									}))
								}))}
								options={editedClientData.clientSecrets?.map(clientSecret => ({label: clientSecret.value, value: clientSecret.value})) ?? []}
								isDisabled={shouldDisableControls} />
						</label>



						{/* Advanced options. */}
						<h1 className="font-bold mt-10">Advanced client configurations</h1>
						<CheckBox
							text="Allow access tokens via browser"
							className="mt-2"
							checked={editedClientData.allowAccessTokensViaBrowser ?? false}
							onChange={evt => setEditedClientData(curData => ({...curData, allowAccessTokensViaBrowser: evt.target.checked}))}
							disabled={shouldDisableControls} />
						<CheckBox
							text="Enforce PKCE usage"
							className="mt-2"
							checked={editedClientData.requirePkce ?? false}
							onChange={evt => setEditedClientData(curData => ({...curData, requirePkce: evt.target.checked}))}
							disabled={shouldDisableControls} />
						<label className="block mt-2">
							Allowed CORS Origins:
							<CreatableSelect isMulti
								placeholder="Click to add an Allowed CORS Origin."
								noOptionsMessage={() => "Type a CORS Origin to add it."}
								formatCreateLabel={inputValue => `Click here or press ENTER/TAB to add this CORS Origin.`}
								onChange={onCorsOriginsChange}
								options={allCorsOrigins}
								value={allCorsOrigins}
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