using System.ComponentModel.DataAnnotations;
using SimpleOidcOauth.Controllers;

namespace SimpleOidcOauth.Models
{
	/// <summary>Input model representing request data to be received by the <see cref="AccountController.Register(AccountRegisterInputModel)" /> endpoint.</summary>
	public class AccountRegisterInputModel {
		/// <summary>New user's username.</summary>
		/// <value>An email address to be used by the new user.</value>
		/// <example>alice-smith.1989</example>
		[Required, RegularExpression(@"^[a-zA-Z_][a-zA-Z_0-9-.]+$")]
		public string UserName { get; set; }
		/// <summary>New user's email.</summary>
		/// <value>An email address to be used by the new user.</value>
		/// <example>alice@fake-mail-domain-283182b.com</example>
		[Required, EmailAddress]
		public string Email { get; set; }
		/// <summary>Password which will be used by the new user.</summary>
		/// <value>
		///     A string which will be registered as the new user's password.
		///     Notice that most validation rules for this field are currently implemented
		///     by third party libraries (IdentityServer4).
		/// </value>
		/// <example>h4X0rP4ssw0rDL0l</example>
		[Required]
		public string Password { get; set; }
	}
}