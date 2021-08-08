using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleOidcOauth.Data.Serialization;

namespace SimpleOidcOauth.Tests.Integration.Data.Comparers
{
	/// <summary>An equality comparer for <see cref="SerializableSecret"/> objects.</summary>
	class SerializableSecretEqualityComparer : IEqualityComparer<SerializableSecret>
	{
		// INSTANCE PROPERTIES
		/// <summary>A flag indicating if the <see cref="SerializableSecret.IsValueHashed"/> properties should be compared.</summary>
		public bool CompareIsValueHashed { init; get; } = true;
		/// <summary>A flag indicating if the <see cref="SerializableSecret.Value"/> properties should be compared.</summary>
		public bool CompareValue { init; get; } = true;





		// INTERFACE IMPLEMENTATION: IEqualityComparer<SerializableSecret>
		/// <inheritdoc/>
		public bool Equals(SerializableSecret x, SerializableSecret y)
		{
			// Basic null comparisons
			if (x == null && y == null)
				return true;
			else if (x == null || y == null)
				return false;

			// Compare simple properties
			if (x.Description != y.Description)
				return false;
			if (x.Expiration != y.Expiration)
				return false;
			if (x.Type != y.Type)
				return false;

			if (CompareIsValueHashed && x.IsValueHashed != y.IsValueHashed)
				return false;
			if (CompareValue && x.Value != y.Value)
				return false;

			// Both instances should be considered equal.
			return true;
		}


		/// <inheritdoc/>
		public int GetHashCode([DisallowNull] SerializableSecret obj)
		{
			var hashCode = new HashCode();
			hashCode.Add(obj.Description);
			hashCode.Add(obj.Expiration);
			hashCode.Add(obj.Type);
			if (CompareIsValueHashed)
				hashCode.Add(obj.IsValueHashed);
			if (CompareValue)
				hashCode.Add(obj.Value);

			int result = hashCode.ToHashCode();
			return result;
		}
	}
}