using IdentityServer4.Models;
using System;
using System.ComponentModel.DataAnnotations;

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
		[Required]
		public string Value { get; set; }
		/// <summary>Gets or sets an expiration date/time for the secret.</summary>
		/// <value>The expiration date/time.</value>
		public DateTime? Expiration { get; set; }
		/// <summary>Gets or sets the secret's type.</summary>
		/// <value>The secret's type.</value>
		[Required]
		public string Type { get; set; }
		/// <summary>
		///     A flag indicating if the <see cref="Value"/> property is either in hashed or in plaintext format.
		///     This property is transient (not saved to the database).
		/// </summary>
		/// <value>
		///     <para>
		///         This property will be set to <c>true</c> if the <see cref="Value"/> property contains a hashed value, or <c>false</c> if it
		///         contains a plaintext secret value.
		///     </para>
		///     <para>A value of <c>null</c> indicates that the secret's value hashing state is currently indetermined - it might or might not be hashed.</para>
		/// </value>
		public bool? IsValueHashed { get; set; }
	}
}