using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleOidcOauth.Data.Configuration;
using System;
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
			IServiceProvider serviceProvider)
		{
			_databaseInitializationConfigs = appConfigs.Value?.DatabaseInitialization;
			_logger = logger;
			_serviceProvider = serviceProvider;
		}





		// INTERFACE IMPLEMENTATION: IHostedService
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			// This service must be enabled in the app's configurations in order to work
			if (_databaseInitializationConfigs?.Enabled != true)
			{
				_logger.LogDebug("Skipping database initialization: the initialization is disabled in the app's configurations.");
				return;
			}

			// Initialize the database
			_logger.LogDebug("Database initialization running...");
			using (var serviceScope = _serviceProvider.CreateScope())
			{
				var databaseInitializerService = serviceScope.ServiceProvider.GetRequiredService<IDatabaseInitializerService>();
				await databaseInitializerService.InitializeDatabaseAsync(
					clients: _databaseInitializationConfigs?.Clients,
					apiScopes: _databaseInitializationConfigs?.ApiScopes,
					apiResources: _databaseInitializationConfigs?.ApiResources,
					identityResources: _databaseInitializationConfigs?.IdentityResources,
					users: _databaseInitializationConfigs?.Users);
			}
			_logger.LogDebug("Finished database initialization successfully.");
		}


		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}