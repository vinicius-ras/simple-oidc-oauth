using IdentityServer4.Test;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Services
{
	/// <summary>A service which can be used to initialize the application's database with configuration-based data.</summary>
	public interface IDatabaseInitializerService
	{
		/// <summary>
		///     Clears all of the entries of the database, leaving it completely empty (while preserving its table structures).
		///     This method should be used with caution, as it might lead to data loss.
		/// </summary>
		/// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
		Task ClearDatabaseAsync();


		/// <summary>Initializes the database(s) with the test/development data.</summary>
		/// <param name="clients">A collection of clients to be saved to the database.</param>
		/// <param name="apiScopes">A collection of API Scopes to be saved to the database.</param>
		/// <param name="apiResources">A collection of API Resources to be saved to the database.</param>
		/// <param name="identityResources">A collection of Identity Resources to be saved to the database.</param>
		/// <param name="users">A collection of users to be saved to the database.</param>
		/// <returns>
		///     Returns a <see cref="DatabaseInitializationResult"/> object containing data about the performed operations,
		///     wrapped within a <see cref="Task"/> object.
		/// </returns>
		Task<DatabaseInitializationResult> InitializeDatabaseAsync(
			IEnumerable<SerializableClient> clients = default,
			IEnumerable<SerializableApiScope> apiScopes = default,
			IEnumerable<SerializableApiResource> apiResources = default,
			IEnumerable<SerializableIdentityResource> identityResources = default,
			IEnumerable<TestUser> users = default);
	}
}