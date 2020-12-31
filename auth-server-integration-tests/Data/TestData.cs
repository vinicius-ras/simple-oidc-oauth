using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleOidcOauth.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Tests.Integration.Data
{
	/// <summary>A class containing constants and sample data to be used for testing/development purposes.</summary>
	internal static class TestData
	{
		// CONSTANTS / READ-ONLY
		/// <summary>Scope for an Identity Resource which provides basic informations about users.</summary>
		public const string ScopeIdentityResourceBasicUserInfo = "basic-user-info";


		/// <summary>Scope for an Identity Resource which provides confidential informations about users.</summary>
		public const string ScopeIdentityResourceConfidentialUserInfo = "confidential-user-info";


		/// <summary>Scope for an Api Resource which provides access to a "products" API.</summary>
		public const string ScopeApiResourceProducts = "products-api";


		/// <summary>Scope for an Api Resource which provides access to a "user management" API.</summary>
		public const string ScopeApiResourceUserManagement = "user-management-api";


		/// <summary>A fake plain text password to be used for the "Client Credentials Flow" client.</summary>
		public static readonly string PlainTextPasswordClientClientCredentialsFlow = "b22787170fdc45bfa22bbfc12e8776b2";


		/// <summary>A fake plain text password to be used for the "Resource Owner Password Flow" client.</summary>
		public static readonly string PlainTextPasswordClientResourceOwnerPasswordFlow = "930151c01fde4c7e9fe91ce56638e952";


		/// <summary>A fake plain text password to be used for the "Authorization Code (without PKCE)" client.</summary>
		public static readonly string PlainTextPasswordClientAuthorizationCodeFlowWithoutPkce = "e42b6cf8a482444385c62e5ee440f910";


		/// <summary>A fake plain text password to be used for the "Authorization Code (with PKCE)" client.</summary>
		public static readonly string PlainTextPasswordClientAuthorizationCodeFlowWithPkce = "74d8fb7b96dc4760846b432c7397f629";


		/// <summary>Stub collection of claims to be used as a placeholder for future improvements.</summary>
		/// <value>An empty collection of strings, which will be used as a placeholder for a future claims list.</value>
		public static readonly string[] EmptyClaimsCollection = new string[] {};


		/// <summary>An API Resource representing an API that deals with products.</summary>
		/// <returns>A pre-initialized <see cref="ApiResource" /> object representing an API that deals with products, and which can be used for testing/development purposes.</returns>
		public static readonly ApiResource ApiResourceProducts = new ApiResource(
			name: ScopeApiResourceProducts,
			displayName: "Products API",
			userClaims: EmptyClaimsCollection);


		/// <summary>An API Resource representing an API that deals with users management.</summary>
		/// <returns>A pre-initialized <see cref="ApiResource" /> object representing an API that deals with users management, and which can be used for testing/development purposes.</returns>
		public static readonly ApiResource ApiResourceUserManagement = new ApiResource(
			name: ScopeApiResourceUserManagement,
			displayName: "User management API",
			userClaims: EmptyClaimsCollection);


		/// <summary>An Identity Resource representing the basic information that can be obtained from a user.</summary>
		/// <returns>A pre-initialized <see cref="IdentityResource" /> object representing the basic claims of a user, and which can be used for testing/development purposes.</returns>
		public static readonly IdentityResource IdentityResourceBasicUserInfo = new IdentityResource(
			name: ScopeIdentityResourceBasicUserInfo,
			displayName: "Basic user information",
			userClaims: new [] {
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				JwtClaimTypes.GivenName,
			});


		/// <summary>An Identity Resource representing the private/confidential information that can be obtained from a user.</summary>
		/// <returns>A pre-initialized <see cref="IdentityResource" /> object representing the private/confidential claims of a user, and which can be used for testing/development purposes.</returns>
		public static readonly IdentityResource IdentityResourceConfidentialUserInfo = new IdentityResource(
			name: ScopeIdentityResourceConfidentialUserInfo,
			displayName: "Private/confidential user information",
			userClaims: new [] {
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				JwtClaimTypes.GivenName,
				JwtClaimTypes.Name,
				JwtClaimTypes.FamilyName,
				JwtClaimTypes.Email,
				JwtClaimTypes.EmailVerified,
				JwtClaimTypes.WebSite,
				JwtClaimTypes.Address,
			});


		/// <summary>Test user "Alice".</summary>
		/// <value>An object describing a mock user called "Alice".</value>
		public static readonly TestUser UserAlice = new TestUser{
			SubjectId = "818727",
			Username = "alice",
			Password = "alice123",
			Claims =
			{
				new Claim(JwtClaimTypes.Name, "Alice Smith"),
				new Claim(JwtClaimTypes.GivenName, "Alice"),
				new Claim(JwtClaimTypes.FamilyName, "Smith"),
				new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
				new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
				new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
				new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
			}
		};


		/// <summary>Test user "Bob".</summary>
		/// <value>An object describing a mock user called "Bob".</value>
		public static readonly TestUser UserBob = new TestUser{
			SubjectId = "88421113",
			Username = "bob",
			Password = "bob123",
			Claims =
			{
				new Claim(JwtClaimTypes.Name, "Bob Smith"),
				new Claim(JwtClaimTypes.GivenName, "Bob"),
				new Claim(JwtClaimTypes.FamilyName, "Smith"),
				new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
				new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean),
				new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
				new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
				new Claim("location", "somewhere")
			}
		};


		/// <summary>Test client configured for using the OAuth 2.0 Client Credentials flow.</summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Client Credentials flow.</returns>
		public static readonly Client ClientClientCredentialsFlow = new Client()
		{
			ClientId = "client-client-credentials-flow",
			ClientName = "Client Credentials Flow Client",
			AllowedGrantTypes = GrantTypes.ClientCredentials,
			ClientSecrets = { new Secret(PlainTextPasswordClientClientCredentialsFlow.Sha256()) },
			AllowedScopes = { ScopeApiResourceUserManagement, ScopeApiResourceProducts },
		};


		/// <summary>Test client configured for using the OAuth 2.0 Resource Owner Password flow.</summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Resource Owner Password flow.</returns>
		public static readonly Client ClientResourceOwnerPasswordFlow = new Client()
		{
			ClientId = "client-resource-owner-password-flow",
			ClientName = "Resource Owner Password Flow Client",
			AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
			ClientSecrets = { new Secret(PlainTextPasswordClientResourceOwnerPasswordFlow.Sha256()) },
			AllowedScopes = { ScopeApiResourceUserManagement },
		};


		/// <summary>Test client configured for using the OAuth 2.0 Authorization Code flow (without PKCE).</summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Authorization Code flow (without PKCE).</returns>
        public static readonly Client ClientAuthorizationCodeFlowWithoutPkce = new Client
        {
            ClientId = "client-authorization-code-flow-no-pkce",
			ClientName = "Authorization Code Flow Client (Without PKCE)",
            ClientSecrets = { new Secret(PlainTextPasswordClientAuthorizationCodeFlowWithoutPkce.Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,
            RequireConsent = false,
            RequirePkce = false,

			AllowedCorsOrigins = { "https://localhost:5002" },
            RedirectUris = { "https://localhost:5002/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

            AllowedScopes = new List<string>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
				ScopeApiResourceProducts,
            }
        };


		/// <summary>Test client configured for using the OAuth 2.0 Authorization Code flow (with PKCE).</summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Authorization Code flow (with PKCE).</returns>
        public static readonly Client ClientAuthorizationCodeFlowWithPkce = new Client
        {
            ClientId = "client-authorization-code-flow-with-pkce",
			ClientName = "Authorization Code Flow Client (With PKCE)",
            ClientSecrets = { new Secret(PlainTextPasswordClientAuthorizationCodeFlowWithPkce.Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,
            RequireConsent = false,
            RequirePkce = true,

			AllowedCorsOrigins = { "https://localhost:5003" },
            RedirectUris = { "https://localhost:5003/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

            AllowedScopes = new List<string>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
				ScopeApiResourceProducts,
            }
        };


		/// <summary>
		///     Test client configured for using the OAuth 2.0 Implicit Flow, wich is configured to be able to request
		///     both Access Tokens and Identity Tokens.
		/// </summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Implicit Flow.</returns>
        public static Client ClientImplicitFlowAccessAndIdTokens { get; } = new Client
		{
			ClientId = "client-implicit-flow-access-and-id-tokens",
			ClientName = "Implicit Flow Client (Access Tokens and ID Tokens)",
			AllowedGrantTypes = GrantTypes.Implicit,
			AllowAccessTokensViaBrowser = true,
			RequireClientSecret = false,

			RedirectUris =           { "http://localhost:5004/callback.html" },
			PostLogoutRedirectUris = { "http://localhost:5004/index.html" },
			AllowedCorsOrigins =     { "http://localhost:5004" },

			AllowedScopes =
			{
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				ScopeApiResourceUserManagement,
			}
		};


		/// <summary>
		///     Test client configured for using the OAuth 2.0 Implicit Flow, wich is configured to be able to request
		///     only Access Tokens (not Identity Tokens, nor Authorization Codes).
		/// </summary>
		/// <returns>Returns a <see cref="Client" /> object configured for testing a Implicit Flow.</returns>
        public static Client ClientImplicitFlowAccessTokensOnly { get; } = new Client
		{
			ClientId = "client-implicit-flow-access-tokens-only",
			ClientName = "Implicit Flow Client (Access Tokens only)",
			AllowedGrantTypes = GrantTypes.Implicit,
			AllowAccessTokensViaBrowser = true,
			RequireClientSecret = false,

			RedirectUris =           { "http://localhost:5005/callback.html" },
			PostLogoutRedirectUris = { "http://localhost:5005/index.html" },
			AllowedCorsOrigins =     { "http://localhost:5005" },

			AllowedScopes =
			{
				ScopeApiResourceUserManagement,
			}
		};


		/// <summary>Collection of test Users.</summary>
		/// <value>A list of pre-initialized <see cref="TestUser" /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<TestUser> SampleUsers = new TestUser[]
		{
			UserAlice,
			UserBob,
		};


		/// <summary>Collection of test Clients.</summary>
		/// <value>A list of pre-initialized <see cref="Client" /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<Client> SampleClients = new Client[]
		{
			ClientClientCredentialsFlow,
			ClientResourceOwnerPasswordFlow,
			ClientAuthorizationCodeFlowWithPkce,
			ClientAuthorizationCodeFlowWithoutPkce,
			ClientImplicitFlowAccessTokensOnly,
			ClientImplicitFlowAccessAndIdTokens,
		};


		/// <summary>Collection of test API Scopes.</summary>
		/// <value>A list of pre-initialized <see cref="ApiScope"/> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<ApiScope> SampleApiScopes = new ApiScope[]
		{
			new ApiScope(ScopeApiResourceProducts, "Products API Scope"),
			new ApiScope(ScopeApiResourceUserManagement, "User Management API Scope"),
		};


		/// <summary>Collection of test API Resources.</summary>
		/// <value>A list of pre-initialized <see cref="ApiResource" /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<ApiResource> SampleApiResources = new ApiResource[]
		{
			ApiResourceProducts,
			ApiResourceUserManagement,
		};


		/// <summary>Collection of test Identity Resources.</summary>
		/// <value>A list of pre-initialized <see cref="IdentityResource" /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<IdentityResource> SampleIdentityResources = new IdentityResource[]
		{
			new IdentityResources.OpenId(),
			new IdentityResources.Profile(),
			IdentityResourceBasicUserInfo,
			IdentityResourceConfidentialUserInfo,
		};





		// PRIVATE METHODS
		/// <summary>
		///     Utility method for saving the entities of a test collection into a database.
		///     This method will only save entities which are not yet present in the target database.
		/// </summary>
		/// <typeparam name="TIdentityServerModel">The type which represents the entities to be saved on IdentityServer's model realm.</typeparam>
		/// <typeparam name="TEntityFrameworkModel">The type which represents the entities to be saved on Entity Framework Core's model realm</typeparam>
		/// <typeparam name="TKey">The type of a key which will be used to verify which of the entities are already present in the target database.</typeparam>
		/// <param name="testEntities">A collection containing all of the test entities which can be persisted in the target database.</param>
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
		/// <param name="serviceProvider">
		///    A service provider instance used to retrieve the required database-related services.
		/// </param>
		public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
		{
			// Perform pending migrations for the IS4 operational database, the IS4 configuration database, and the application's database
			var operationalDbContext = serviceProvider.GetRequiredService<PersistedGrantDbContext>();
			var configsDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
			var usersDbContext = serviceProvider.GetRequiredService<AppDbContext>();

			var allDbContexts = new DbContext[] { operationalDbContext, configsDbContext, usersDbContext };
			await EnsureDatabasesCreatedAndMigrationsAppliedAsync(allDbContexts);


			// Save the test entities (ApiScope`s, Client`s, ApiResource`s, and IdentityResource`s) which are not yet present in the database
			var savedApiScopes = await SaveAllUnsavedTestEntities(
				TestData.SampleApiScopes.AsQueryable(),
				configsDbContext.ApiScopes,
				idSrvApiScope => idSrvApiScope.Name,
				efApiScope => efApiScope.Name,
				idSrvApiScope => idSrvApiScope.ToEntity());

			var savedClients = await SaveAllUnsavedTestEntities(
				TestData.SampleClients.AsQueryable(),
				configsDbContext.Clients,
				idSrvClient => idSrvClient.ClientId,
				efClient => efClient.ClientId,
				idSrvClient => idSrvClient.ToEntity());

			var savedApiResources = await SaveAllUnsavedTestEntities(
				TestData.SampleApiResources.AsQueryable(),
				configsDbContext.ApiResources,
				idSrvApiResource => idSrvApiResource.Name,
				efApiResource => efApiResource.Name,
				idSrvApiResource => idSrvApiResource.ToEntity());

			var savedIdentityResources = await SaveAllUnsavedTestEntities(
				TestData.SampleIdentityResources.AsQueryable(),
				configsDbContext.IdentityResources,
				idSrvIdentityResource => idSrvIdentityResource.Name,
				efIdentityResource => efIdentityResource.Name,
				idSrvIdentityResource => idSrvIdentityResource.ToEntity());

			var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
			var savedUsers = await SaveAllUnsavedTestEntities(
				TestData.SampleUsers.AsQueryable(),
				usersDbContext.Users,
				idSrvUser => idSrvUser.Username,
				efIdentityUser => efIdentityUser.UserName,
				idSvrTestUser => ConvertTestUserToIdentityUser(idSvrTestUser, userManager)
			);

			// Commit changes to the respective database(s)
			foreach (var curDbContext in allDbContexts)
				await curDbContext.SaveChangesAsync();
		}


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