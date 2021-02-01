using IdentityServer4.EntityFramework.Entities;
using SimpleOidcOauth.Tests.Integration.Models.DTO;
using System.Collections.Generic;

namespace SimpleOidcOauth.Tests.Integration.Models
{
	/// <summary>Output model for the <see cref="Controllers.TestDatabaseInitializerServiceController.GetAllRegisteredData"/> endpoint.</summary>
	public class TestDatabaseInitializerOutputModel
	{
		// INSTANCE PROPERTIES
		/// <summary>A collection of all <see cref="ClientDto"/> objects which were obtained from the database.</summary>
		public IEnumerable<ClientDto> Clients { get; init; }
		/// <summary>A collection of all <see cref="ApiScope"/> objects which were obtained from the database.</summary>
		public IEnumerable<ApiScope> ApiScopes { get; init; }
		/// <summary>A collection of all <see cref="ApiResource"/> objects which were obtained from the database.</summary>
		public IEnumerable<ApiResource> ApiResources { get; init; }
		/// <summary>A collection of all <see cref="IdentityResource"/> objects which were obtained from the database.</summary>
		public IEnumerable<IdentityResource> IdentityResources { get; init; }
		/// <summary>A collection of all <see cref="UserDto"/> objects which were obtained from the database.</summary>
		public IEnumerable<UserDto> Users { get; init; }
	}
}
