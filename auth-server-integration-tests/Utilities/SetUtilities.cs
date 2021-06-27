using System.Collections.Generic;
using System.Linq;

namespace SimpleOidcOauth.Tests.Integration.Utilities
{
	/// <summary>Utilities for set comparisons.</summary>
	static class SetUtilities
	{
		/// <summary>Compares two sets of elements to verify if they are the equal.</summary>
		/// <remarks>
		///     This method ignores the ordering of elements in the sets, the repetition of elements in the sets, and the
		///     cardinalities of the sets. It considers two sets as equal if the difference between set "A" and set "B" is empty,
		///     and if the difference between set "B" and set "A" is also empty.
		/// </remarks>
		/// <param name="set1">
		///     The first set to be compared.
		///     A value of <c>null</c> will be considered equivalent to an empty set.
		/// </param>
		/// <param name="set2">
		///     The second set to be compared.
		///     A value of <c>null</c> will be considered equivalent to an empty set.
		/// </param>
		/// <param name="equalityComparer">
		///     An optional equality comparer for the elements in the sets.
		///     If not specified, <see cref="EqualityComparer{T}.Default"/> will be used.
		/// </param>
		/// <typeparam name="T">The type of elements in the set.</typeparam>
		/// <returns>
		///     Returns a flag indicating if the sets are to be considered equal, according to the description of equality provided
		///     in the remarks section of this documentation.
		/// </returns>
		public static bool AreSetsEqual<T>(IEnumerable<T> set1, IEnumerable<T> set2, IEqualityComparer<T> equalityComparer = null)
			where T : class
		{
			equalityComparer ??= EqualityComparer<T>.Default;
			set1 ??= Enumerable.Empty<T>();
			set2 ??= Enumerable.Empty<T>();

			HashSet<T> hashSet1 = set1.ToHashSet(equalityComparer),
				hashSet2 = set2.ToHashSet(equalityComparer);

			return (hashSet1.Except(hashSet2, equalityComparer).Count() == 0)
				&& (hashSet2.Except(hashSet1, equalityComparer).Count() == 0);
		}
	}
}