using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleOidcOauth.Data
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


		/// <summary>Stub collection of claims to be used as a placeholder for future improvements.</summary>
		/// <value>An empty collection of strings, which will be used as a placeholder for a future claims list.</value>
		public static readonly string[] EmptyClaimsCollection = new string[] {};


		/// <summary>An API Resource representing an API that deals with products.</summary>
		/// <returns>A pre-initialized <see cref={ApiResource} /> object representing an API that deals with products, and which can be used for testing/development purposes.</returns>
		public static readonly ApiResource ApiResourceProducts = new ApiResource(
			name: ScopeApiResourceProducts,
			displayName: "Products API",
			userClaims: EmptyClaimsCollection);


		/// <summary>An API Resource representing an API that deals with users management.</summary>
		/// <returns>A pre-initialized <see cref={ApiResource} /> object representing an API that deals with users management, and which can be used for testing/development purposes.</returns>
		public static readonly ApiResource ApiResourceUserManagement = new ApiResource(
			name: ScopeApiResourceUserManagement,
			displayName: "User management API",
			userClaims: EmptyClaimsCollection);


		/// <summary>An Identity Resource representing the basic information that can be obtained from a user.</summary>
		/// <returns>A pre-initialized <see cref={IdentityResource} /> object representing the basic claims of a user, and which can be used for testing/development purposes.</returns>
		public static readonly IdentityResource IdentityResourceBasicUserInfo = new IdentityResource(
			name: ScopeIdentityResourceBasicUserInfo,
			displayName: "Basic user information",
			userClaims: new [] {
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				JwtClaimTypes.GivenName,
			});


		/// <summary>An Identity Resource representing the private/confidential information that can be obtained from a user.</summary>
		/// <returns>A pre-initialized <see cref={IdentityResource} /> object representing the private/confidential claims of a user, and which can be used for testing/development purposes.</returns>
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
				new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
				new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
				new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
				new Claim("location", "somewhere")
			}
		};


		/// <summary>Test client configured for using the OAuth 2.0 Client Credentials flow.</summary>
		/// <returns>Returns a <see cref={Client} /> object configured for testing a Client Credentials flow.</returns>
		public static readonly Client ClientClientCredentialsFlow = new Client()
		{
			ClientId = "client-client-credentials",
			ClientName = "Client-credentials Client (Client Credentials Grant Type)",
			AllowedGrantTypes = GrantTypes.ClientCredentials,
			ClientSecrets = { new Secret("client-client-credentials-secret-123".Sha256()) },
			AllowedScopes = { ScopeIdentityResourceBasicUserInfo, ScopeApiResourceProducts },
		};


		/// <summary>Test client configured for using the OAuth 2.0 Authorization Code grant type on an MVC-like app.</summary>
		/// <returns>Returns a <see cref={Client} /> object configured for testing a Authorization Code grant type on an MVC-like app.</returns>
        public static readonly Client ClientMvc = new Client
        {
            ClientId = "client-mvc",
			ClientName = "MVC Client (Code Grant Type)",
            ClientSecrets = { new Secret("client-mvc-secret-123".Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,
            RequireConsent = false,
            RequirePkce = true,

            RedirectUris = { "http://localhost:5002/signin-oidc" },
            PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

            AllowedScopes = new List<string>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
				ScopeApiResourceProducts,
            }
        };


		/// <summary>Test client configured for using the OAuth 2.0 implicit grant type on an SPA-like app.</summary>
		/// <returns>Returns a <see cref={Client} /> object configured for testing a implicit grant type on an SPA-like app.</returns>
        public static Client ClientSpa { get; } = new Client
		{
			ClientId = "client-spa",
			ClientName = "SPA Client (Code Grant Type)",
			AllowedGrantTypes = GrantTypes.Code,
			RequirePkce = true,
			RequireClientSecret = false,

			RedirectUris =           { "http://localhost:5003/callback.html" },
			PostLogoutRedirectUris = { "http://localhost:5003/index.html" },
			AllowedCorsOrigins =     { "http://localhost:5003" },

			AllowedScopes =
			{
				IdentityServerConstants.StandardScopes.OpenId,
				IdentityServerConstants.StandardScopes.Profile,
				ScopeApiResourceProducts,
				ScopeApiResourceUserManagement,
			}
		};


		/// <summary>Collection of test Users.</summary>
		/// <value>A list of pre-initialized <see cref={TestUser} /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<TestUser> SampleUsers = new TestUser[]
		{
			UserAlice,
			UserBob,
		};


		/// <summary>Collection of test Clients.</summary>
		/// <value>A list of pre-initialized <see cref={Client} /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<Client> SampleClients = new Client[]
		{
			ClientClientCredentialsFlow,
			ClientMvc,
			ClientSpa,
		};


		/// <summary>Collection of test API Resources.</summary>
		/// <value>A list of pre-initialized <see cref={ApiResource} /> objects to be used for testing purposes.</value>
		public static readonly IEnumerable<ApiResource> SampleApiResources = new ApiResource[]
		{
			ApiResourceProducts,
			ApiResourceUserManagement,
		};


		/// <summary>Collection of test Identity Resources.</summary>
		/// <value>A list of pre-initialized <see cref={IdentityResource} /> objects to be used for testing purposes.</value>
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
		/// <param name="databaseCollection">A <see cref={DbSet} /> object used to manage entities in the target database.</param>
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
		///     Converts an IdentityServer <see cref={TestUser} /> object to the
		///     corresponding ASP.NET Core Identity user model representation, as used by this application.
		/// </summary>
		/// <param name="testUser">The <see cref={TestUser} /> object to be converted.</param>
		/// <returns>
		///     Returns an <see cref={IdentityUser} /> object containing the retrieved data
		///     which was converted from the input object.
		/// </returns>
		private static IdentityUser ConvertTestUserToIdentityUser(TestUser testUser, UserManager<IdentityUser> userManager)
		{
			var result = new IdentityUser()
			{
				UserName = testUser.Username,
				Email = testUser.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value,
				PhoneNumber = testUser.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.PhoneNumber)?.Value,
			};

			result.NormalizedEmail = userManager.NormalizeEmail(result.Email);
			result.NormalizedUserName = userManager.NormalizeName(result.UserName);
			result.PasswordHash = userManager.PasswordHasher.HashPassword(result, testUser.Password);
			return result;
		}



		// PUBLIC METHODS
		/// <summary>Initializes the database(s) with the test/development data.</summary>
		/// <param name="appBuilder">
		///    Reference to the application builder object, used for retrieving database-related services.
		/// </param>
		public static async Task InitializeDatabase(IApplicationBuilder appBuilder)
		{
			var serviceScopeFactory = appBuilder.ApplicationServices.GetService<IServiceScopeFactory>();
			using (var scope = serviceScopeFactory.CreateScope())
			{
				// Perform pending migrations for both the operational and the configuration databases
				var operationalDbContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
				var configsDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				var usersDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

				var allDbContexts = new DbContext [] { operationalDbContext, configsDbContext, usersDbContext };
				foreach (var curDbContext in allDbContexts)
				{
					await curDbContext.Database.EnsureCreatedAsync();

					var pendingMigrations = await curDbContext.Database.GetPendingMigrationsAsync();
					if (pendingMigrations.Any())
						await curDbContext.Database.MigrateAsync();
				}

				// Save the test entities (Client`s, ApiResource`s, and IdentityResource`s) which are not yet present in the database
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

				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
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
		}
	}
}