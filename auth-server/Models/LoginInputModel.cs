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
		/// <summary>User's password.</summary>
		/// <value>The password for the user who is trying to log in.</value>
		[Required]
		public string Password { get; set; }
		/// <summary>The URL to which the user will be redirected to after login.</summary>
		/// <value>The return URL to which the user will be redirected to after a successful login.</value>
		public string ReturnUrl { get; set; }
	}
}