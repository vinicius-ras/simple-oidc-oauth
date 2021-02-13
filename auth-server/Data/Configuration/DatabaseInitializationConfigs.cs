using System.Collections.Generic;
using SimpleOidcOauth.Data.Serialization;

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
		/// <summary>A flag indicating if database initialization through the configuration infrastructure is enabled for initializing the database's structure.</summary>
		public bool InitializeStructure { get; set; }
		/// <summary>A flag indicating if database initialization through the configuration infrastructure is enabled for initializing the database's data.</summary>
		public bool InitializeData { get; set; }
		/// <summary>
		///     A flag indicating if database should be cleared before it is initialized.
		///     Only valid if either <see cref="InitializeStructure"/> or <see cref="InitializeData"/> are set to <c>true</c>.
		/// </summary>
		public bool CleanBeforeInitialize { get; set; }
		/// <summary>
		///     <para>An optional collection of users that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         Users will be looked up by their <see cref="SerializableTestUser.Username"/> property.
		///         Those users that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>These settings can be used to initialize the application with preconfigured users (e.g., a preconfigured "administrator" user).</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of users required to be present.</value>
		public IEnumerable<SerializableTestUser> Users { get; set; }
		/// <summary>
		///     <para>An optional collection of clients that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         Clients will be looked up by their <see cref="SerializableClient.ClientId"/> property.
		///         Those clients that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>These settings can be used to initialize the application with preconfigured client apps.</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of clients required to be present.</value>
		public IEnumerable<SerializableClient> Clients { get; set; }
		/// <summary>
		///     <para>An optional collection of API Scopes that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         API Scopes will be looked up by their <see cref="SerializableResource.Name"/> property.
		///         Those API Scopes that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>
		///         These settings can be used to initialize the application with preconfigured API Scopes (e.g., the
		///         standard "openid" and "profile" scopes, required by the OpenID Connect standards).</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of API Scopes required to be present.</value>
		public IEnumerable<SerializableApiScope> ApiScopes { get; set; }
		/// <summary>
		///     <para>An optional collection of API Resources that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         API Resources will be looked up by their <see cref="SerializableResource.Name"/> property.
		///         Those API Resources that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>These settings can be used to initialize the application with preconfigured API Resources.</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of API Resources required to be present.</value>
		public IEnumerable<SerializableApiResource> ApiResources { get; set; }
		/// <summary>
		///     <para>An optional collection of Identity Resources that the IdP server will ensure exist in the database, during the server's startup.</para>
		///     <para>
		///         Identity Resources will be looked up by their <see cref="SerializableResource.Name"/> property.
		///         Those Identity Resources that are not present in the database will be created during the IdP's startup.
		///     </para>
		///     <para>These settings can be used to initialize the application with preconfigured Identity Resources.</para>
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> collection of Identity Resources required to be present.</value>
		public IEnumerable<SerializableIdentityResource> IdentityResources { get; set; }
	}
}