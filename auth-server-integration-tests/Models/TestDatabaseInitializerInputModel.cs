using SimpleOidcOauth.Data.Serialization;
using System.Collections.Generic;

namespace SimpleOidcOauth.Tests.Integration.Models
{
	/// <summary>Input model for the <see cref="Controllers.TestDatabaseInitializerServiceController.InitializeDatabase"/> endpoint.</summary>
	public class TestDatabaseInitializerInputModel
	{
		// INSTANCE PROPERTIES
		/// <summary>A collection of <see cref="SerializableClient"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<SerializableClient> Clients { get; set; }
		/// <summary>A collection of <see cref="SerializableApiScope"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<SerializableApiScope> ApiScopes { get; set; }
		/// <summary>A collection of <see cref="SerializableApiResource"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<SerializableApiResource> ApiResources { get; set; }
		/// <summary>A collection of <see cref="SerializableIdentityResource"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<SerializableIdentityResource> IdentityResources { get; set; }
		/// <summary>A collection of <see cref="SerializableTestUser"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<SerializableTestUser> Users { get; set; }
	}
}
