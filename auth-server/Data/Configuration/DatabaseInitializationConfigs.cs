using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace SimpleOidcOauth.Data.Configuration
{
	/// <summary>
	///     <para>A class holding optional configurations that will be used to initialize the application's database.</para>
	///     <para>
	///         The database initialization configurations are optional. If present, these configurations will be applied by creating
	///         the necessary entries during the auth-server's initialization.
	///     </para>
	/// </summary>
	public class DatabaseInitializationConfigs
	{
		// PROPERTIES
		/// <summary>A flag indicating if database initialization through the configuration infrastructure is enabled.</summary>
		public bool Enabled { get; set; }
		/// <summary>
		///     <para>An optional collection of users that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         Users will be looked up by their <see cref="TestUser.Username"/> property.
		///         Those users that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>These settings can be used to initialize the application with preconfigured users (e.g., a preconfigured "administrator" user).</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of users required to be present.</value>
		public IEnumerable<TestUser> Users { get; set; }
		/// <summary>
		///     <para>An optional collection of clients that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         Clients will be looked up by their <see cref="Client.ClientId"/> property.
		///         Those clients that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>These settings can be used to initialize the application with preconfigured client apps.</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of clients required to be present.</value>
		public IEnumerable<Client> Clients { get; set; }
		/// <summary>
		///     <para>An optional collection of API Scopes that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         API Scopes will be looked up by their <see cref="Resource.Name"/> property.
		///         Those API Scopes that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>
		///         These settings can be used to initialize the application with preconfigured API Scopes (e.g., the
		///         standard "openid" and "profile" scopes, required by the OpenID Connect standards).</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of API Scopes required to be present.</value>
		public IEnumerable<ApiScope> ApiScopes { get; set; }
		/// <summary>
		///     <para>An optional collection of API Resources that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         API Resources will be looked up by their <see cref="Resource.Name"/> property.
		///         Those API Resources that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>These settings can be used to initialize the application with preconfigured API Resources.</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of API Resources required to be present.</value>
		public IEnumerable<ApiResource> ApiResources { get; set; }
		/// <summary>
		///     <para>An optional collection of Identity Resources that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         Identity Resources will be looked up by their <see cref="Resource.Name"/> property.
		///         Those Identity Resources that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>These settings can be used to initialize the application with preconfigured Identity Resources.</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of Identity Resources required to be present.</value>
		public IEnumerable<IdentityResource> IdentityResources { get; set; }
	}
}