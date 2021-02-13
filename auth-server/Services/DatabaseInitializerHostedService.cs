using AutoMapper;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Data.Serialization;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Services
{
	/// <summary>
	///     An <see cref="IHostedService"/> which initializes the database according to the application's configurations (<see cref="DatabaseInitializationConfigs"/>).
	///     This service runs to completion before the server starts servicing HTTP clients.
	/// </summary>
	public class DatabaseInitializerHostedService : IHostedService
	{
		// INSTANCE FIELDS
		/// <summary>Container-injected database initialization configurations.</summary>
		private readonly DatabaseInitializationConfigs _databaseInitializationConfigs;
		/// <summary>Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</summary>
		private readonly ILogger<DatabaseInitializerHostedService> _logger;
		/// <summary>Container-injected instance for the <see cref="IMapper" /> service (from AutoMapper).</summary>
		private readonly IMapper _mapper;
		/// <summary>
		///     <para>Container-injected instance for the <see cref="IServiceProvider" /> service.</para>
		///     <para>This instance is used for creating scopes in order to access scoped services.</para>
		/// </summary>
		private readonly IServiceProvider _serviceProvider;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="appConfigs">Container-injected application configurations.</param>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		/// <param name="serviceProvider">
		///     <para>Container-injected instance for the <see cref="IServiceProvider" /> service.</para>
		///     <para>This instance is used for creating scopes in order to access scoped services.</para>
		/// </param>
		public DatabaseInitializerHostedService(
			IOptions<AppConfigs> appConfigs,
			ILogger<DatabaseInitializerHostedService> logger,
			IServiceProvider serviceProvider,
			IMapper mapper)
		{
			_databaseInitializationConfigs = appConfigs.Value?.DatabaseInitialization;
			_logger = logger;
			_serviceProvider = serviceProvider;
			_mapper = mapper;
		}





		// INTERFACE IMPLEMENTATION: IHostedService
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			// Verify if any initialization is enabled
			if (_databaseInitializationConfigs?.InitializeStructure != true && _databaseInitializationConfigs?.InitializeData != true)
			{
				_logger.LogDebug("Skipping database initialization procedures: application not configured to initialize the structure or the data for the database.");
				return;
			}

			_logger.LogDebug("Starting database initialization procedures...");
			using (var serviceScope = _serviceProvider.CreateScope())
			{
				var databaseInitializerService = serviceScope.ServiceProvider.GetRequiredService<IDatabaseInitializerService>();

				// Clean the database before initializing (if necessary)
				if (_databaseInitializationConfigs?.CleanBeforeInitialize == true)
				{
					_logger.LogDebug("Cleaning all database data...");
					await databaseInitializerService.ClearDatabaseAsync();
					_logger.LogDebug("Finished cleaning all database data.");
				}


				// Initialize the database's structure (if necessary)
				if (_databaseInitializationConfigs?.InitializeStructure == true)
				{
					_logger.LogDebug("Initializing database's structure...");
					await databaseInitializerService.InitializeDatabaseAsync();
					_logger.LogDebug("Finished database's structure initialization.");
				}
				else
					_logger.LogDebug("Skipping database's structural initialization: that initialization is disabled in the app's configurations.");


				// Initialize the database's data (if necessary)
				if (_databaseInitializationConfigs?.InitializeData == true)
				{
					_logger.LogDebug("Initializing database's data...");
					try
					{
						var clients = _databaseInitializationConfigs
							?.Clients
							?.Select(serializableObject => _mapper.Map<SerializableClient, Client>(serializableObject));
						var apiScopes = _databaseInitializationConfigs
							?.ApiScopes
							?.Select(serializableObject => _mapper.Map<SerializableApiScope, ApiScope>(serializableObject));
						var apiResources = _databaseInitializationConfigs
							?.ApiResources
							?.Select(serializableObject => _mapper.Map<SerializableApiResource, ApiResource>(serializableObject));
						var identityResources = _databaseInitializationConfigs
							?.IdentityResources
							?.Select(serializableObject => _mapper.Map<SerializableIdentityResource, IdentityResource>(serializableObject));
						var users = _databaseInitializationConfigs
							?.Users
							?.Select(serializableObject => _mapper.Map<SerializableTestUser, TestUser>(serializableObject));
						await databaseInitializerService.InitializeDatabaseAsync(
							clients,
							apiScopes,
							apiResources,
							identityResources,
							users);
					}
					catch (Exception ex)
					{
						// Log and rethrow
						_logger.LogError(ex, "Failed to initialize database with sample data!");
						throw;
					}
					_logger.LogDebug("Finished initialization of database's data.");
				}
				else
					_logger.LogDebug("Skipping database's data initialization: that initialization is disabled in the app's configurations.");
			}

			// Log ending of procedures
			_logger.LogDebug("Finished database initialization procedures.");
		}


		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}