using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleOidcOauth.Data.Serialization;

namespace SimpleOidcOauth.Tests.Integration.Data.Comparers
{
	/// <summary>An equality comparer for <see cref="SerializableApiScope"/> objects.</summary>
	class SerializableApiScopeEqualityComparer : SerializableResourceEqualityComparer, IEqualityComparer<SerializableApiScope>
	{
		// INTERFACE IMPLEMENTATION: IEqualityComparer<SerializableApiScope>
		public bool Equals(SerializableApiScope x, SerializableApiScope y)
		{
			// Basic null comparisons
			if (x == null && y == null)
				return true;
			else if (x == null || y == null)
				return false;

			// Compare base class
			if (base.Equals(x, y) == false)
				return false;

			// Compare simple properties
			if (x.Emphasize != y.Emphasize)
				return false;
			if (x.Required != y.Required)
				return false;

			// Both instances should be considered equal.
			return true;
		}


		public int GetHashCode([DisallowNull] SerializableApiScope obj)
		{
			var hashCode = new HashCode();
			hashCode.Add(obj.Description);
			hashCode.Add(obj.DiscriminatorValue);
			hashCode.Add(obj.DisplayName);
			hashCode.Add(obj.Emphasize);
			hashCode.Add(obj.Enabled);
			hashCode.Add(obj.Name);
			hashCode.Add(obj.Required);
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