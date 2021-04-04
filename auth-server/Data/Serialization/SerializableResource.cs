using System.Collections.Generic;
using IdentityServer4.Models;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="Resource"/> object.</summary>
	public class SerializableResource
	{
		/// <summary>Indicates if this resource is enabled.</summary>
		public bool Enabled { get; set; }
		/// <summary>The unique name of the resource.</summary>
		public string Name { get; set; }
		/// <summary>Display name of the resource.</summary>
		public string DisplayName { get; set; }
		/// <summary>Description of the resource.</summary>
		public string Description { get; set; }
		/// <summary>Specifies whether this scope is shown in the discovery document.</summary>
		public bool ShowInDiscoveryDocument { get; set; }
		/// <summary>
		///     List of associated user claims that should be included when this resource is
		///     requested.
		/// </summary>
		public IEnumerable<string> UserClaims { get; set; }
		/// <summary>Gets or sets the custom properties for the resource.</summary>
		/// <value>The properties.</value>
		public IDictionary<string, string> Properties { get; set; }
	}
}