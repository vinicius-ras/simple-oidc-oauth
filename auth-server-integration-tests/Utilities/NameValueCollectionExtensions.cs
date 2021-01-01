using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SimpleOidcOauth.Tests.Integration.Utilities
{
	/// <summary>Extension methods for the <see cref="NameValueCollection"/> class.</summary>
	static class NameValueCollectionExtensions
	{
		/// <summary>Transforms a <see cref="NameValueCollection"/> into a dictionary of values.</summary>
		/// <param name="nameValueCollection">The <see cref="NameValueCollection"/> to be transformed into a dictionary.</param>
		/// <returns>Returns an <see cref="IDictionary{TKey, TValue}"/> representing the input <see cref="NameValueCollection"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the input <paramref name="nameValueCollection"/> instance is set to <c>null</c>.</exception>
		public static IDictionary<string, string> ToDictionary(this NameValueCollection nameValueCollection)
		{
			if (nameValueCollection == null)
				throw new ArgumentNullException(nameof(nameValueCollection));

			var result = new Dictionary<string, string>();
			foreach (var key in nameValueCollection.AllKeys)
				result.Add(key, nameValueCollection[key]);

			return result;
		}
	}
}