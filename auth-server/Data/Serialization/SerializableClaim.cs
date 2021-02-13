using System.Security.Claims;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>A serializable version of a <see cref="Claim"/> object.</summary>
	public class SerializableClaim
	{
		/// <summary>Gets the claim type of the claim.</summary>
		/// <value>The claim type.</value>
		public string Type { get; set; }
		/// <summary>Gets the value of the claim.</summary>
		/// <value>The claim value.</value>
		public string Value { get; set; }
	}
}