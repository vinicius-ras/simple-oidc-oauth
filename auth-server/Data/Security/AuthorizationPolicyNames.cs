namespace SimpleOidcOauth.Data.Security
{
	/// <summary>Holds the name of the Authorization Policies defined for the IdP Management Interface.</summary>
	static class AuthorizationPolicyNames
	{
		/// <summary>
		///     A policy which allows users to read Client Application's data.
		///     This policy is inteded for "guest users" who only have read access to the registered data, and thus
		///     some Client Application data (e.g., client secrets) might be redacted.
		/// </summary>
		public const string ClientsView = "ClientsView";
		/// <summary>A policy which allows users to read, create and update Client Application's data.</summary>
		public const string ClientsViewAndEdit = "ClientsViewAndEdit";
		/// <summary>
		///     A policy which allows users to read other Users's data.
		///     This policy is inteded for "guest users" who only have read access to the registered data, and thus
		///     some of the registered Users' data (e.g., user secrets) might be redacted.
		/// </summary>
		public const string UsersView = "UsersView";
		/// <summary>A policy which allows users to read, create and update registered Users's data.</summary>
		public const string UsersViewAndEdit = "UsersViewAndEdit";
		/// <summary>
		///     A policy which allows users to read Resources's (API Scopes, API Resources and Identity Resources) data.
		///     This policy is inteded for "guest users" who only have read access to the registered data, and thus
		///     some Resources data (e.g., client secrets) might be redacted.
		/// </summary>
		public const string ResourcesView = "ResourcesView";
		/// <summary>A policy which allows users to read, create and update Resources's (API Scopes, API Resources and Identity Resources) data.</summary>
		public const string ResourcesViewAndEdit = "ResourcesViewAndEdit";
	}
}