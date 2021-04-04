/** The claim types specific to the IdP server. */
enum AuthServerClaimTypes
{
	/** Allows the users to view the registered Clients and their related data. */
	CanViewClients = "auth-server.clients.view",
	/** Grants the same access as <see cref="CanViewClients"/>, while also allowing the users to edit (add, update and remove) the
	 * registered Clients and their related data. */
	CanViewAndEditClients = "auth-server.clients.view-and-edit",
	/** Allows the users to view the registered Users and their related data. */
	CanViewUsers = "auth-server.users.view",
	/** Grants the same access as <see cref="CanViewUsers"/>, while also allowing the users to edit (add, update and remove) the
	 * registered Users and their related data. */
	CanViewAndEditUsers = "auth-server.users.view-and-edit",
	/** Allows the users to view the registered Resources (API Scopes, API Resources, Identity Resources) and their related data. */
	CanViewResources = "auth-server.resources.view",
	/** Grants the same access as <see cref="CanViewResources"/>, while also allowing the users to edit (add, update and remove) the
	 * registered Resources (API Scopes, API Resources, Identity Resources) and their related data. */
	CanViewAndEditResources = "auth-server.resources.view-and-edit",
}


export default AuthServerClaimTypes;