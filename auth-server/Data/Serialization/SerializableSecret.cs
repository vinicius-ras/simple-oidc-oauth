using IdentityServer4.Models;
using System;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="Secret"/> object.</summary>
	public class SerializableSecret
	{
		/// <summary>Gets or sets a description for the secret.</summary>
		/// <value>The description.</value>
		public string Description { get; set; }
		//
		// Summary:
		//     Gets or sets the value.
		//
		// Value:
		//     The value.
		/// <summary>Gets or sets a value for the secret.</summary>
		/// <value>The value.</value>
		public string Value { get; set; }
		/// <summary>Gets or sets an expiration date/time for the secret.</summary>
		/// <value>The expiration date/time.</value>
		public DateTime? Expiration { get; set; }
		/// <summary>Gets or sets the secret's type.</summary>
		/// <value>The secret's type.</value>
		public string Type { get; set; }
	}
}