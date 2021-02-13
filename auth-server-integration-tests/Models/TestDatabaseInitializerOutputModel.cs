using IdentityServer4.EntityFramework.Entities;
using SimpleOidcOauth.Data.Serialization;
using System.Collections.Generic;

namespace SimpleOidcOauth.Tests.Integration.Models
{
	/// <summary>Output model for the <see cref="Controllers.TestDatabaseInitializerServiceController.GetAllRegisteredData"/> endpoint.</summary>
	public class TestDatabaseInitializerOutputModel
	{
		// INSTANCE PROPERTIES
		/// <summary>A collection of all <see cref="ClientDto"/> objects which were obtained from the database.</summary>
		public IEnumerable<SerializableClient> Clients { get; init; }
		/// <summary>A collection of all <see cref="ApiScope"/> objects which were obtained from the database.</summary>
		public IEnumerable<SerializableApiScope> ApiScopes { get; init; }
		/// <summary>A collection of all <see cref="ApiResource"/> objects which were obtained from the database.</summary>
		public IEnumerable<SerializableApiResource> ApiResources { get; init; }
		/// <summary>A collection of all <see cref="IdentityResource"/> objects which were obtained from the database.</summary>
		public IEnumerable<SerializableIdentityResource> IdentityResources { get; init; }
		/// <summary>A collection of all <see cref="UserDto"/> objects which were obtained from the database.</summary>
		public IEnumerable<SerializableTestUser> Users { get; init; }
	}
}
