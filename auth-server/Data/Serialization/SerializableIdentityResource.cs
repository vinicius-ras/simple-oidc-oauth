using IdentityServer4.Models;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="IdentityResource"/> object.</summary>
	public class SerializableIdentityResource : SerializableResource
	{
		/// <summary>
		///     Specifies whether the user can de-select the scope on the consent screen.
		///     Defaults to <c>false</c>.
		/// </summary>
		public bool Required { get; set; }
		/// <summary>
		///     Specifies whether the consent screen will emphasize this scope (if the consent
		///     screen wants to implement such a feature). Use this setting for sensitive or
		///     important scopes.
		///     Defaults to <c>false</c>.
		/// </summary>
		public bool Emphasize { get; set; }
	}
}