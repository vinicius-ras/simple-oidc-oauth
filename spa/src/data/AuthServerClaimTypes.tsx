/** The claim types specific to the IdP server. */
enum AuthServerClaimTypes
{
	/** Allows the users to view the registered Clients and their related data. */
	CanViewClients = "auth-server.clients.view",
	/** Allows the users to edit (add, update and remove) the registered Clients and their related data. */
	CanEditClients = "auth-server.clients.edit",
	/** Allows the users to view the registered Users and their related data. */
	CanViewUsers = "auth-server.users.view",
	/** Allows the users to edit (add, update and remove) the registered Users and their related data. */
	CanEditUsers = "auth-server.users.edit",
	/** Allows the users to view the registered Resources (API Scopes, API Resources, Identity Resources) and their related data. */
	CanViewResources = "auth-server.resources.view",
	/** Allows the users to edit (add, update and remove) the registered Resources (API Scopes, API Resources, Identity Resources) and their related data. */
	CanEditResources = "auth-server.resources.edit",
}


export default AuthServerClaimTypes;