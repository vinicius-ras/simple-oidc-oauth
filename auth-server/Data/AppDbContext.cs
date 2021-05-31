using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleOidcOauth.Data.Security;

namespace SimpleOidcOauth.Data
{
	/// <summary>
	///     Implementation for the application's <see cref="DbContext" /> class for accessing the main
	///     application's database, while supporting ASP.NET Core Identity entities.
	/// </summary>
	public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
	{
		/// <summary>Constructor.</summary>
		/// <param name="options">Database configuration options.</param>
		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options)
		{
		}
	}
}