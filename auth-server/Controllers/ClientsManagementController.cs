using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Data.Security;
using SimpleOidcOauth.Data.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Controllers
{
	/// <summary>
	///     Controller responsible for the the main endpoints used during Client Application registration
	///     and editing in the IdP Management Interface.
	/// </summary>
	[ApiController]
	[Authorize(AuthorizationPolicyNames.ClientsView)]
	[Route(AppEndpoints.ClientsManagementControllerUri)]
	public class ClientsManagementController : ControllerBase
	{
		// INSTANCE FIELDS
		/// <summary>Container-injected instance for the <see cref="ConfigurationDbContext"/> service.</summary>
		private readonly ConfigurationDbContext _configurationDbContext;
		/// <summary>Container-injected instance for the <see cref="IMapper"/> service.</summary>
		private readonly IMapper _mapper;
		/// <summary>Container-injected instance for the <see cref="ILogger{TCategoryName}"/> service.</summary>
		private readonly ILogger<ClientsManagementController> _logger;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="configurationDbContext">Container-injected instance for the <see cref="ConfigurationDbContext"/> service.</param>
		/// <param name="mapper">Container-injected instance for the <see cref="IMapper"/> service.</param>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}"/> service.</param>
		public ClientsManagementController(ConfigurationDbContext configurationDbContext, IMapper mapper, ILogger<ClientsManagementController> logger)
		{
			_configurationDbContext = configurationDbContext;
			_mapper = mapper;
			_logger = logger;
		}


		/// <summary>Retrieves a list of all of the currently registered Clients in the IdP Server.</summary>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>
		///         In case of success, this endpoint returns an HTTP OK (200) response with a collection of <see cref="SerializableClient"/> instances.
		///         Depending on the requesting user's claims, some of the returned information (such as client Secret values) might be redacted.
		///     </para>
		///     <para>
		///         In case of failure, this endpoint will return one of the following responses:
		///
		///         <list type="bullet">
		///             <item>
		///                 <term>HTTP Unauthorized (401)</term>
		///                 <description>Returned for unauthenticated requests.</description>
		///             </item>
		///             <item>
		///                 <term>HTTP Forbidden (403)</term>
		///                 <description>
		///                     Returned for authenticated requests coming from a user that does not have the claims
		///                     to view clients data (<see cref="AuthServerClaimTypes.CanViewClients"/>).
		///                 </description>
		///             </item>
		///         </list>
		///     </para>
		/// </returns>
		[HttpGet(AppEndpoints.GetAllRegisteredClients)]
		public async Task<IEnumerable<SerializableClient>> GetAllClients()
		{
			var clients = await _configurationDbContext.Clients
				.AsNoTracking()
				.AsSplitQuery()
				.Include(client => client.AllowedCorsOrigins)
				.Include(client => client.AllowedGrantTypes)
				.Include(client => client.AllowedScopes)
				.Include(client => client.ClientSecrets)
				.Include(client => client.PostLogoutRedirectUris)
				.Include(client => client.RedirectUris)
				.Select(client => _mapper.Map<SerializableClient>(client.ToModel()))
				.ToListAsync();
			return clients;
		}


		/// <summary>Retrieves information about a specific Client Application registered with the IdP server.</summary>
		/// <param name="clientId">
		///     The client ID (<see cref="IdentityServer4.EntityFramework.Entities.Client.ClientId"/>) for the client whose data needs to be requested.
		/// </param>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>
		///         In case of success, this endpoint returns an HTTP OK (200) response with a <see cref="SerializableClient"/> instance representing the target
		///         Client Application's data.
		///         Depending on the requesting user's claims, some of the returned information (such as client Secret values) might be redacted.
		///     </para>
		///     <para>
		///         In case of failure, this endpoint will return one of the following responses:
		///
		///         <list type="bullet">
		///             <item>
		///                 <term>HTTP Unauthorized (401)</term>
		///                 <description>Returned for unauthenticated requests.</description>
		///             </item>
		///             <item>
		///                 <term>HTTP Forbidden (403)</term>
		///                 <description>
		///                     Returned for authenticated requests coming from a user that does not have the claims
		///                     to view clients data (<see cref="AuthServerClaimTypes.CanViewClients"/>).
		///                 </description>
		///             </item>
		///             <item>
		///                 <term>HTTP Not Found (404)</term>
		///                 <description>
		///                     Returned when the specified client could not be found.
		///                 </description>
		///             </item>
		///         </list>
		///     </para>
		/// </returns>
		[HttpGet(AppEndpoints.GetRegisteredClient)]
		public async Task<ActionResult<SerializableClient>> GetClient([FromRoute(Name = AppEndpoints.ClientIdParameterName)] string clientId)
		{
			var clientEntity = await _configurationDbContext.Clients
				.AsNoTracking()
				.AsSplitQuery()
				.Include(client => client.AllowedCorsOrigins)
				.Include(client => client.AllowedGrantTypes)
				.Include(client => client.AllowedScopes)
				.Include(client => client.ClientSecrets)
				.Include(client => client.PostLogoutRedirectUris)
				.Include(client => client.RedirectUris)
				.SingleOrDefaultAsync(client => client.ClientId == clientId);
			if (clientEntity == null)
				return NotFound();

			var serializableClient = _mapper.Map<SerializableClient>(clientEntity.ToModel());
			return serializableClient;
		}


		/// <summary>Retrieves a list of all of the currently allowed Grant Types for client registration.</summary>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>In case of success, this endpoint returns an HTTP OK (200) response with a collection of strings representing the allowed grant types.</para>
		///     <para>
		///         In case of failure, this endpoint will return one of the following responses:
		///
		///         <list type="bullet">
		///             <item>
		///                 <term>HTTP Unauthorized (401)</term>
		///                 <description>Returned for unauthenticated requests.</description>
		///             </item>
		///             <item>
		///                 <term>HTTP Forbidden (403)</term>
		///                 <description>
		///                     Returned for authenticated requests coming from a user that does not have the claims
		///                     to view clients data (<see cref="AuthServerClaimTypes.CanViewClients"/>).
		///                 </description>
		///             </item>
		///         </list>
		///     </para>
		/// </returns>
		[HttpGet(AppEndpoints.GetAllowedClientRegistrationGrantTypes)]
		public IEnumerable<string> GetAllowedClientRegistrationGrantTypes() => AppConfigs.AllowedClientRegistrationGrantTypes;


		/// <summary>Retrieves a list of all of the currently registered resources (API Scopes, API Resources and Identity Resources).</summary>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>
		///         In case of success, this endpoint returns an HTTP OK (200) response with a collection of <see cref="SerializableResource"/> instances,
		///         representing the list of available/registered resources and their data.
		///         Notice that this data includes API Scopes, API Resources and Identity Resources.
		///     </para>
		///     <para>
		///         In case of failure, this endpoint will return one of the following responses:
		///
		///         <list type="bullet">
		///             <item>
		///                 <term>HTTP Unauthorized (401)</term>
		///                 <description>Returned for unauthenticated requests.</description>
		///             </item>
		///             <item>
		///                 <term>HTTP Forbidden (403)</term>
		///                 <description>
		///                     Returned for authenticated requests coming from a user that does not have the claims
		///                     to view clients data (<see cref="AuthServerClaimTypes.CanViewClients"/>).
		///                 </description>
		///             </item>
		///         </list>
		///     </para>
		/// </returns>
		[HttpGet(AppEndpoints.GetAvailableClientRegistrationResources)]
		public async Task<IEnumerable<SerializableResource>> GetAvailableResources()
		{
			// Retrieve all resources
			var apiScopes = await _configurationDbContext.ApiScopes
				.Select(apiScope => _mapper.Map<SerializableResource>(apiScope.ToModel()))
				.ToListAsync();
			var apiResources = await _configurationDbContext.ApiResources
				.Select(apiResource => _mapper.Map<SerializableResource>(apiResource.ToModel()))
				.ToListAsync();
			var identityResources = await _configurationDbContext.IdentityResources
				.Select(identityResource => _mapper.Map<SerializableResource>(identityResource.ToModel()))
				.ToListAsync();

			// Return all results
			var allResources = apiScopes.Concat(apiResources)
				.Concat(identityResources);
			return allResources;
		}


		/// <summary>Endpoint used to register a new Client Application in the IdP Management Interface.</summary>
		/// <param name="client">The data for the client that needs to be created.</param>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>
		///         In case of success, this endpoint will return an HTTP Created (201) response with a body containing the data registered for the new
		///         client (in <see cref="SerializableClient"/> format). Also, a "Location" HTTP Header with the URI to retrieve the created Client's data will be present in
		///         the response.
		///     </para>
		///     <para>
		///         In case of failure, this endpoint will return one of the following responses:
		///
		///         <list type="bullet">
		///             <item>
		///                 <term>HTTP Bad Request (400)</term>
		///                 <description>
		///                     Returned in case of any validation errors, such as invalid/missing data.
		///                     The response body will contain a <see cref="ValidationProblemDetails"/> describing the error.
		///                 </description>
		///             </item>
		///             <item>
		///                 <term>HTTP Unauthorized (401)</term>
		///                 <description>Returned for unauthenticated requests.</description>
		///             </item>
		///             <item>
		///                 <term>HTTP Forbidden (403)</term>
		///                 <description>
		///                     Returned for authenticated requests coming from a user that does not have the claims
		///                     to view clients data (<see cref="AuthServerClaimTypes.CanViewAndEditClients"/>).
		///                 </description>
		///             </item>
		///         </list>
		///     </para>
		/// </returns>
		[HttpPost(AppEndpoints.CreateNewClientApplication)]
		[Authorize(AuthorizationPolicyNames.ClientsViewAndEdit)]
		public async Task<ActionResult<SerializableClient>> CreateNewClientApplication([FromBody] SerializableClient client)
		{
			// Validate incoming data
			if (string.IsNullOrEmpty(client.ClientId) == false)
				ModelState.AddModelError(nameof(client.ClientId), $@"New Client Applications must have an empty Client ID.");

			if (ModelState.ErrorCount > 0)
				return BadRequest(new ValidationProblemDetails(ModelState));

			// Generate a random client ID and register a new Client Application
			var clientEntity = _mapper.Map<Client>(client)
				.ToEntity();
			clientEntity.ClientId = Guid.NewGuid().ToString();

			await _configurationDbContext.Clients.AddAsync(clientEntity);
			await _configurationDbContext.SaveChangesAsync();

			// Return the created client's data
			var updatedSerializableClient = _mapper.Map<SerializableClient>(clientEntity.ToModel());
			return CreatedAtAction(
				nameof(GetClient),
				new RouteValueDictionary {
					[AppEndpoints.ClientIdParameterName] = clientEntity.ClientId,
				},
				updatedSerializableClient);
		}


		/// <summary>Endpoint used to update an existing Client Application's data in the IdP Management Interface.</summary>
		/// <param name="clientId">The Client ID for the client that must be updated.</param>
		/// <param name="clientData">
		///     <para>The data for the client that needs to be updated.</para>
		///     <para>
		///         The <see cref="SerializableClient.ClientId"/> property must match the ID provided in the <paramref name="clientId"/> URL parameter.
		///         Otherwise, this endpoint will return an HTTP 400 (Bad Request) response.
		///     </para>
		/// </param>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>
		///         In case of success, this endpoint will return an HTTP Ok (200) response with a body containing the data registered for the new
		///         client (in <see cref="SerializableClient"/> format).
		///     </para>
		///     <para>
		///         In case of failure, this endpoint will return one of the following responses:
		///
		///         <list type="bullet">
		///             <item>
		///                 <term>HTTP Bad Request (400)</term>
		///                 <description>
		///                     Returned in case of any validation errors, such as invalid/missing data.
		///                     The response body will contain a <see cref="ValidationProblemDetails"/> describing the error.
		///                 </description>
		///             </item>
		///             <item>
		///                 <term>HTTP Unauthorized (401)</term>
		///                 <description>Returned for unauthenticated requests.</description>
		///             </item>
		///             <item>
		///                 <term>HTTP Forbidden (403)</term>
		///                 <description>
		///                     Returned for authenticated requests coming from a user that does not have the claims
		///                     to view clients data (<see cref="AuthServerClaimTypes.CanViewAndEditClients"/>).
		///                 </description>
		///             </item>
		///             <item>
		///                 <term>HTTP Not Found (404)</term>
		///                 <description>Returned if the specified Client Application is not registered in the database.</description>
		///             </item>
		///         </list>
		///     </para>
		/// </returns>
		[HttpPut(AppEndpoints.UpdateClientApplication)]
		[Authorize(AuthorizationPolicyNames.ClientsViewAndEdit)]
		public async Task<ActionResult<SerializableClient>> UpdateClientApplication(
			[FromRoute(Name = AppEndpoints.ClientIdParameterName)] string clientId,
			[FromBody] SerializableClient clientData)
		{
			// Extra validation: client ID cannot be null, cannot be empty, and must match both in the requested URL and the data sent to this endpoint
			if (string.IsNullOrEmpty(clientData.ClientId))
				ModelState.AddModelError(nameof(clientData.ClientId), $@"An existing Client ID must be provided.");

			if (clientId != clientData.ClientId)
				ModelState.AddModelError(nameof(clientData.ClientId), $@"URI's Client ID and HTTP payload ""{nameof(clientData.ClientId)}"" do not match.");


			// In case of any validation errors, return an HTTP Bad Request (400) response straight away (containing all errors)
			if (ModelState.ErrorCount > 0)
				return BadRequest(new ValidationProblemDetails(ModelState));


			// Try to find the client's data
			var clientInDatabase = await _configurationDbContext.Clients
				.Include(client => client.AllowedCorsOrigins)
				.Include(client => client.AllowedGrantTypes)
				.Include(client => client.AllowedScopes)
				.Include(client => client.ClientSecrets)
				.Include(client => client.PostLogoutRedirectUris)
				.Include(client => client.RedirectUris)
				.AsSplitQuery()
				.FirstOrDefaultAsync(c => c.ClientId == clientId);
			if (clientInDatabase == null)
				return NotFound();

			// Hash any unhashed client secret
			foreach (var secret in clientData.ClientSecrets)
			{
				if (secret.IsValueHashed == false)
					secret.Value = secret.Value.Sha256();
			}

			// Update values and navigation properties on the found client
			var updatedEntityData = _mapper.Map<Client>(clientData)
				.ToEntity();
			updatedEntityData.Id = clientInDatabase.Id;

			var entry = _configurationDbContext.Entry(clientInDatabase);
			entry.CurrentValues.SetValues(updatedEntityData);

			clientInDatabase.AllowedCorsOrigins = updatedEntityData.AllowedCorsOrigins;
			clientInDatabase.AllowedGrantTypes = updatedEntityData.AllowedGrantTypes;
			clientInDatabase.AllowedScopes = updatedEntityData.AllowedScopes;
			clientInDatabase.ClientSecrets = updatedEntityData.ClientSecrets;
			clientInDatabase.PostLogoutRedirectUris = updatedEntityData.PostLogoutRedirectUris;
			clientInDatabase.RedirectUris = updatedEntityData.RedirectUris;

			// Delete old versions of remaining related entities
			var oldCorsOrigins = _configurationDbContext.ClientCorsOrigins
				.Where(origin => origin.ClientId == clientInDatabase.Id);
			_configurationDbContext.ClientCorsOrigins.RemoveRange(oldCorsOrigins);

			// Save changes to the database
			await _configurationDbContext.SaveChangesAsync();

			// Return the created client's data
			var updatedSerializableClient = _mapper.Map<SerializableClient>(clientInDatabase.ToModel());
			return Ok(updatedSerializableClient);
		}
	}
}