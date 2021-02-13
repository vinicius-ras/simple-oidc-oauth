using System.Collections.Generic;
using IdentityServer4.Test;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="TestUser"/> object.</summary>
	public class SerializableTestUser
	{
		/// <summary>Gets or sets the subject identifier.</summary>
		public string SubjectId { get; set; }
		/// <summary>Gets or sets the username.</summary>
		public string Username { get; set; }
		/// <summary>Gets or sets the password.</summary>
		public string Password { get; set; }
		/// <summary>Gets or sets the email.</summary>
		public string Email { get; set; }
		/// <summary>Gets or sets the "email confirmed" flag.</summary>
		public bool EmailConfirmed { get; set; }

		/// <summary>Gets or sets the claims.</summary>
		public IEnumerable<SerializableClaim> Claims { get; set; }
	}
}