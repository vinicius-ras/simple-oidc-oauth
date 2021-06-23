using IdentityServer4.Models;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="ApiScope"/> object.</summary>
	public class SerializableApiScope : SerializableResource
	{
		// INSTANCE PROPERTIES
		/// <summary>
		///     Specifies whether the user can de-select the scope on the consent screen. Defaults
		///     to false.
		/// </summary>
		public bool Required { get; set; }
		/// <summary>
		///     Specifies whether the consent screen will emphasize this scope. Use this setting
		///     for sensitive or important scopes. Defaults to false.
		/// </summary>
		public bool Emphasize { get; set; }





		// INTERFACE OVERRIDES: IPolymorphicDiscriminator
		/// <inheritdoc/>
		public override string DiscriminatorValue => "api-scope";
	}
}