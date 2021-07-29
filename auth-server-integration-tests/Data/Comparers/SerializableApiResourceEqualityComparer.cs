using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleOidcOauth.Data.Serialization;

namespace SimpleOidcOauth.Tests.Integration.Data.Comparers
{
	/// <summary>An equality comparer for <see cref="SerializableApiResource"/> objects.</summary>
	class SerializableApiResourceEqualityComparer : SerializableResourceEqualityComparer, IEqualityComparer<SerializableApiResource>
	{
		// INSTANCE PROPERTIES
		/// <summary>A flag indicating if the <see cref="SerializableApiResource.AllowedAccessTokenSigningAlgorithms"/> should be compared.</summary>
		public bool CompareAllowedAccessTokenSigningAlgorithms { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableApiResource.ApiSecrets"/> should be compared.</summary>
		public bool CompareApiSecrets { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableApiResource.Scopes"/> should be compared.</summary>
		public bool CompareScopes { init; get; } = true;





		// INTERFACE IMPLEMENTATION: IEqualityComparer<SerializableApiResource>
		public bool Equals(SerializableApiResource x, SerializableApiResource y)
		{
			// Basic null comparisons
			if (x == null && y == null)
				return true;
			else if (x == null || y == null)
				return false;

			// Compare base class
			if (base.Equals(x, y) == false)
				return false;

			// Compare signing algorithms
			if (CompareAllowedAccessTokenSigningAlgorithms)
			{
				var xSigningAlgorithms = x.AllowedAccessTokenSigningAlgorithms ?? Enumerable.Empty<string>();
				var ySigningAlgorithms = y.AllowedAccessTokenSigningAlgorithms ?? Enumerable.Empty<string>();
				if (xSigningAlgorithms.ToHashSet().SetEquals(ySigningAlgorithms.ToHashSet()) == false)
					return false;
			}

			// Compare API secrets
			if (CompareApiSecrets)
			{
				var xSecrets = x.ApiSecrets ?? Enumerable.Empty<SerializableApiResourceSecret>();
				var ySecrets = y.ApiSecrets ?? Enumerable.Empty<SerializableApiResourceSecret>();
				var secretsComparer = new SerializableApiResourceSecretEqualityComparer();
				if (xSecrets.ToHashSet(secretsComparer).SetEquals(ySecrets.ToHashSet(secretsComparer)) == false)
					return false;
			}

			// Compare scopes
			if (CompareScopes)
			{
				var xScopes = x.Scopes ?? Enumerable.Empty<string>();
				var yScopes = y.Scopes ?? Enumerable.Empty<string>();
				if (xScopes.ToHashSet().SetEquals(yScopes.ToHashSet()) == false)
					return false;
			}

			// Both instances should be considered equal.
			return true;
		}


		public int GetHashCode([DisallowNull] SerializableApiResource obj)
		{
			var hashCode = new HashCode();
			hashCode.Add(obj.AllowedAccessTokenSigningAlgorithms);
			hashCode.Add(obj.ApiSecrets);
			hashCode.Add(obj.Description);
			hashCode.Add(obj.DiscriminatorValue);
			hashCode.Add(obj.DisplayName);
			hashCode.Add(obj.Enabled);
			hashCode.Add(obj.Name);
			hashCode.Add(obj.ShowInDiscoveryDocument);

			if (CompareProperties)
			{
				if (obj.Properties == null)
					hashCode.Add<IDictionary<string,string>>(null);
				else
				{
					foreach (var prop in obj.Properties.OrderBy(prop => prop.Key))
						hashCode.Add($"{prop.Key} = {prop.Value}");
				}
			}
			if (CompareUserClaims)
			{
				if (obj.UserClaims == null)
					hashCode.Add<IEnumerable<string>>(null);
				else
				{
					foreach (var claim in obj.UserClaims.OrderBy(claim => claim))
						hashCode.Add(claim);
				}
			}
			if (CompareScopes)
			{
				if (obj.Scopes == null)
					hashCode.Add<ICollection<string>>(null);
				else
				{
					foreach (var scope in obj.Scopes.OrderBy(scope => scope))
						hashCode.Add(scope);
				}
			}

			return hashCode.ToHashCode();
		}
	}
}