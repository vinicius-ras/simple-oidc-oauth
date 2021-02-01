using IdentityServer4.Models;
using SimpleOidcOauth.Tests.Integration.Models.DTO;
using System.Collections.Generic;

namespace SimpleOidcOauth.Tests.Integration.Models
{
	/// <summary>Input model for the <see cref="Controllers.TestDatabaseInitializerServiceController.InitializeDatabase"/> endpoint.</summary>
	public class TestDatabaseInitializerInputModel
	{
		// INSTANCE PROPERTIES
		/// <summary>A collection of <see cref="ClientDto"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<ClientDto> Clients { get; init; }
		/// <summary>A collection of <see cref="ApiScope"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<ApiScope> ApiScopes { get; init; }
		/// <summary>A collection of <see cref="ApiResource"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<ApiResource> ApiResources { get; init; }
		/// <summary>A collection of <see cref="IdentityResource"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<IdentityResource> IdentityResources { get; init; }
		/// <summary>A collection of <see cref="UserDto"/> objects used to initialize the database with test data.</summary>
		public IEnumerable<UserDto> Users { get; init; }
	}
}
