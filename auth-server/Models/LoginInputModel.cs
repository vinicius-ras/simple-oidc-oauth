using SimpleOidcOauth.Controllers;
using System.ComponentModel.DataAnnotations;

namespace SimpleOidcOauth.Models
{
	/// <summary>Input model representing request data to be received by the <see cref="AccountController.Login(LoginInputModel)" /> endpoint.</summary>
	public class LoginInputModel
	{
		/// <summary>User's email.</summary>
		/// <value>The email for the user who is trying to log in.</value>
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		/// <summary>User's password.</summary>
		/// <value>The password for the user who is trying to log in.</value>
		public string Password { get; set; }
	}
}