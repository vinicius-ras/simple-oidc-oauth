using Microsoft.AspNetCore.Identity;

namespace SimpleOidcOauth.Data.Security
{
	/// <summary>
	///     <para>
	///         Represents a user role in the application. Users can belong to roles in order to group them,
	///         while roles can be associated to claims that define permissions for operations their respective users
	///         can perform in the application.
	///     </para>
	///     <para>This class is currently a placeholder to allow for future extensibility, if necessary.</para>
	/// </summary>
	public class ApplicationRole : IdentityRole
	{
	}
}