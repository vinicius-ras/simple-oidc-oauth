using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Mvc;
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





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="databaseInitializerService">Container-injected instance for the <see cref="IDatabaseInitializerService" /> service.</param>
		/// <param name="configurationDbContext">Container-injected instance for the <see cref="ConfigurationDbContext" /> service.</param>
		/// <param name="appDbContext">Container-injected instance for the <see cref="AppDbContext" /> service.</param>
		public TestDatabaseInitializerServiceController(
			IDatabaseInitializerService databaseInitializerService,
			ConfigurationDbContext configurationDbContext,
			AppDbContext appDbContext,
			IMapper mapper)
		{
			_databaseInitializerService = databaseInitializerService;
			_configurationDbContext = configurationDbContext;
			_appDbContext = appDbContext;
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
		public IActionResult InitializeDatabase([FromBody] TestDatabaseInitializerInputModel input)
		{
			_databaseInitializerService.InitializeDatabaseAsync(
				clients: input.Clients?.Select(serializableClient => _mapper.Map<Client>(serializableClient)),
				apiScopes: input.ApiScopes?.Select(serializableApiScope => _mapper.Map<ApiScope>(serializableApiScope)),
				apiResources: input.ApiResources?.Select(serializableApiResource => _mapper.Map<ApiResource>(serializableApiResource)),
				identityResources: input.IdentityResources?.Select(serializableIdentityResource => _mapper.Map<IdentityResource>(serializableIdentityResource)),
				users: input.Users?.Select(serializableUser => _mapper.Map<TestUser>(serializableUser))
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
				.Include(client => client.AllowedCorsOrigins)
				.Include(client => client.AllowedGrantTypes)
				.Include(client => client.AllowedScopes)
				.Include(client => client.PostLogoutRedirectUris)
				.Include(client => client.RedirectUris)
				.ToListAsync();
			var apiScopes = await _configurationDbContext.ApiScopes
				.ToListAsync();
			var apiResources = await _configurationDbContext.ApiResources
				.Include(apiResource => apiResource.UserClaims)
				.ToListAsync();
			var identityResources = await _configurationDbContext.IdentityResources
				.ToListAsync();
			var users = await _appDbContext.Users
				.ToListAsync();
			var result = new TestDatabaseInitializerOutputModel()
			{
				Clients = clients.Select(client => _mapper.Map<SerializableClient>(client.ToModel())),
				ApiScopes = apiScopes.Select(apiScope => _mapper.Map<SerializableApiScope>(apiScope.ToModel())),
				ApiResources = apiResources.Select(apiResource => _mapper.Map<SerializableApiResource>(apiResource.ToModel())),
				IdentityResources = identityResources.Select(identityResource => _mapper.Map<SerializableIdentityResource>(identityResource.ToModel())),
				Users = users.Select(user => _mapper.Map<SerializableTestUser>(user.ConvertToTestUser())),
			};

			// Return results
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