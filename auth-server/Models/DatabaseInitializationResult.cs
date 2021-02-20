using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using SimpleOidcOauth.Services;
using System.Collections.Generic;

namespace SimpleOidcOauth.Models
{
	/// <summary>Describes the results of a call to <see cref="DatabaseInitializerService.InitializeDatabaseAsync(IEnumerable{IdentityServer4.Models.Client}, IEnumerable{IdentityServer4.Models.ApiScope}, IEnumerable{IdentityServer4.Models.ApiResource}, IEnumerable{IdentityServer4.Models.IdentityResource}, IEnumerable{IdentityServer4.Test.TestUser})"/>.</summary>
	public class DatabaseInitializationResult
	{
		/// <summary>The entities of type <see cref="Client"/> that have been inserted into the database during the initialization operation.</summary>
		/// <value>An enumerable collection of inserted entities of type <see cref="Client"/>.</value>
		public IEnumerable<Client> InsertedClients { get; init; }
		/// <summary>The entities of type <see cref="ApiScope"/> that have been inserted into the database during the initialization operation.</summary>
		/// <value>An enumerable collection of inserted entities of type <see cref="ApiScope"/>.</value>
		public IEnumerable<ApiScope> InsertedApiScopes { get; init; }
		/// <summary>The entities of type <see cref="ApiResource"/> that have been inserted into the database during the initialization operation.</summary>
		/// <value>An enumerable collection of inserted entities of type <see cref="ApiResource"/>.</value>
		public IEnumerable<ApiResource> InsertedApiResources { get; init; }
		/// <summary>The entities of type <see cref="IdentityResource"/> that have been inserted into the database during the initialization operation.</summary>
		/// <value>An enumerable collection of inserted entities of type <see cref="IdentityResource"/>.</value>
		public IEnumerable<IdentityResource> InsertedIdentityResources { get; init; }
		/// <summary>The entities of type <see cref="IdentityUser"/> that have been inserted into the database during the initialization operation.</summary>
		/// <value>An enumerable collection of inserted entities of type <see cref="IdentityUser"/>.</value>
		public IEnumerable<IdentityUser> InsertedUsers { get; init; }
		/// <summary>The entities of type <see cref="Client"/> that have been updated in the database during the initialization operation.</summary>
		/// <value>An enumerable collection of updated entities of type <see cref="Client"/>.</value>
		public IEnumerable<Client> UpdatedClients { get; init; }
		/// <summary>The entities of type <see cref="ApiScope"/> that have been updated in the database during the initialization operation.</summary>
		/// <value>An enumerable collection of updated entities of type <see cref="ApiScope"/>.</value>
		public IEnumerable<ApiScope> UpdatedApiScopes { get; init; }
		/// <summary>The entities of type <see cref="ApiResource"/> that have been updated in the database during the initialization operation.</summary>
		/// <value>An enumerable collection of updated entities of type <see cref="ApiResource"/>.</value>
		public IEnumerable<ApiResource> UpdatedApiResources { get; init; }
		/// <summary>The entities of type <see cref="IdentityResource"/> that have been updated in the database during the initialization operation.</summary>
		/// <value>An enumerable collection of updated entities of type <see cref="IdentityResource"/>.</value>
		public IEnumerable<IdentityResource> UpdatedIdentityResources { get; init; }
		/// <summary>The entities of type <see cref="IdentityUser"/> that have been updated in the database during the initialization operation.</summary>
		/// <value>An enumerable collection of updated entities of type <see cref="IdentityUser"/>.</value>
		public IEnumerable<IdentityUser> UpdatedUsers { get; init; }
	}
}