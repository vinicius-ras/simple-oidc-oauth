using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.Tests.Integration.Utilities;

namespace SimpleOidcOauth.Tests.Integration.Data.Comparers
{
	/// <summary>An equality comparer for <see cref="SerializableClient"/> objects.</summary>
	class SerializableClientEqualityComparer : IEqualityComparer<SerializableClient>
	{
		// INSTANCE PROPERTIES
		/// <summary>A flag indicating if the <see cref="SerializableClient.AllowedCorsOrigins"/> collections should be compared.</summary>
		public bool CompareAllowedCorsOrigins { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableClient.AllowedGrantTypes"/> collections should be compared.</summary>
		public bool CompareAllowedGrantTypes { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableClient.AllowedScopes"/> collections should be compared.</summary>
		public bool CompareAllowedScopes { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableClient.PostLogoutRedirectUris"/> collections should be compared.</summary>
		public bool ComparePostLogoutRedirectUris { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableClient.RedirectUris"/> collections should be compared.</summary>
		public bool CompareRedirectUris { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableClient.ClientSecrets"/> collections should be compared.</summary>
		public bool CompareClientSecrets { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableClient.ClientId"/> properties should be compared.</summary>
		public bool CompareClientId { init; get; } = true;





		// INTERFACE IMPLEMENTATION: IEqualityComparer<SerializableClient>
		public bool Equals(SerializableClient x, SerializableClient y)
		{
			// Basic null comparisons
			if (x == null && y == null)
				return true;
			else if (x == null || y == null)
				return false;

			// Compare simple properties
			if (x.AllowAccessTokensViaBrowser != y.AllowAccessTokensViaBrowser)
				return false;
			if (CompareClientId && x.ClientId != y.ClientId)
				return false;
			if (x.ClientName != y.ClientName)
				return false;
			if (x.RequireClientSecret != y.RequireClientSecret)
				return false;
			if (x.RequireConsent != y.RequireConsent)
				return false;
			if (x.RequirePkce != y.RequirePkce)
				return false;

			// Compare collection-like properties
			if (CompareAllowedCorsOrigins)
			{
				var xAllowedCorsOrigins = x.AllowedCorsOrigins ?? Enumerable.Empty<string>();
				var yAllowedCorsOrigins = y.AllowedCorsOrigins ?? Enumerable.Empty<string>();
				if (xAllowedCorsOrigins.ToHashSet().SetEquals(yAllowedCorsOrigins.ToHashSet()) == false)
					return false;
			}
			if (CompareAllowedGrantTypes)
			{
				var xAllowedGrantTypes = x.AllowedGrantTypes ?? Enumerable.Empty<string>();
				var yAllowedGrantTypes = y.AllowedGrantTypes ?? Enumerable.Empty<string>();
				if (xAllowedGrantTypes.ToHashSet().SetEquals(yAllowedGrantTypes.ToHashSet()) == false)
					return false;
			}
			if (CompareAllowedScopes)
			{
				var xAllowedScopes = x.AllowedScopes ?? Enumerable.Empty<string>();
				var yAllowedScopes = y.AllowedScopes ?? Enumerable.Empty<string>();
				if (xAllowedScopes.ToHashSet().SetEquals(yAllowedScopes.ToHashSet()) == false)
					return false;
			}
			if (ComparePostLogoutRedirectUris)
			{
				var xPostLogoutRedirectUris = x.PostLogoutRedirectUris ?? Enumerable.Empty<string>();
				var yPostLogoutRedirectUris = y.PostLogoutRedirectUris ?? Enumerable.Empty<string>();
				if (xPostLogoutRedirectUris.ToHashSet().SetEquals(yPostLogoutRedirectUris.ToHashSet()) == false)
					return false;
			}
			if (CompareRedirectUris)
			{
				var xRedirectUris = x.RedirectUris ?? Enumerable.Empty<string>();
				var yRedirectUris = y.RedirectUris ?? Enumerable.Empty<string>();
				if (xRedirectUris.ToHashSet().SetEquals(yRedirectUris.ToHashSet()) == false)
					return false;
			}

			if (CompareClientSecrets)
			{
				var secretsEqualityComparer = new SerializableSecretEqualityComparer();

				if (SetUtilities.AreSetsEqual(x.ClientSecrets, y.ClientSecrets, secretsEqualityComparer) == false)
					return false;
			}

			// Both instances should be considered equal.
			return true;
		}


		public int GetHashCode([DisallowNull] SerializableClient obj) {
			HashCode hashCode = new HashCode();
			hashCode.Add(obj.AllowAccessTokensViaBrowser);
			hashCode.Add(obj.ClientId);
			hashCode.Add(obj.ClientName);
			hashCode.Add(obj.RequireClientSecret);
			hashCode.Add(obj.RequireConsent);
			hashCode.Add(obj.RequirePkce);

			if (CompareAllowedCorsOrigins)
			{
				if (obj.AllowedCorsOrigins == null)
					hashCode.Add<IEnumerable<string>>(null);
				else
				{
					foreach (var allowedCorsOrigin in obj.AllowedCorsOrigins.OrderBy(allowedCorsOrigin => allowedCorsOrigin))
						hashCode.Add(allowedCorsOrigin);
				}
			}
			if (CompareAllowedGrantTypes)
			{
				if (obj.AllowedGrantTypes == null)
					hashCode.Add<IEnumerable<string>>(null);
				else
				{
					foreach (var allowedGrantType in obj.AllowedGrantTypes.OrderBy(allowedGrantType => allowedGrantType))
						hashCode.Add(allowedGrantType);
				}
			}
			if (CompareAllowedScopes)
			{
				if (obj.AllowedScopes == null)
					hashCode.Add<IEnumerable<string>>(null);
				else
				{
					foreach (var allowedScope in obj.AllowedScopes.OrderBy(allowedScope => allowedScope))
						hashCode.Add(allowedScope);
				}
			}
			if (CompareClientSecrets)
			{
				if (obj.ClientSecrets == null)
					hashCode.Add<IEnumerable<SerializableSecret>>(null);
				else
				{
					foreach (var clientSecret in obj.ClientSecrets.OrderBy(clientSecret => new { clientSecret.Description, clientSecret.Value } ))
					{
						hashCode.Add(clientSecret.Description);
						hashCode.Add(clientSecret.Expiration);
						hashCode.Add(clientSecret.IsValueHashed);
						hashCode.Add(clientSecret.Type);
						hashCode.Add(clientSecret.Value);
					}
				}
			}
			if (ComparePostLogoutRedirectUris)
			{
				if (obj.PostLogoutRedirectUris == null)
					hashCode.Add<IEnumerable<string>>(null);
				else
				{
					foreach (var postLogoutRedirectUri in obj.PostLogoutRedirectUris.OrderBy(postLogoutRedirectUri => postLogoutRedirectUri))
						hashCode.Add(postLogoutRedirectUri);
				}
			}
			if (CompareRedirectUris)
			{
				if (obj.RedirectUris == null)
					hashCode.Add<IEnumerable<string>>(null);
				else
				{
					foreach (var redirectUri in obj.RedirectUris.OrderBy(redirectUri => redirectUri))
						hashCode.Add(redirectUri);
				}
			}

			int result = hashCode.ToHashCode();
			return result;
		}
	}
}