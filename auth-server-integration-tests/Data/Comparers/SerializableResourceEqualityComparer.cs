using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleOidcOauth.Data.Serialization;

namespace SimpleOidcOauth.Tests.Integration.Data.Comparers
{
	/// <summary>An equality comparer for <see cref="SerializableResource"/> objects.</summary>
	class SerializableResourceEqualityComparer : IEqualityComparer<SerializableResource>
	{
		// INSTANCE PROPERTIES
		/// <summary>A flag indicating if the <see cref="SerializableResource.Properties"/> should be compared.</summary>
		public bool CompareProperties { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableResource.UserClaims"/> should be compared.</summary>
		public bool CompareUserClaims { init; get; } = true;





		// INTERFACE IMPLEMENTATION: IEqualityComparer<SerializableResource>
		public bool Equals(SerializableResource x, SerializableResource y)
		{
			// Basic null comparisons
			if (x == null && y == null)
				return true;
			else if (x == null || y == null)
				return false;

			// Compare simple properties
			if (x.Description != y.Description)
				return false;
			if (x.DisplayName != y.DisplayName)
				return false;
			if (x.Enabled != y.Enabled)
				return false;
			if (x.Name != y.Name)
				return false;
			if (x.ShowInDiscoveryDocument != y.ShowInDiscoveryDocument)
				return false;

			// Compare properties
			if (CompareProperties)
			{
				if (x.Properties?.Count != y.Properties?.Count)
					return false;
				foreach (var xPropertiesEntry in x.Properties ?? Enumerable.Empty<KeyValuePair<string, string>>())
				{
					var xPropertiesEntryKey = xPropertiesEntry.Key;

					string yPropertiesValue = null;
					if (y.Properties?.TryGetValue(xPropertiesEntryKey, out yPropertiesValue) == false)
						return false;
					if (xPropertiesEntry.Value != yPropertiesValue)
						return false;
				}

				if (x.UserClaims?.Count() != y.UserClaims?.Count())
					return false;
			}

			// Compare user claims
			if (CompareUserClaims)
			{
				var xUserClaims = x.UserClaims ?? Enumerable.Empty<string>();
				var yUserClaims = y.UserClaims ?? Enumerable.Empty<string>();
				if (xUserClaims.ToHashSet().SetEquals(yUserClaims.ToHashSet()) == false)
					return false;
			}

			// Both instances should be considered equal.
			return true;
		}


		public int GetHashCode([DisallowNull] SerializableResource obj)
		{
			var hashCode = new HashCode();
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

			return hashCode.ToHashCode();
		}
	}
}