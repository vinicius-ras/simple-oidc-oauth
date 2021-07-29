using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleOidcOauth.Data.Serialization;

namespace SimpleOidcOauth.Tests.Integration.Data.Comparers
{
	/// <summary>An equality comparer for <see cref="SerializableSecret"/> objects.</summary>
	abstract class SerializableSecretEqualityComparer : IEqualityComparer<SerializableSecret>
	{
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
			if (x.IsValueHashed != y.IsValueHashed)
				return false;
			if (x.Type != y.Type)
				return false;
			if (x.Value != y.Value)
				return false;

			// Both instances should be considered equal.
			return true;
		}


		/// <inheritdoc/>
		public int GetHashCode([DisallowNull] SerializableSecret obj) =>
			HashCode.Combine(obj.Description, obj.Expiration, obj.IsValueHashed, obj.Type, obj.Value);
	}
}