using IdentityServer4.EntityFramework.Entities;

namespace SimpleOidcOauth.Data.Serialization
{
	/// <summary>Serialized data of a API Resource secret.</summary>
	/// <remarks>This class is basically a serializable version of the <see cref="ClientSecret"/> class.</remarks>
	public class SerializableApiResourceSecret : SerializableSecret
	{
		/// <summary>The database ID for the client this secret belongs to.</summary>
		public int? ApiResourceDatabaseId { get; set; }
	}
}