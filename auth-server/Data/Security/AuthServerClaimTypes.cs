namespace SimpleOidcOauth.Data.Security
{
	/// <summary>The claim types used internally by the IdP server.</summary>
	public static class AuthServerClaimTypes
	{
		/// <summary>Allows the users to view the registered Clients and their related data.</summary>
		public const string CanViewClients = "auth-server.clients.view";
		/// <summary>Allows the users to edit (add, update and remove) the registered Clients and their related data.</summary>
		public const string CanEditClients = "auth-server.clients.edit";
		/// <summary>Allows the users to view the registered Users and their related data.</summary>
		public const string CanViewUsers = "auth-server.users.view";
		/// <summary>Allows the users to edit (add, update and remove) the registered Users and their related data.</summary>
		public const string CanEditUsers = "auth-server.users.edit";
		/// <summary>Allows the users to view the registered Resources (API Scopes, API Resources, Identity Resources) and their related data.</summary>
		public const string CanViewResources = "auth-server.resources.view";
		/// <summary>Allows the users to edit (add, update and remove) the registered Resources (API Scopes, API Resources, Identity Resources) and their related data.</summary>
		public const string CanEditResources = "auth-server.resources.edit";
	}
}