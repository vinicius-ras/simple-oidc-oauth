using System.ComponentModel.DataAnnotations;

namespace SimpleOidcOauth.Models
{
	/// <summary>Output model representing response data returned by the <see cref="AccountController.Login(LoginInputModel)" /> endpoint.</summary>
	public class LoginOutputModel
	{
		/// <summary>User's identifier.</summary>
		/// <value>An unique identifier representing the user that has been logged in.</value>
		public string Id { get; set; }
		/// <summary>User's name.</summary>
		/// <value>The username for the user that has been logged in.</value>
		public string Name { get; set; }
		/// <summary>User's email.</summary>
		/// <value>The email for the user that has been logged in.</value>
		public string Email { get; set; }
	}
}