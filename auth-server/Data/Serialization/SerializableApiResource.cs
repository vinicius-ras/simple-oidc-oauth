using IdentityServer4.EntityFramework.Entities;
using System.Collections.Generic;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="ApiResource"/> object.</summary>
	public class SerializableApiResource : SerializableResource
	{
		// INSTANCE PROPERTIES
		/// <summary>
		///     The API secret is used for the introspection endpoint. The API can authenticate
		///     with introspection using the API name and secret.
		/// </summary>
		public IEnumerable<SerializableApiResourceSecret> ApiSecrets { get; set; }
		/// <summary>Models the scopes this API resource allows.</summary>
		public ICollection<string> Scopes { get; set; }
		/// <summary>
		///     Signing algorithm for access token. If empty, will use the server default signing
		///     algorithm.
		/// </summary>
		public IEnumerable<string> AllowedAccessTokenSigningAlgorithms { get; set; }





		// INTERFACE OVERRIDES: IPolymorphicDiscriminator
		/// <inheritdoc/>
		public override string DiscriminatorValue => "api-resource";
	}
}