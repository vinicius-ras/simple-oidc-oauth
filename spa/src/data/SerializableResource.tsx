/** A type representing a Resource (API Scope, API Resource or Identity Resource) in a serializable format. */
type SerializableResource = {
	/** Indicates if this resource is enabled. */
	enabled?: boolean
	/** The unique name of the resource. */
	name?: string;
	/** Display name of the resource. */
	displayName?: string;
	/** Description of the resource. */
	description?: string;
	/** Specifies whether this scope is shown in the discovery document. */
	showInDiscoveryDocument?: boolean;
	/** List of associated user claims that should be included when this resource is requested. */
	userClaims?: string[];
	/** Custom properties for the resource. */
	properties?: {
		[index: string]: string;
	};
};

export default SerializableResource;