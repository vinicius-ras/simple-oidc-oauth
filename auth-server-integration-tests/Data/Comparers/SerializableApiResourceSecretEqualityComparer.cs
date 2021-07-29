using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SimpleOidcOauth.Data.Serialization;

namespace SimpleOidcOauth.Tests.Integration.Data.Comparers
{
	/// <summary>An equality comparer for <see cref="SerializableApiResourceSecret"/> objects.</summary>
	class SerializableApiResourceSecretEqualityComparer : SerializableSecretEqualityComparer, IEqualityComparer<SerializableApiResourceSecret>
	{
		// INTERFACE IMPLEMENTATION: IEqualityComparer<SerializableApiResourceSecret>
		/// <inheritdoc/>
		public bool Equals(SerializableApiResourceSecret x, SerializableApiResourceSecret y) => base.Equals(x, y);


		/// <inheritdoc/>
		public int GetHashCode([DisallowNull] SerializableApiResourceSecret obj) => base.GetHashCode(obj);
	}
}