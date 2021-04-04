import SerializableSecret from "./SerializableSecret";

/** A serializable representation of a client application. */
type SerializableClient = {
	/** Controls whether access tokens are transmitted via the browser for this client (defaults to false).
	 * This can prevent accidental leakage of access tokens when multiple response types are allowed. */
	allowAccessTokensViaBrowser?: boolean,
	/** The allowed CORS origins for JavaScript clients. */
	allowedCorsOrigins?: string[],
	/** Specifies the allowed grant types (legal combinations of AuthorizationCode, Implicit, Hybrid,
	 * ResourceOwner, ClientCredentials). */
	allowedGrantTypes?: string[],
	/** Specifies the api scopes that the client is allowed to request. If empty, the client can't access
	 * any scope. */
	allowedScopes?: string[],
	/** Unique ID of the client. */
	clientId?: string,
	/** Client display name (used for logging and consent screen). */
	clientName?: string,
	/** Client secrets - only relevant for flows that require a secret. */
	clientSecrets?: SerializableSecret[],
	/** Specifies allowed URIs to redirect to after logout. */
	postLogoutRedirectUris?: string[],
	/** Specifies allowed URIs to return tokens or authorization codes to. */
	redirectUris?: string[],
	/** If set to false, no client secret is needed to request tokens at the token endpoint. */
	requireClientSecret?: boolean,
	/** Specifies whether a consent screen is required. */
	requireConsent?: boolean,
	/** Specifies whether a proof key is required for authorization code based token requests. */
	requirePkce?: boolean,
};

export default SerializableClient;