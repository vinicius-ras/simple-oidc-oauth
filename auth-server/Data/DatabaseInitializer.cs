using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleOidcOauth.Data
{
	/// <summary>
	///     A class with utility methods for initializing databases that can be used to store the
	///     application's data.
	/// </summary>
	public static class DatabaseInitializer
	{
		// PRIVATE METHODS
		/// <summary>
		///     Utility method for saving the entities of a test collection into a database.
		///     This method will only save entities which are not yet present in the target database.
		/// </summary>
		/// <typeparam name="TIdentityServerModel">The type which represents the entities to be saved on IdentityServer's model realm.</typeparam>
		/// <typeparam name="TEntityFrameworkModel">The type which represents the entities to be saved on Entity Framework Core's model realm</typeparam>
		/// <typeparam name="TKey">The type of a key which will be used to verify which of the entities are already present in the target database.</typeparam>
		/// <param name="testEntities">
		///     <para>A collection containing all of the test entities which can be persisted in the target database.</para>
		///     <para>If this parameter is set to <c>null</c>, this method does nothing and returns immediately with an empty <see cref="IEnumerable{T}"/> result.</para>
		/// </param>
		/// <param name="databaseCollection">A <see cref="DbSet{TEntity}" /> object used to manage entities in the target database.</param>
		/// <param name="identityServerModelKeySelector">A function which takes an object from the IdentityServer's model realm and extracts a key discriminator for comparing it to other entities.</param>
		/// <param name="entityFrameworkModelKeySelector">A function which takes an object from the Entity Framework Core's model realm and extracts a key discriminator for comparing it to other entities.</param>
		/// <param name="convertToEntityFrameworkModel">
		///     A function which takes an object from the IdentityServer's model realm and converts it to the Entity Framework Core's model realm.
		///     This function is used to convert entities to the right class before saving them to the target database.
		/// </param>
		/// <return>Returns a list saved entities.</return>
		private static async Task<IEnumerable<TEntityFrameworkModel>> SaveAllUnsavedTestEntities<TIdentityServerModel, TEntityFrameworkModel, TKey>(
			IQueryable<TIdentityServerModel> testEntities,
			DbSet<TEntityFrameworkModel> databaseCollection,
			Expression<Func<TIdentityServerModel, TKey>> identityServerModelKeySelector,
			Expression<Func<TEntityFrameworkModel, TKey>> entityFrameworkModelKeySelector,
			Expression<Func<TIdentityServerModel, TEntityFrameworkModel>> convertToEntityFrameworkModel) where TEntityFrameworkModel : class
		{
			if (testEntities == null)
				return Enumerable.Empty<TEntityFrameworkModel>();

			TKey[] testEntitiesKeys = testEntities
				.Select(identityServerModelKeySelector)
				.ToArray();
			var alreadyRegisteredEntityKeys = await databaseCollection
				.Select(entityFrameworkModelKeySelector)
				.Where(databaseEntityKey => testEntitiesKeys.Contains(databaseEntityKey))
				.ToArrayAsync();
			var notRegisteredEntityKeys = testEntitiesKeys.Except(alreadyRegisteredEntityKeys);

			var compiledKeyExtractor = identityServerModelKeySelector.Compile();
			var entitiesToSave = testEntities
				.Where(testEntity => notRegisteredEntityKeys.Contains(compiledKeyExtractor(testEntity)))
				.Select(convertToEntityFrameworkModel)
				.ToList();
			await databaseCollection.AddRangeAsync(entitiesToSave);
			return entitiesToSave;
		}


		/// <summary>
		///     Converts an IdentityServer <see cref="TestUser" /> object to the
		///     corresponding ASP.NET Core Identity user model representation, as used by this application.
		/// </summary>
		/// <param name="testUser">The <see cref="TestUser" /> object to be converted.</param>
		/// <returns>
		///     Returns an <see cref="IdentityUser" /> object containing the retrieved data
		///     which was converted from the input object.
		/// </returns>
		private static IdentityUser ConvertTestUserToIdentityUser(TestUser testUser, UserManager<IdentityUser> userManager)
		{
			var result = new IdentityUser()
			{
				UserName = testUser.Username,
				Email = testUser.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value,
				EmailConfirmed = testUser.Claims
					.FirstOrDefault(c => c.Type == JwtClaimTypes.EmailVerified)?.Value
					switch {
						null => false,
						string claimValue => bool.Parse(claimValue),
					},
				PhoneNumber = testUser.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.PhoneNumber)?.Value,
			};

			result.NormalizedEmail = userManager.NormalizeEmail(result.Email);
			result.NormalizedUserName = userManager.NormalizeName(result.UserName);
			result.PasswordHash = userManager.PasswordHasher.HashPassword(result, testUser.Password);
			return result;
		}


		/// <summary>
		///     Ensures the given database contexts have their respective databases created, and that these
		///     databases have their respective migrations applied and up-to-date.
		/// </summary>
		/// <param name="dbContexts">The contexts of the databases where the operations will be performed.</param>
		/// <returns>Returns a task representing the underlying asynchronous operation.</returns>
		private static async Task EnsureDatabasesCreatedAndMigrationsAppliedAsync(params DbContext[] dbContexts)
		{
			foreach (var curDbContext in dbContexts)
			{
				await curDbContext.Database.EnsureCreatedAsync();

				var allMigrations = curDbContext.Database.GetMigrations().ToList();
				var appliedMigrations = await curDbContext.Database.GetAppliedMigrationsAsync();

				var pendingMigrations = await curDbContext.Database.GetPendingMigrationsAsync();
				if (pendingMigrations.Any())
					await curDbContext.Database.MigrateAsync();
			}
		}





		// PUBLIC METHODS
		/// <summary>Initializes the database(s) with the test/development data.</summary>
		/// <param name="serviceProvider">A service provider instance used to retrieve the required database-related services.</param>
		/// <param name="clients">A collection of clients to be saved to the database.</param>
		/// <param name="apiScopes">A collection of API Scopes to be saved to the database.</param>
		/// <param name="apiResources">A collection of API Resources to be saved to the database.</param>
		/// <param name="identityResources">A collection of Identity Resources to be saved to the database.</param>
		/// <param name="users">A collection of users to be saved to the database.</param>
		public static async Task InitializeDatabaseAsync(
			IServiceProvider serviceProvider,
			IEnumerable<Client> clients = default,
			IEnumerable<ApiScope> apiScopes = default,
			IEnumerable<ApiResource> apiResources = default,
			IEnumerable<IdentityResource> identityResources = default,
			IEnumerable<TestUser> users = default)
		{
			// Perform pending migrations for the IS4 operational database, the IS4 configuration database, and the application's database
			var operationalDbContext = serviceProvider.GetRequiredService<PersistedGrantDbContext>();
			var configsDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
			var usersDbContext = serviceProvider.GetRequiredService<AppDbContext>();

			var allDbContexts = new DbContext[] { operationalDbContext, configsDbContext, usersDbContext };
			await EnsureDatabasesCreatedAndMigrationsAppliedAsync(allDbContexts);


			// Save the test entities (ApiScope`s, Client`s, ApiResource`s, and IdentityResource`s) which are not yet present in the database
			var savedApiScopes = await SaveAllUnsavedTestEntities(
				apiScopes?.AsQueryable(),
				configsDbContext.ApiScopes,
				idSrvApiScope => idSrvApiScope.Name,
				efApiScope => efApiScope.Name,
				idSrvApiScope => idSrvApiScope.ToEntity());

			var savedClients = await SaveAllUnsavedTestEntities(
				clients?.AsQueryable(),
				configsDbContext.Clients,
				idSrvClient => idSrvClient.ClientId,
				efClient => efClient.ClientId,
				idSrvClient => idSrvClient.ToEntity());

			var savedApiResources = await SaveAllUnsavedTestEntities(
				apiResources?.AsQueryable(),
				configsDbContext.ApiResources,
				idSrvApiResource => idSrvApiResource.Name,
				efApiResource => efApiResource.Name,
				idSrvApiResource => idSrvApiResource.ToEntity());

			var savedIdentityResources = await SaveAllUnsavedTestEntities(
				identityResources?.AsQueryable(),
				configsDbContext.IdentityResources,
				idSrvIdentityResource => idSrvIdentityResource.Name,
				efIdentityResource => efIdentityResource.Name,
				idSrvIdentityResource => idSrvIdentityResource.ToEntity());

			var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
			var savedUsers = await SaveAllUnsavedTestEntities(
				users?.AsQueryable(),
				usersDbContext.Users,
				idSrvUser => idSrvUser.Username,
				efIdentityUser => efIdentityUser.UserName,
				idSvrTestUser => ConvertTestUserToIdentityUser(idSvrTestUser, userManager)
			);

			// Commit changes to the respective database(s)
			foreach (var curDbContext in allDbContexts)
				await curDbContext.SaveChangesAsync();
		}


		/// <summary>
		///     Clears all of the entries of the database, leaving it completely empty (while preserving its table structures).
		///     This method should be used with caution, as it might lead to data loss.
		/// </summary>
		/// <param name="serviceProvider">A service provider instance used to retrieve the required database-related services.</param>
		/// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
		public static async Task ClearDatabaseAsync(IServiceProvider serviceProvider)
		{
			// Perform pending migrations for the IS4 operational database, the IS4 configuration database, and the application's database
			var operationalDbContext = serviceProvider.GetRequiredService<PersistedGrantDbContext>();
			var configsDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
			var usersDbContext = serviceProvider.GetRequiredService<AppDbContext>();

			var allDbContexts = new DbContext[] { operationalDbContext, configsDbContext, usersDbContext };
			await EnsureDatabasesCreatedAndMigrationsAppliedAsync(allDbContexts);


			// Remove all entities
			configsDbContext.ApiScopes.RemoveRange(configsDbContext.ApiScopes);
			configsDbContext.Clients.RemoveRange(configsDbContext.Clients);
			configsDbContext.ApiResources.RemoveRange(configsDbContext.ApiResources);
			configsDbContext.IdentityResources.RemoveRange(configsDbContext.IdentityResources);
			usersDbContext.Users.RemoveRange(usersDbContext.Users);

			// Commit changes to the respective database(s)
			foreach (var curDbContext in allDbContexts)
				await curDbContext.SaveChangesAsync();
		}
	}
}