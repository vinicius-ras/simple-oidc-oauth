using IdentityServer4.Models;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Data.ValidationAttributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="Client"/> object.</summary>
	public class SerializableClient : IValidatableObject
	{
		// INSTANCE PROPERTIES
		/// <summary>
		///     Controls whether access tokens are transmitted via the browser for this client
        ///     (defaults to false). This can prevent accidental leakage of access tokens when
        ///     multiple response types are allowed.
		/// </summary>
		/// <value><c>true</c> if access tokens can be transmitted via the browser; otherwise, <c>false</c>.</value>
		public bool AllowAccessTokensViaBrowser { get; set; }
		/// <summary>Gets or sets the allowed CORS origins for JavaScript clients.</summary>
		/// <value>The allowed CORS origins.</value>
		[UrlOriginCollection]
		public IEnumerable<string> AllowedCorsOrigins { get; set; }
		/// <summary>
		///     Specifies the allowed grant types (legal combinations of AuthorizationCode, Implicit,
		///     Hybrid, ResourceOwner, ClientCredentials).
		/// </summary>
		[Required]
		[MinLength(1)]
		public IEnumerable<string> AllowedGrantTypes { get; set; }
		/// <summary>
		///     Specifies the api scopes that the client is allowed to request. If empty, the
		///     client can't access any scope.
		/// </summary>
		public IEnumerable<string> AllowedScopes { get; set; }
		/// <summary>Unique ID of the client.</summary>
		public string ClientId { get; set; }
		/// <summary>Client display name (used for logging and consent screen).</summary>
		[Required]
		public string ClientName { get; set; }
		/// <summary>Client secrets - only relevant for flows that require a secret.</summary>
		public IEnumerable<SerializableSecret> ClientSecrets { get; set; }
		/// <summary>Specifies allowed URIs to redirect to after logout.</summary>
		[Required]
		[MinLength(1)]
		[AbsoluteUrlCollection]
		public IEnumerable<string> PostLogoutRedirectUris { get; set; }
		/// <summary>Specifies allowed URIs to return tokens or authorization codes to.</summary>
		[Required]
		[MinLength(1)]
		[AbsoluteUrlCollection]
		public IEnumerable<string> RedirectUris { get; set; }
		/// <summary>
		///     If set to false, no client secret is needed to request tokens at the token endpoint
		///     (defaults to true)
		/// </summary>
		public bool RequireClientSecret { get; set; }
		/// <summary>Specifies whether a consent screen is required (defaults to <c>false</c>).</summary>
		public bool RequireConsent { get; set; }
		/// <summary>
		///     Specifies whether a proof key is required for authorization code based token
		///     requests (defaults to true).
		/// </summary>
		public bool RequirePkce { get; set; }





		// INTERFACE IMPLEMENTATION: IValidatableObject
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			// The client's allowed Grant Types must be in the list of allowed types, and must not be duplicated
			var allowedGrantTypes = this.AllowedGrantTypes ?? Enumerable.Empty<string>();
			var invalidGrantTypes = allowedGrantTypes
				.Where(grantType => AppConfigs.AllowedClientRegistrationGrantTypes.Contains(grantType) == false)
				.ToList();
			if (invalidGrantTypes.Count > 0)
			{
				var formattedInvalidGrantTypes = invalidGrantTypes.Select(grantType =>
					grantType == null
						? "null"
						: $@"""{grantType}"""
				);
				yield return new ValidationResult(
					invalidGrantTypes.Count == 1
						? $@"Invalid grant type: {formattedInvalidGrantTypes.First()}."
						: $@"Invalid grant types: {string.Join(", ", formattedInvalidGrantTypes)}.",
					new[] { nameof(this.AllowedGrantTypes) });
			}


			var duplicateGrantTypes = allowedGrantTypes
				.GroupBy(grantType => grantType)
				.Select(group => new {
					GrantType = group.Key,
					Count = group.Count(),
				})
				.Where(groupData => groupData.Count > 1)
				.Select(groupData => groupData.GrantType)
				.ToList();
			if (duplicateGrantTypes.Count > 0)
			{
				var formattedDuplicatedGrantTypes = duplicateGrantTypes.Select(grantType =>
					grantType == null
						? "null"
						: $@"""{grantType}"""
				);
				yield return new ValidationResult(
					duplicateGrantTypes.Count == 1
						? $@"Duplicated grant type: {formattedDuplicatedGrantTypes.First()}."
						: $@"Duplicated grant types: {string.Join(", ", formattedDuplicatedGrantTypes)}.",
					new[] { nameof(this.AllowedGrantTypes) }
				);
			}


			// If a Client Secret is required, then at least one client secret must be provided
			var clientSecrets = this.ClientSecrets ?? Enumerable.Empty<SerializableSecret>();
			if (this.RequireClientSecret && clientSecrets.Count() <= 0)
				yield return new ValidationResult(
					"Client requires at least one secret to be valid.",
					new[] { nameof(this.ClientSecrets) }
				);

			// Client secrets should not be null references, should not have null values, and must be of valid types
			if (clientSecrets.Any(secret => secret == null))
				yield return new ValidationResult(
					"Null Client Secret objects are not acceptable.",
					new[] { nameof(this.ClientSecrets) }
				);

			var nonNullClientSecrets = clientSecrets.Where(secret => secret != null);
			if (nonNullClientSecrets.Any(secret => secret.Value == null))
				yield return new ValidationResult(
					"Null Client Secret values are not acceptable.",
					new[] { nameof(this.ClientSecrets) }
				);

			var unsupportedSecretTypes = nonNullClientSecrets
				.Where(secret => AppConfigs.SupportedClientSecretTypes.Contains(secret.Type) == false)
				.Select(secret => secret.Type)
				.ToList();
			if (unsupportedSecretTypes.Count > 0)
			{
				var formattedSecretTypeNames = unsupportedSecretTypes.Select(secretType =>
					secretType == null
						? "null"
						: $@"""{secretType}"""
				);
				yield return new ValidationResult(
					unsupportedSecretTypes.Count == 1
						? $@"Unsupported Secret type: {formattedSecretTypeNames.First()}."
						: $@"Unsupported Secret types: {string.Join(", ", formattedSecretTypeNames)}.",
					new[] { nameof(this.ClientSecrets) }
				);
			}
		}
	}
}