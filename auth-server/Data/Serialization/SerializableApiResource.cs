using IdentityServer4.Models;
using System.Collections.Generic;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="ApiResource"/> object.</summary>
	public class SerializableApiResource : SerializableResource
	{
		/// <summary>
		///     The API secret is used for the introspection endpoint. The API can authenticate
		///     with introspection using the API name and secret.
		/// </summary>
		public IEnumerable<SerializableSecret> ApiSecrets { get; set; }
		/// <summary>Models the scopes this API resource allows.</summary>
		public ICollection<string> Scopes { get; set; }
		/// <summary>
		///     Signing algorithm for access token. If empty, will use the server default signing
		///     algorithm.
		/// </summary>
		public IEnumerable<string> AllowedAccessTokenSigningAlgorithms { get; set; }
	}
}