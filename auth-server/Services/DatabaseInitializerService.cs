using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Services
{
	/// <summary>Implementation for the <see cref="IDatabaseInitializerService" />.</summary>
	public class DatabaseInitializerService : IDatabaseInitializerService
	{
		// INSTANCE FIELDS
		/// <summary>Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</summary>
		private readonly ILogger<DatabaseInitializerService> _logger;
		/// <summary>Container-injected instance for the <see cref="PersistedGrantDbContext" /> service.</summary>
		private readonly PersistedGrantDbContext _persistedGrantDbContext;
		/// <summary>Container-injected instance for the <see cref="ConfigurationDbContext" /> service.</summary>
		private readonly ConfigurationDbContext _configurationDbContext;
		/// <summary>Container-injected instance for the <see cref="AppDbContext" /> service.</summary>
		private readonly AppDbContext _appDbContext;
		/// <summary>Container-injected instance for the <see cref="UserManager{TUser}" /> service.</summary>
		private readonly UserManager<IdentityUser> _userManager;





		// INSTANCE METHODS
		/// <summary>
		///     Utility method for saving the entities of a test collection into a database.
		///     This method will only save entities which are not yet present in the target database.
		/// </summary>
		/// <typeparam name="TIdentityServerModel">The type which represents the entities to be saved on IdentityServer's model realm.</typeparam>
		/// <typeparam name="TEntityFrameworkModel">The type which represents the entities to be saved on Entity Framework Core's model realm</typeparam>
		/// <typeparam name="TKey">The type of a key which will be used to verify which of the entities are already present in the target database.</typeparam>
		/// <typeparam name="TEntityFrameworkModelId">
		///     The type of a the property which represents the primary key for the Entity Framework
		///     Model (<typeparamref name="TEntityFrameworkModel"/>) being used.
		/// </typeparam>
		/// <param name="entities">
		///     <para>A collection containing all of the entities which can be persisted in the target database.</para>
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
		private async Task<IEnumerable<TEntityFrameworkModel>> SaveAllUnsavedTestEntities<TIdentityServerModel, TEntityFrameworkModel, TKey, TEntityFrameworkModelId>(
			IQueryable<TIdentityServerModel> entities,
			DbContext databaseContext,
			DbSet<TEntityFrameworkModel> databaseCollection,
			Expression<Func<TIdentityServerModel, TKey>> identityServerModelKeySelector,
			Expression<Func<TEntityFrameworkModel, TKey>> entityFrameworkModelKeySelector,
			Expression<Func<TEntityFrameworkModel, TEntityFrameworkModelId>> entityFrameworkModelIdSelector,
			Expression<Func<TIdentityServerModel, TEntityFrameworkModel>> convertToEntityFrameworkModel) where TEntityFrameworkModel : class
		{
			if (entities == null)
				return Enumerable.Empty<TEntityFrameworkModel>();


			// Separate entities to be created from entities to be updated in the database
			TKey[] entityKeys = entities
				.Select(identityServerModelKeySelector)
				.ToArray();
			var alreadyRegisteredEntityKeys = await databaseCollection
				.Select(entityFrameworkModelKeySelector)
				.Where(databaseEntityKey => entityKeys.Contains(databaseEntityKey))
				.ToArrayAsync();
			var notRegisteredEntityKeys = entityKeys
				.Except(alreadyRegisteredEntityKeys)
				.ToArray();


			// Create unregistered entities
			var entitiesToCreate = entities
				.WhereIn(notRegisteredEntityKeys, identityServerModelKeySelector)
				.Select(convertToEntityFrameworkModel)
				.ToList();
			await databaseCollection.AddRangeAsync(entitiesToCreate);
			try
			{
				await databaseContext.SaveChangesAsync();
			}
			catch (DbUpdateException ex)
			{
				// Catches possible "UNIQUE" constraint failures.
				// This will be logged as a warning (and not an error) bercause it might happen due to multiple instances of this application running in parallel,
				// and in this case it would not actually be an error.
				_logger.LogWarning(ex, $"Database initialization failure: failed to register one or more {typeof(TEntityFrameworkModel).Name} objects in the database.");
			}

			// Update entities that were already registered in the database
			var entityFrameworkModelIdPropertyName = ((MemberExpression) entityFrameworkModelIdSelector.Body).Member.Name;
			var propertiesToCopy = typeof(TEntityFrameworkModel)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(prop => prop.Name != entityFrameworkModelIdPropertyName)
				.ToArray();

			var entitiesToUpdate = await databaseCollection
				.WhereIn(alreadyRegisteredEntityKeys, entityFrameworkModelKeySelector)
				.ToListAsync();
			var compiledEntityFrameworkModelKeySelector = entityFrameworkModelKeySelector.Compile();
			var compiledIdentityServerModelKeySelector = identityServerModelKeySelector.Compile();
			var compiledConvertToEntityFrameworkModel = convertToEntityFrameworkModel.Compile();
			foreach (var entity in entitiesToUpdate)
			{
				var entityToUpdateKey = compiledEntityFrameworkModelKeySelector(entity);
				var newEntityInputData = entities.Single(updatedEntity => compiledIdentityServerModelKeySelector(updatedEntity).Equals(entityToUpdateKey));
				var newEntityData = compiledConvertToEntityFrameworkModel(newEntityInputData);

				foreach (var property in propertiesToCopy)
				{
					var newValue = property.GetValue(newEntityData);
					property.SetValue(entity, newValue);
				}
			}
			return entitiesToCreate;
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





		/// <summary>Constructor.</summary>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		/// <param name="persistedGrantDbContext">Container-injected instance for the <see cref="PersistedGrantDbContext" /> service.</param>
		/// <param name="configurationDbContext">Container-injected instance for the <see cref="ConfigurationDbContext" /> service.</param>
		/// <param name="appDbContext">Container-injected instance for the <see cref="AppDbContext" /> service.</param>
		/// <param name="userManager">Container-injected instance for the <see cref="UserManager{TUser}" /> service.</param>
		public DatabaseInitializerService(
			ILogger<DatabaseInitializerService> logger,
			PersistedGrantDbContext persistedGrantDbContext,
			ConfigurationDbContext configurationDbContext,
			AppDbContext appDbContext,
			UserManager<IdentityUser> userManager)
		{
			_logger = logger;
			_persistedGrantDbContext = persistedGrantDbContext;
			_configurationDbContext = configurationDbContext;
			_appDbContext = appDbContext;
			_userManager = userManager;
		}





		// INTERFACE IMPLEMENTATION: IDatabaseInitializerService
		/// <inheritdoc/>
		public async Task ClearDatabaseAsync()
		{
			// Perform pending migrations for the IS4 operational database, the IS4 configuration database, and the application's database
			var allDbContexts = new DbContext[] { _persistedGrantDbContext, _configurationDbContext, _appDbContext };
			await EnsureDatabasesCreatedAndMigrationsAppliedAsync(allDbContexts);

			// Remove all entities
			_configurationDbContext.ApiScopes.RemoveRange(_configurationDbContext.ApiScopes);
			_configurationDbContext.Clients.RemoveRange(_configurationDbContext.Clients);
			_configurationDbContext.ApiResources.RemoveRange(_configurationDbContext.ApiResources);
			_configurationDbContext.IdentityResources.RemoveRange(_configurationDbContext.IdentityResources);

			_appDbContext.Users.RemoveRange(_appDbContext.Users);

			// Commit changes to the respective database(s)
			foreach (var curDbContext in allDbContexts)
				await curDbContext.SaveChangesAsync();
		}


		/// <inheritdoc/>
		public async Task InitializeDatabaseAsync(
			IEnumerable<Client> clients = default,
			IEnumerable<ApiScope> apiScopes = default,
			IEnumerable<ApiResource> apiResources = default,
			IEnumerable<IdentityResource> identityResources = default,
			IEnumerable<TestUser> users = default)
		{
			// Perform pending migrations for the IS4 operational database, the IS4 configuration database, and the application's database
			var allDbContexts = new DbContext[] { _persistedGrantDbContext, _configurationDbContext, _appDbContext };
			await EnsureDatabasesCreatedAndMigrationsAppliedAsync(allDbContexts);


			// Save the test entities (ApiScope`s, Client`s, ApiResource`s, and IdentityResource`s) which are not yet present in the database
			var savedApiScopes = await SaveAllUnsavedTestEntities(
				apiScopes?.AsQueryable(),
				_configurationDbContext,
				_configurationDbContext.ApiScopes,
				idSrvApiScope => idSrvApiScope.Name,
				efApiScope => efApiScope.Name,
				efApiScope => efApiScope.Id,
				idSrvApiScope => idSrvApiScope.ToEntity());

			var savedClients = await SaveAllUnsavedTestEntities(
				clients?.AsQueryable(),
				_configurationDbContext,
				_configurationDbContext.Clients,
				idSrvClient => idSrvClient.ClientId,
				efClient => efClient.ClientId,
				efClient => efClient.Id,
				idSrvClient => idSrvClient.ToEntity());

			var savedApiResources = await SaveAllUnsavedTestEntities(
				apiResources?.AsQueryable(),
				_configurationDbContext,
				_configurationDbContext.ApiResources,
				idSrvApiResource => idSrvApiResource.Name,
				efApiResource => efApiResource.Name,
				efApiResource => efApiResource.Id,
				idSrvApiResource => idSrvApiResource.ToEntity());

			var savedIdentityResources = await SaveAllUnsavedTestEntities(
				identityResources?.AsQueryable(),
				_configurationDbContext,
				_configurationDbContext.IdentityResources,
				idSrvIdentityResource => idSrvIdentityResource.Name,
				efIdentityResource => efIdentityResource.Name,
				efIdentityResource => efIdentityResource.Id,
				idSrvIdentityResource => idSrvIdentityResource.ToEntity());

			var savedUsers = await SaveAllUnsavedTestEntities(
				users?.AsQueryable(),
				_appDbContext,
				_appDbContext.Users,
				idSrvUser => idSrvUser.Username,
				efIdentityUser => efIdentityUser.UserName,
				efIdentityUser => efIdentityUser.Id,
				idSvrTestUser => idSvrTestUser.ConvertToIdentityUser(_userManager)
			);

			// Commit changes to the respective database(s)
			foreach (var curDbContext in allDbContexts)
				await curDbContext.SaveChangesAsync();
		}
	}
}