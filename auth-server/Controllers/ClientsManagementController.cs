using AutoMapper;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Data.Security;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.OpenApi.Swagger.Attributes;
using SimpleOidcOauth.OpenApi.Swagger.Filters;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
	[SwaggerTag("Endpoints for managing the Client Applications which can communicate with the IdP.")]
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
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">
		///     Indicates success on the operation.
		///     A collection of <see cref="SerializableClient"/> objects will be returned in the response body.
		///     Depending on the requesting user's claims, some of the returned information (such as client Secret values) might be redacted.
		/// </response>
		/// <response code="401">Status code returned for unauthenticated requests.</response>
		/// <response code="403">
		///     Returned for authenticated requests coming from a user that does not have the claims
		///     to view clients data (<see cref="AuthServerClaimTypes.CanViewClients"/>).
		/// </response>
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
				.Select(client => _mapper.Map<SerializableClient>(client))
				.ToListAsync();
			return clients;
		}


		/// <summary>Retrieves information about a specific Client Application registered with the IdP server.</summary>
		/// <param name="clientId">
		///     The client ID (<see cref="IdentityServer4.EntityFramework.Entities.Client.ClientId"/>) for the client whose data needs to be requested.
		/// </param>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">
		///     Indicates the operation was successful. A <see cref="SerializableClient"/> object will be returned in the response body, describing the
		///     requested Client Application.
		/// </response>
		/// <response code="401">Returned for unauthenticated requests.</response>
		/// <response code="403">
		///     Returned for authenticated requests coming from a user that does not have the claims
		///     to view clients data (<see cref="AuthServerClaimTypes.CanViewClients"/>).
		/// </response>
		/// <response code="404">Informs that the specified client could not be found in the database.</response>
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

			var serializableClient = _mapper.Map<SerializableClient>(clientEntity);
			return serializableClient;
		}


		/// <summary>Retrieves a list of all of the currently allowed Grant Types for client registration.</summary>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">
		///     Indicates the operation was successful. A collection of strings representing the allowed grant types will be returned in
		///     the response body.
		/// </response>
		/// <response code="401">Returned for unauthenticated requests.</response>
		/// <response code="403">
		///     Returned for authenticated requests coming from a user that does not have the claims
		///     to view clients data (<see cref="AuthServerClaimTypes.CanViewClients"/>).
		/// </response>
		[HttpGet(AppEndpoints.GetAllowedClientRegistrationGrantTypes)]
		[SwaggerOperationFilter(typeof(ClientsManagementControllerGrantTypesOperationFilter))]
		public IEnumerable<string> GetAllowedClientRegistrationGrantTypes() => AppConfigs.AllowedClientRegistrationGrantTypes;


		/// <summary>Retrieves a list of all of the currently registered resources (API Scopes, API Resources and Identity Resources).</summary>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">
		///     <para>
		///         Indicates the operation was successful. A collection of <see cref="SerializableResource"/> objects will be returned, each one representing
		///         one of the available resources and their respective data.
		///     </para>
		///     <para>Notice that this data might include a mix of API Scopes, API Resources, and Identity Resources.</para>
		/// </response>
		/// <response code="401">Returned for unauthenticated requests.</response>
		/// <response code="403">
		///     Returned for authenticated requests coming from a user that does not have the claims
		///     to view clients data (<see cref="AuthServerClaimTypes.CanViewClients"/>).
		/// </response>
		[HttpGet(AppEndpoints.GetAvailableClientRegistrationResources)]
		public async Task<IEnumerable<SerializableResource>> GetAvailableResources()
		{
			// Retrieve all resources
			var apiScopes = await _configurationDbContext.ApiScopes
				.Select(apiScope => _mapper.Map<SerializableApiScope>(apiScope))
				.ToListAsync();
			var apiResources = await _configurationDbContext.ApiResources
				.Select(apiResource => _mapper.Map<SerializableApiResource>(apiResource))
				.ToListAsync();
			var identityResources = await _configurationDbContext.IdentityResources
				.Select(identityResource => _mapper.Map<SerializableIdentityResource>(identityResource))
				.ToListAsync();

			// Return all results
			var allResources = Enumerable.Empty<SerializableResource>()
				.Concat(apiScopes)
				.Concat(apiResources)
				.Concat(identityResources);
			return allResources;
		}


		/// <summary>Registers a new Client Application in the IdP Management Interface.</summary>
		/// <remarks>
		///     Notice that all client secrets sent to this endpoint must be new. Thus, they must have their <see cref="SerializableSecret.IsValueHashed"/> field set to false, and
		///     their <see cref="SerializableSecret.Value"/> field set to a non-empty plaintext representation of the password. Also notice that after this endpoint returns a
		///     successful result, all of the Secrets' values will be hashed to the application's database and their plaintext representations will be unrecoverable.
		///
		///     Any endpoints called after a Client Application registration which return the client's data will always return the secrets'
		///     <see cref="SerializableSecret.IsValueHashed"/> field set to true, and their <see cref="SerializableSecret.Value"/> fields always set to null.
		/// </remarks>
		/// <param name="client">The data for the client that needs to be created.</param>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="201">
		///     Indicates the operation was successful. A <see cref="SerializableClient"/> containing the newly created client's data
		///     will be returned in the response's body. The response will also contain a "Location" HTTP Header indicating the URI where
		///     API clients can retrieve the created client's data.
		/// </response>
		/// <response code="400">
		///     Indicates validation errors in the request body's payload data.
		///     The response body will contain a <see cref="ValidationProblemDetails"/> instance describing the errors.
		/// </response>
		/// <response code="401">Status code returned for unauthenticated requests.</response>
		/// <response code="403">
		///     Status code returned for authenticated requests coming from a user that does not have the claims to view
		///     clients' data (<see cref="AuthServerClaimTypes.CanViewAndEditClients"/>).
		/// </response>
		[HttpPost(AppEndpoints.CreateNewClientApplication)]
		[Authorize(AuthorizationPolicyNames.ClientsViewAndEdit)]
		[ProducesResponseType(typeof(SerializableClient), 201)]
		[ProducesResponseType(typeof(ValidationProblemDetails), 400)]
		[ProducesResponseType(401)]
		[ProducesResponseType(403)]
		[ProducesErrorResponseType(typeof(void))]
		[CustomResponseHeader(StatusCode = HttpStatusCode.Created, HeaderName = nameof(HeaderNames.Location), Description = "URI to the newly registered Client Application's data.")]
		public async Task<ActionResult<SerializableClient>> CreateNewClientApplication([FromBody] SerializableClient client)
		{
			// Validate incoming data
			if (string.IsNullOrEmpty(client.ClientId) == false)
				ModelState.AddModelError(nameof(client.ClientId), $@"New Client Applications must have an empty Client ID.");

			client.ClientSecrets ??= Enumerable.Empty<SerializableSecret>();
			int totalSecrets = client.ClientSecrets.Count();
			for (int s = 0; s < totalSecrets; s++)
			{
				var secret = client.ClientSecrets.ElementAt(s);
				if (secret.IsValueHashed == true)
					ModelState.AddModelError($"{nameof(client.ClientSecrets)}[{s}]", "Secrets must not be hashed while registering new Client Applications.");
			}

			if (ModelState.ErrorCount > 0)
				return BadRequest(new ValidationProblemDetails(ModelState));

			// Generate a random client ID and register a new Client Application
			var clientEntity = _mapper.Map<Client>(client);
			clientEntity.ClientId = Guid.NewGuid().ToString();

			await _configurationDbContext.Clients.AddAsync(clientEntity);
			await _configurationDbContext.SaveChangesAsync();

			// Return the created client's data
			var updatedSerializableClient = _mapper.Map<SerializableClient>(clientEntity);
			return CreatedAtAction(
				nameof(GetClient),
				new RouteValueDictionary {
					[AppEndpoints.ClientIdParameterName] = clientEntity.ClientId,
				},
				updatedSerializableClient);
		}


		/// <summary>Updates the data of a Client Application that has been previously registered with the IdP.</summary>
		/// <remarks>
		///     Notice that all new client secrets sent to this endpoint must have their <see cref="SerializableSecret.IsValueHashed"/> field set to false, and
		///     their <see cref="SerializableSecret.Value"/> field set to a non-empty plaintext representation of the password.
		///
		///     As opposed to that, any already registered client secrets that should be kept in the database must be sent with their <see cref="SerializableSecret.IsValueHashed"/>
		///     fields set to true, and their <see cref="SerializableSecret.Value"/> fields set to null. This will inform the Auth Server to keep the secrets' values unchanged,
		///     while possibly updating other fields of the secret (e.g., you can send a new description for the secret).
		///
		///     It is not possible to modify the value of any previously registered secret - if you lost the plaintext representation of a secret, it will need to be excluded
		///     and recreated.
		/// </remarks>
		/// <param name="clientId">The Client ID for the client that must be updated.</param>
		/// <param name="clientData">
		///     <para>The new data for the client that needs to be updated.</para>
		///     <para>
		///         The <see cref="SerializableClient.ClientId"/> property must match the ID provided in the <paramref name="clientId"/> URL parameter.
		///         Otherwise, this endpoint will return an HTTP 400 (Bad Request) response.
		///     </para>
		/// </param>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">
		///     Indicates the operation was successful. A <see cref="SerializableClient"/> instance representing the updated client data will
		///     be returned in the response body.
		/// </response>
		/// <response code="400">
		///     Indicates that invalid/incorrect Client Application data has been sent to the endpoint.
		///     The response body will contain a <see cref="ValidationProblemDetails"/> describing the error.
		/// </response>
		/// <response code="401">Status code returned for unauthenticated requests.</response>
		/// <response code="403">
		///     Status code returned for authenticated requests coming from a user that does not have the claims to view
		///     clients' data (<see cref="AuthServerClaimTypes.CanViewAndEditClients"/>).
		/// </response>
		/// <response code="404">Informs that the specified client could not be found in the database.</response>
		[HttpPut(AppEndpoints.UpdateClientApplication)]
		[Authorize(AuthorizationPolicyNames.ClientsViewAndEdit)]
		[ProducesResponseType(typeof(SerializableClient), 200)]
		[ProducesResponseType(typeof(ValidationProblemDetails), 400)]
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


			// For client secrets which are being updated, recover their values from the database
			var updatedEntityData = _mapper.Map<Client>(clientData);
			updatedEntityData.Id = clientInDatabase.Id;

			var secretIdsToUpdateInDatabase = clientData.ClientSecrets
				.Where(secret => secret.DatabaseId.HasValue)
				.Select(secret => secret.DatabaseId.Value)
				.ToHashSet();
			var clientSecretsInDatabase = clientInDatabase.ClientSecrets
				.Select(secret => new {
					secretId = secret.Id,
					secretValue = secret.Value,
				})
				.ToHashSet();
			for (int s = 0; s < updatedEntityData.ClientSecrets.Count; s++)
			{
				var updatedSecret = updatedEntityData.ClientSecrets[s];
				if (updatedSecret.Id <= 0)
					continue;


				var correspondingDatabaseSecret = clientSecretsInDatabase.FirstOrDefault(databaseSecret => databaseSecret.secretId == updatedSecret.Id);
				if (correspondingDatabaseSecret == null)
					ModelState.AddModelError($"{nameof(clientData.ClientSecrets)}[{s}].{nameof(SerializableSecret.DatabaseId)}", $"Could not find Secret entry in the database.");
				else
					updatedSecret.Value = correspondingDatabaseSecret.secretValue;
			}

			// In case of any new validation errors, return an HTTP Bad Request (400) response
			if (ModelState.ErrorCount > 0)
				return BadRequest(new ValidationProblemDetails(ModelState));


			// Update values and navigation properties on the found client
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
			var updatedSerializableClient = _mapper.Map<SerializableClient>(clientInDatabase);
			return Ok(updatedSerializableClient);
		}
	}
}