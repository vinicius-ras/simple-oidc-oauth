using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SimpleOidcOauth.Data.Serialization;

namespace SimpleOidcOauth.Tests.Integration.Data.Comparers
{
	/// <summary>An equality comparer for <see cref="SerializableClientSecret"/> objects.</summary>
	class SerializableClientSecretEqualityComparer : SerializableSecretEqualityComparer, IEqualityComparer<SerializableClientSecret>
	{
		// INTERFACE IMPLEMENTATION: IEqualityComparer<SerializableClientSecret>
		/// <inheritdoc/>
		public bool Equals(SerializableClientSecret x, SerializableClientSecret y) => base.Equals(x, y);


		/// <inheritdoc/>
		public int GetHashCode([DisallowNull] SerializableClientSecret obj) => base.GetHashCode(obj);
	}
}