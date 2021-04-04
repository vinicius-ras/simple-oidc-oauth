namespace SimpleOidcOauth.Data.Security
{
	/// <summary>The claim types used internally by the IdP server.</summary>
	public static class AuthServerClaimTypes
	{
		/// <summary>Allows the users to view the registered Clients and their related data.</summary>
		public const string CanViewClients = "auth-server.clients.view";
		/// <summary>
		///     Grants the same access as <see cref="CanViewClients"/>, while also allowing the users to edit (add, update and remove) the
		///     registered Clients and their related data.
		/// </summary>
		public const string CanViewAndEditClients = "auth-server.clients.view-and-edit";
		/// <summary>Allows the users to view the registered Users and their related data.</summary>
		public const string CanViewUsers = "auth-server.users.view";
		/// <summary>
		///     Grants the same access as <see cref="CanViewUsers"/>, while also allowing the users to edit (add, update and remove) the
		///     registered Users and their related data.
		/// </summary>
		public const string CanViewAndEditUsers = "auth-server.users.view-and-edit";
		/// <summary>Allows the users to view the registered Resources (API Scopes, API Resources, Identity Resources) and their related data.</summary>
		public const string CanViewResources = "auth-server.resources.view";
		/// <summary>
		///     Grants the same access as <see cref="CanViewResources"/>, while also allowing the users to edit (add, update and remove) the
		///     registered Resources (API Scopes, API Resources, Identity Resources) and their related data.
		/// </summary>
		public const string CanViewAndEditResources = "auth-server.resources.view-and-edit";
	}
}