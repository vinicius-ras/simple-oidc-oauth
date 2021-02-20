using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.Extensions;
using SimpleOidcOauth.Services;
using SimpleOidcOauth.Tests.Integration.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Tests.Integration.Controllers
{
	/// <summary>A special controller used to test the <see cref="IDatabaseInitializerService"/>.</summary>
	[ApiController]
	public class TestDatabaseInitializerServiceController : ControllerBase
	{
		// CONSTANTS
		/// <summary>Path to the endpoint used for initializing the test database with specific data.</summary>
		public const string InitializeDatabaseEndpoint = "/database-initializer-tests/initialize-database-b69c46a945604ece86d290aba1809dc0";
		/// <summary>Path to the endpoint used for fully clearing the test database.</summary>
		public const string ClearDatabaseEndpoint = "/database-initializer-tests/clear-database-c2966660ed4b406aab7f29d804187443";
		/// <summary>Path to the endpoint used for retrieving all of the data from the test database.</summary>
		public const string GetAllRegisteredDataEndpoint = "/database-initializer-tests/get-all-data-c2966660ed4b406aab7f29d804187443";





		// INSTANCE FIELDS
		/// <summary>Container-injected instance for the <see cref="IDatabaseInitializerService" /> service.</summary>
		private readonly IDatabaseInitializerService _databaseInitializerService;
		/// <summary>Container-injected instance for the <see cref="ConfigurationDbContext" /> service.</summary>
		private readonly ConfigurationDbContext _configurationDbContext;
		/// <summary>Container-injected instance for the <see cref="AppDbContext" /> service.</summary>
		private readonly AppDbContext _appDbContext;
		/// <summary>Container-injected instance for the <see cref="IMapper" /> service.</summary>
		private readonly IMapper _mapper;
		/// <summary>Container-injected instance for the <see cref="UserManager{TUser}" /> service.</summary>
		private readonly UserManager<IdentityUser> _userManager;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="databaseInitializerService">Container-injected instance for the <see cref="IDatabaseInitializerService" /> service.</param>
		/// <param name="configurationDbContext">Container-injected instance for the <see cref="ConfigurationDbContext" /> service.</param>
		/// <param name="appDbContext">Container-injected instance for the <see cref="AppDbContext" /> service.</param>
		/// <param name="userManager">Container-injected instance for the <see cref="UserManager{TUser}" /> service.</param>
		/// <param name="mapper">Container-injected instance for the <see cref="IMapper" /> service.</param>
		public TestDatabaseInitializerServiceController(
			IDatabaseInitializerService databaseInitializerService,
			ConfigurationDbContext configurationDbContext,
			AppDbContext appDbContext,
			UserManager<IdentityUser> userManager,
			IMapper mapper)
		{
			_databaseInitializerService = databaseInitializerService;
			_configurationDbContext = configurationDbContext;
			_appDbContext = appDbContext;
			_userManager = userManager;
			_mapper = mapper;
		}


		/// <summary>
		///     Endpoint used to test the <see cref="IDatabaseInitializerService.InitializeDatabaseAsync(IEnumerable{Client}, IEnumerable{ApiScope}, IEnumerable{ApiResource}, IEnumerable{IdentityResource}, IEnumerable{TestUser})"/> method.
		///     Any POSTed data will be used in a call to the target method.
		/// </summary>
		/// <param name="input">The data to be used to initialize the test database.</param>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>This endpoint should always return an HTTP 200 (Ok) Status Code.</para>
		/// </returns>
		[HttpPost(InitializeDatabaseEndpoint)]
		public async Task<IActionResult> InitializeDatabase([FromBody] TestDatabaseInitializerInputModel input)
		{
			var clients = input.Clients?.Select(serializableClient => _mapper.Map<Client>(serializableClient));
			var apiScopes = input.ApiScopes?.Select(serializableApiScope => _mapper.Map<ApiScope>(serializableApiScope));
			var apiResources = input.ApiResources?.Select(serializableApiResource => _mapper.Map<ApiResource>(serializableApiResource));
			var identityResources = input.IdentityResources?.Select(serializableIdentityResource => _mapper.Map<IdentityResource>(serializableIdentityResource));
			var users = input.Users?.Select(serializableUser => _mapper.Map<TestUser>(serializableUser));

			await _databaseInitializerService.InitializeDatabaseAsync(
				clients,
				apiScopes,
				apiResources,
				identityResources,
				users
			);
			return Ok();
		}


		/// <summary>Endpoint which retrieves all of the database registered data which is relevant to the integration tests.</summary>
		/// <returns>Returns a <see cref="TestDatabaseInitializerOutputModel"/> (wrapped within a <see cref="Task{TResult}"/>) containing all of the registered data.</returns>
		[HttpGet(GetAllRegisteredDataEndpoint)]
		public async Task<TestDatabaseInitializerOutputModel> GetAllRegisteredData()
		{
			// Fetch all data
			var clients = await _configurationDbContext.Clients
				.Include(client => client.IdentityProviderRestrictions)
				.Include(client => client.Claims)
				.Include(client => client.AllowedCorsOrigins)
				.Include(client => client.Properties)
				.Include(client => client.AllowedScopes)
				.Include(client => client.ClientSecrets)
				.Include(client => client.AllowedGrantTypes)
				.Include(client => client.RedirectUris)
				.Include(client => client.PostLogoutRedirectUris)
				.AsSplitQuery()
				.ToListAsync();
			var apiScopes = await _configurationDbContext.ApiScopes
				.Include(apiScope => apiScope.UserClaims)
				.Include(apiScope => apiScope.Properties)
				.AsSplitQuery()
				.ToListAsync();
			var apiResources = await _configurationDbContext.ApiResources
				.Include(apiResource => apiResource.Secrets)
				.Include(apiResource => apiResource.Scopes)
				.Include(apiResource => apiResource.UserClaims)
				.Include(apiResource => apiResource.Properties)
				.AsSplitQuery()
				.ToListAsync();
			var identityResources = await _configurationDbContext.IdentityResources
				.Include(identityResource => identityResource.UserClaims)
				.Include(identityResource => identityResource.Properties)
				.AsSplitQuery()
				.ToListAsync();
			var users = await _appDbContext.Users
				.ToListAsync();

			// For the users, also fetch and fill their claims
			var serializableUsers = users.Select(user => _mapper.Map<SerializableTestUser>(user.ConvertToTestUser()))
				.ToArray();
			foreach (var user in users)
			{
				var serializableUser = serializableUsers.Single(serUser => serUser.Username == user.UserName);
				var claims = await _userManager.GetClaimsAsync(user);
				serializableUser.Claims = claims.Select(claim => _mapper.Map<SerializableClaim>(claim));
			}

			// Return results
			var serializableClients = clients.Select(client => _mapper.Map<SerializableClient>(client.ToModel()));
			var serializableApiScopes = apiScopes.Select(apiScope => _mapper.Map<SerializableApiScope>(apiScope.ToModel()));
			var serializableApiResources = apiResources.Select(apiResource => _mapper.Map<SerializableApiResource>(apiResource.ToModel()));
			var serializableIdentityResources = identityResources.Select(identityResource => _mapper.Map<SerializableIdentityResource>(identityResource.ToModel()));
			var result = new TestDatabaseInitializerOutputModel()
			{
				Clients = serializableClients,
				ApiScopes = serializableApiScopes,
				ApiResources = serializableApiResources,
				IdentityResources = serializableIdentityResources,
				Users = serializableUsers,
			};
			return result;
		}


		/// <summary>Endpoint which clears all of the database registered data which is relevant to the integration tests.</summary>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>This endpoint should always return an HTTP 200 (Ok) Status Code.</para>
		/// </returns>
		[HttpGet(ClearDatabaseEndpoint)]
		public async Task<IActionResult> ClearDatabase()
		{
			await _databaseInitializerService.ClearDatabaseAsync();
			return Ok();
		}
	}
}