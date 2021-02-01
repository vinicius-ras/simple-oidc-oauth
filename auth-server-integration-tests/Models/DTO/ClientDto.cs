using System.Collections.Generic;
using IdentityServer4.Models;

namespace SimpleOidcOauth.Tests.Integration.Models.DTO
{
	/// <summary>
	///     <para>
	///         A simplified <see cref="Client"/> version used to generate output for
	///         the <see cref="Controllers.TestDatabaseInitializerServiceController.GetAllRegisteredData"/> endpoint.
	///     </para>
	///     <para>
	///         This class is required to avoid serialization errors when returning JSON responses.
	///         Serialization errors include object cycles, trying to serialize properties whose types do not contain default
	///         constructors (e.g., <see cref="System.Security.Claims.Claim"/>), and others.
	///     </para>
	/// </summary>
	public class ClientDto
	{
		// INSTANCE PROPERTIES
		/// <summary>Maps the <see cref="Client.AllowAccessTokensViaBrowser"/> property.</summary>
		public bool AllowAccessTokensViaBrowser { get; set; }
		/// <summary>Maps the <see cref="Client.ClientId"/> property.</summary>
		public string ClientId { get; set; }
		/// <summary>Maps the <see cref="Client.ClientName"/> property.</summary>
		public string ClientName { get; set; }
		/// <summary>Maps the <see cref="Client.RequireConsent"/> property.</summary>
		public bool RequireConsent { get; set; }
		/// <summary>Maps the <see cref="Client.RequirePkce"/> property.</summary>
		public bool RequirePkce { get; set; }
		/// <summary>Maps the <see cref="Client.RequireClientSecret"/> property.</summary>
		public bool RequireClientSecret { get; set; }
		/// <summary>Maps the <see cref="Client.ClientSecrets"/> property.</summary>
		public ICollection<Secret> ClientSecrets { get; set; } = new List<Secret>();
		/// <summary>Maps the <see cref="Client.PostLogoutRedirectUris"/> property.</summary>
		public ICollection<string> PostLogoutRedirectUris { get; set; } = new List<string>();
		/// <summary>Maps the <see cref="Client.RedirectUris"/> property.</summary>
		public ICollection<string> RedirectUris { get; set; } = new List<string>();
		/// <summary>Maps the <see cref="Client.AllowedCorsOrigins"/> property.</summary>
		public ICollection<string> AllowedCorsOrigins { get; set; } = new List<string>();
		/// <summary>Maps the <see cref="Client.AllowedGrantTypes"/> property.</summary>
		public ICollection<string> AllowedGrantTypes { get; set; } = new List<string>();
		/// <summary>Maps the <see cref="Client.AllowedScopes"/> property.</summary>
		public ICollection<string> AllowedScopes { get; set; } = new List<string>();





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		public ClientDto()
		{
		}


		/// <summary>Constructor.</summary>
		/// <param name="sourceClient">The source <see cref="Client"/> object whose data will be used to generate the corresponding <see cref="ClientDto"/>.</param>
		public ClientDto(Client sourceClient)
		{
			AllowAccessTokensViaBrowser = sourceClient.AllowAccessTokensViaBrowser;
			ClientId = sourceClient.ClientId;
			ClientName = sourceClient.ClientName;
			RequireConsent = sourceClient.RequireConsent;
			RequirePkce = sourceClient.RequirePkce;
			RequireClientSecret = sourceClient.RequireClientSecret;

			foreach (var origin in sourceClient.AllowedCorsOrigins)
				AllowedCorsOrigins.Add(origin);

			foreach (var grantType in sourceClient.AllowedGrantTypes)
				AllowedGrantTypes.Add(grantType);

			foreach (var scope in sourceClient.AllowedScopes)
				AllowedScopes.Add(scope);

			foreach (var clientSecret in sourceClient.ClientSecrets)
				ClientSecrets.Add(clientSecret);

			foreach (var postLogoutRedirectUri in sourceClient.PostLogoutRedirectUris)
				PostLogoutRedirectUris.Add(postLogoutRedirectUri);

			foreach (var redirectUri in sourceClient.RedirectUris)
				RedirectUris.Add(redirectUri);
		}


		/// <summary>Converts the <see cref="ClientDto"/> into a <see cref="Client"/> instance.</summary>
		/// <returns>Returns the corresponding <see cref="Client"/> instance.</returns>
		public Client MakeClient()
		{
			var result = new Client
			{
				AllowAccessTokensViaBrowser = this.AllowAccessTokensViaBrowser,
				ClientId = this.ClientId,
				ClientName = this.ClientName,
				RequireConsent = this.RequireConsent,
				RequirePkce = this.RequirePkce,
				RequireClientSecret = this.RequireClientSecret,
				AllowedCorsOrigins = new List<string>(),
				AllowedGrantTypes = new List<string>(),
				AllowedScopes = new List<string>(),
				ClientSecrets = new List<Secret>(),
				PostLogoutRedirectUris = new List<string>(),
				RedirectUris = new List<string>(),
			};

			foreach (var origin in AllowedCorsOrigins)
				result.AllowedCorsOrigins.Add(origin);

			foreach (var grantType in AllowedGrantTypes)
				result.AllowedGrantTypes.Add(grantType);

			foreach (var scope in AllowedScopes)
				result.AllowedScopes.Add(scope);

			foreach (var clientSecret in ClientSecrets)
				result.ClientSecrets.Add(clientSecret);

			foreach (var postLogoutRedirectUri in PostLogoutRedirectUris)
				result.PostLogoutRedirectUris.Add(postLogoutRedirectUri);

			foreach (var redirectUri in RedirectUris)
				result.RedirectUris.Add(redirectUri);

			return result;
		}
	}
}