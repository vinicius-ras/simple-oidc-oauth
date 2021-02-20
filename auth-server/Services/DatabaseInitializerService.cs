using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Extensions;
using SimpleOidcOauth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
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
		/// <summary>Container-injected instance for the <see cref="IMapper" /> service.</summary>
		private readonly IMapper _mapper;





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
		private async Task<SaveEntitiesResult<TEntityFrameworkModel>> SaveAllUnsavedTestEntities<TIdentityServerModel, TEntityFrameworkModel, TKey, TEntityFrameworkModelId>(
			IQueryable<TIdentityServerModel> entities,
			DbContext databaseContext,
			DbSet<TEntityFrameworkModel> databaseCollection,
			Expression<Func<TIdentityServerModel, TKey>> identityServerModelKeySelector,
			Expression<Func<TEntityFrameworkModel, TKey>> entityFrameworkModelKeySelector,
			Expression<Func<TEntityFrameworkModel, TEntityFrameworkModelId>> entityFrameworkModelIdSelector,
			Expression<Func<TIdentityServerModel, TEntityFrameworkModel>> convertToEntityFrameworkModel) where TEntityFrameworkModel : class
		{
			if (entities == null)
				return SaveEntitiesResult<TEntityFrameworkModel>.GetEmptyInstance();

			try
			{
				// Start a transaction
				await databaseContext.Database.BeginTransactionAsync();

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
				await databaseContext.SaveChangesAsync();

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
					var newEntityInputData = entities.Single(updatedEntity => compiledIdentityServerModelKeySelector(updatedEntity) != null && compiledIdentityServerModelKeySelector(updatedEntity).Equals(entityToUpdateKey));
					var newEntityData = compiledConvertToEntityFrameworkModel(newEntityInputData);

					foreach (var property in propertiesToCopy)
					{
						var newValue = property.GetValue(newEntityData);
						property.SetValue(entity, newValue);
					}
				}

				// Commit the transaction and end
				await databaseContext.Database.CommitTransactionAsync();
				var result = new SaveEntitiesResult<TEntityFrameworkModel>
				{
					InsertedEntities = entitiesToCreate,
					UpdatedEntities = entitiesToUpdate,
				};
				return result;
			}
			catch (DbUpdateException ex)
			{
				// Catches possible failures (e.g., violated "UNIQUE" constraints).
				// This will be logged as a warning (and not an error) bercause it might happen due to multiple instances of this
				// application running in parallel (e.g., in a cluster), and in this case it would not actually be an error.

				// In this case, a warning will be issued, the transaction will be rolled back, and involved entities will be cleared from the change tracker
				_logger.LogWarning(ex, $"Database initialization failure: failed to register one or more {typeof(TEntityFrameworkModel).Name} objects in the database.");
				await databaseContext.Database.RollbackTransactionAsync();

				foreach (var entityEntry in ex.Entries)
				{
					// Revert any entity changes
					entityEntry.CurrentValues.SetValues(entityEntry.OriginalValues);
					entityEntry.State = EntityState.Detached;
					entityEntry.DetectChanges();
				}
			}
			return SaveEntitiesResult<TEntityFrameworkModel>.GetEmptyInstance();
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


		/// <summary>
		///     Utility method for eliminating duplicate elements in a given list.
		///     Mainly used to eliminate duplicate elements in a given navigation property, avoiding
		///     that multiple application restarts duplicate entries in the database.
		/// </summary>
		/// <param name="entitiesList">The list whose duplicate elements must be eliminated.</param>
		/// <param name="discriminatorKeys">
		///     A function used to extract one or more discriminator keys from the given navigation property elements.
		///     If two elements return the same set of discriminator key values, then the elements are considered to be duplicate.
		/// </param>
		/// <typeparam name="TListElement">The type of the elements contained in the list to be deduplicated.</typeparam>
		/// <typeparam name="TDiscriminatorKey">The type of the discriminator key. Notice that this might be a composite key.</typeparam>
		/// <returns>Returns an enumerable collection containing the removed elements.</returns>
		private IEnumerable<TListElement> EliminateDuplicates<TListElement, TDiscriminatorKey>(
			List<TListElement> entitiesList,
			Func<TListElement, TDiscriminatorKey> discriminatorKeys)
		{
			var entitiesToRemove = entitiesList
				.GroupBy(discriminatorKeys)
				.SelectMany(group => group.Skip(1));
			var removedElements = entitiesToRemove.ToList();

			foreach (var entityToRemove in removedElements)
				entitiesList.Remove(entityToRemove);
			return removedElements;
		}





		/// <summary>Constructor.</summary>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		/// <param name="persistedGrantDbContext">Container-injected instance for the <see cref="PersistedGrantDbContext" /> service.</param>
		/// <param name="configurationDbContext">Container-injected instance for the <see cref="ConfigurationDbContext" /> service.</param>
		/// <param name="appDbContext">Container-injected instance for the <see cref="AppDbContext" /> service.</param>
		/// <param name="userManager">Container-injected instance for the <see cref="UserManager{TUser}" /> service.</param>
		/// <param name="mapper">Container-injected instance for the <see cref="IMapper" /> service.</param>
		public DatabaseInitializerService(
			ILogger<DatabaseInitializerService> logger,
			PersistedGrantDbContext persistedGrantDbContext,
			ConfigurationDbContext configurationDbContext,
			AppDbContext appDbContext,
			UserManager<IdentityUser> userManager,
			IMapper mapper)
		{
			_logger = logger;
			_persistedGrantDbContext = persistedGrantDbContext;
			_configurationDbContext = configurationDbContext;
			_appDbContext = appDbContext;
			_userManager = userManager;
			_mapper = mapper;
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
		public async Task<DatabaseInitializationResult> InitializeDatabaseAsync(
			IEnumerable<Client> clients = default,
			IEnumerable<ApiScope> apiScopes = default,
			IEnumerable<ApiResource> apiResources = default,
			IEnumerable<IdentityResource> identityResources = default,
			IEnumerable<TestUser> users = default)
		{
			// Perform pending migrations for the IS4 operational database, the IS4 configuration database, and the application's database
			var allDbContexts = new DbContext[] { _persistedGrantDbContext, _configurationDbContext, _appDbContext };
			await EnsureDatabasesCreatedAndMigrationsAppliedAsync(allDbContexts);


			// Save the test entities (ApiScope`s, Client`s, ApiResource`s, IdentityResource`s, and users) which are not yet present in the database
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
				idSvrTestUser => idSvrTestUser.ConvertToIdentityUser(_mapper, _userManager)
			);


			// For the users, we must also save their respective user-claims
			var allClaimsToSave = new List<IdentityUserClaim<string>>();
			foreach (var testUser in users ?? Enumerable.Empty<TestUser>())
			{
				var userEntity = savedUsers.AllAffectedEntities.Single(savedUser => savedUser.UserName == testUser.Username);
				var userClaims = testUser.Claims ?? Enumerable.Empty<Claim>();
				var claimsToSave = userClaims.Select(claim => new IdentityUserClaim<string> {
					UserId = userEntity.Id,
					ClaimType = claim.Type,
					ClaimValue = claim.Value,
				});
				allClaimsToSave.AddRange(claimsToSave);
			}
			await _appDbContext.UserClaims.AddRangeAsync(allClaimsToSave);


			// Commit changes to the respective database(s)
			foreach (var curDbContext in allDbContexts)
				await curDbContext.SaveChangesAsync();


			// Deduplicate entity-related data: ApiScope
			var affectedApiScopeIds = savedApiScopes.AllAffectedEntities
				.Select(apiScope => apiScope.Name)
				.Distinct();
			var affectedApiScopesInDatabase = await _configurationDbContext.ApiScopes
				.Include(apiScope => apiScope.UserClaims)
				.Include(apiScope => apiScope.Properties)
				.AsSplitQuery()
				.WhereIn(affectedApiScopeIds, apiScope => apiScope.Name)
				.ToListAsync();
			foreach (var dbApiScope in affectedApiScopesInDatabase)
			{
				EliminateDuplicates(dbApiScope.UserClaims, userClaim => new { userClaim.ScopeId, userClaim.Type });
				EliminateDuplicates(dbApiScope.Properties, property => new { property.ScopeId, property.Key, property.Value });
			}

			// Deduplicate entity-related data: ApiResource
			var affectedApiResourceIds = savedApiResources.AllAffectedEntities
				.Select(apiResource => apiResource.Name)
				.Distinct();
			var affectedApiResourcesInDatabase = await _configurationDbContext.ApiResources
				.Include(apiResource => apiResource.UserClaims)
				.Include(apiResource => apiResource.Properties)
				.Include(apiResource => apiResource.Scopes)
				.Include(apiResource => apiResource.Secrets)
				.AsSplitQuery()
				.WhereIn(affectedApiResourceIds, apiResource => apiResource.Name)
				.ToListAsync();
			foreach (var dbApiResource in affectedApiResourcesInDatabase)
			{
				EliminateDuplicates(dbApiResource.UserClaims, userClaim => new { userClaim.ApiResourceId, userClaim.Type });
				EliminateDuplicates(dbApiResource.Properties, property => new { property.ApiResourceId, property.Key, property.Value });
			}

			// Deduplicate entity-related data: IdentityResource
			var affectedIdentityResourceIds = savedIdentityResources.AllAffectedEntities
				.Select(identityResource => identityResource.Name)
				.Distinct();
			var affectedIdentityResourcesInDatabase = await _configurationDbContext.IdentityResources
				.Include(identityResource => identityResource.UserClaims)
				.Include(identityResource => identityResource.Properties)
				.AsSplitQuery()
				.WhereIn(affectedIdentityResourceIds, identityResource => identityResource.Name)
				.ToListAsync();
			foreach (var dbIdentityResource in affectedIdentityResourcesInDatabase)
			{
				EliminateDuplicates(dbIdentityResource.UserClaims, userClaim => new { userClaim.IdentityResourceId, userClaim.Type });
				EliminateDuplicates(dbIdentityResource.Properties, property => new { property.IdentityResourceId, property.Key, property.Value });
			}

			// Deduplicate entity-related data: Identity User Claims
			var affectedClaimIds = allClaimsToSave
				.Select(claim => claim.UserId)
				.Distinct();
			var affectedClaimsInDatabase = await _appDbContext.UserClaims
				.WhereIn(affectedClaimIds, claim => claim.UserId)
				.ToListAsync();
			var duplicatesToEliminate = EliminateDuplicates(affectedClaimsInDatabase, userClaim => new { userClaim.UserId, userClaim.ClaimType, userClaim.ClaimValue });

			_appDbContext.UserClaims.RemoveRange(duplicatesToEliminate);


			// Deduplicate entity-related data: Client
			var affectedClientIds = savedClients.AllAffectedEntities
				.Select(client => client.ClientId)
				.Distinct();
			var affectedClientsInDatabase = await _configurationDbContext.Clients
				.Include(client => client.AllowedCorsOrigins)
				.Include(client => client.IdentityProviderRestrictions)
				.Include(client => client.Claims)
				.Include(client => client.Properties)
				.Include(client => client.AllowedScopes)
				.Include(client => client.ClientSecrets)
				.Include(client => client.AllowedGrantTypes)
				.Include(client => client.RedirectUris)
				.Include(client => client.PostLogoutRedirectUris)
				.AsSplitQuery()
				.WhereIn(affectedClientIds, client => client.ClientId)
				.ToListAsync();
			foreach (var dbClient in affectedClientsInDatabase)
			{
				EliminateDuplicates(dbClient.AllowedCorsOrigins, corsOrigin => corsOrigin.Origin);
				EliminateDuplicates(dbClient.ClientSecrets, secret => secret.Value);
				EliminateDuplicates(dbClient.IdentityProviderRestrictions, restriction => restriction.Provider);
				EliminateDuplicates(dbClient.Claims, claim => new { claim.Type, claim.Value });
				EliminateDuplicates(dbClient.Properties, property => new { property.Key, property.Value });
				EliminateDuplicates(dbClient.AllowedScopes, scope => scope.Scope);
				EliminateDuplicates(dbClient.AllowedGrantTypes, grantType => grantType.GrantType);
				EliminateDuplicates(dbClient.RedirectUris, uri => uri.RedirectUri);
				EliminateDuplicates(dbClient.PostLogoutRedirectUris, uri => uri.PostLogoutRedirectUri);
			}

			// Save chages from the duplicate elimination routines
			await Task.WhenAll(
				_appDbContext.SaveChangesAsync(),
				_configurationDbContext.SaveChangesAsync()
			);

			// Return the results
			var result = new DatabaseInitializationResult
			{
				InsertedApiScopes = savedApiScopes.InsertedEntities,
				InsertedClients = savedClients.InsertedEntities,
				InsertedApiResources = savedApiResources.InsertedEntities,
				InsertedIdentityResources = savedIdentityResources.InsertedEntities,
				InsertedUsers = savedUsers.InsertedEntities,

				UpdatedApiScopes = savedApiScopes.UpdatedEntities,
				UpdatedClients = savedClients.UpdatedEntities,
				UpdatedApiResources = savedApiResources.UpdatedEntities,
				UpdatedIdentityResources = savedIdentityResources.UpdatedEntities,
				UpdatedUsers = savedUsers.UpdatedEntities,
			};

			return result;
		}





		// NESTED STRUCTURES
		/// <summary>A structure describing the results for a call to <see cref="SaveAllUnsavedTestEntities{TIdentityServerModel, TEntityFrameworkModel, TKey, TEntityFrameworkModelId}(IQueryable{TIdentityServerModel}, DbContext, DbSet{TEntityFrameworkModel}, Expression{Func{TIdentityServerModel, TKey}}, Expression{Func{TEntityFrameworkModel, TKey}}, Expression{Func{TEntityFrameworkModel, TEntityFrameworkModelId}}, Expression{Func{TIdentityServerModel, TEntityFrameworkModel}})"/>.</summary>
		/// <typeparam name="TEntity">The type of entity for which the save operation was performed.</typeparam>
		private struct SaveEntitiesResult<TEntity>
		{
			// INSTANCE PROPERTIES
			/// <summary>Collection of entities that have been inserted into the database.</summary>
			/// <value>An enumerable collection of entities which were inserted into the database during the save operation.</value>
			public IEnumerable<TEntity> InsertedEntities { get; init; }
			/// <summary>Collection of entities that have been updated in the database.</summary>
			/// <value>An enumerable collection of entities which were updated in the database during the save operation.</value>
			public IEnumerable<TEntity> UpdatedEntities { get; init; }
			/// <summary>An enumerable collection containing all of the <see cref="InsertedEntities"/> and <see cref="UpdatedEntities"/> present in this object.</summary>
			/// <returns>
			///     Returns a concatenation enumerable (<see cref="Enumerable.Concat{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>) between
			///     the <see cref="InsertedEntities"/> and <see cref="UpdatedEntities"/> collections.
			/// </returns>
			public IEnumerable<TEntity> AllAffectedEntities => InsertedEntities.Concat(UpdatedEntities);





			// STATIC METHODS
			/// <summary>
			///     Retrieves a new instance of an empty <see cref="SaveEntitiesResult{TEntity}"/>.
			///     This instance will have empty (not <c>null</c>!) collections in each of its members.
			/// </summary>
			/// <returns>Returns a new, empty instance of a <see cref="SaveEntitiesResult{TEntity}"/>.</returns>
			public static SaveEntitiesResult<TEntity> GetEmptyInstance() => new SaveEntitiesResult<TEntity>
			{
				InsertedEntities = Enumerable.Empty<TEntity>(),
				UpdatedEntities = Enumerable.Empty<TEntity>(),
			};
		}
	}
}