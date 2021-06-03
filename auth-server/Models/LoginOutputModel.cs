using SimpleOidcOauth.Data.Serialization;
using System.Collections.Generic;

namespace SimpleOidcOauth.Models
{
	/// <summary>Output model representing response data returned by the <see cref="SimpleOidcOauth.Controllers.AccountController.Login(LoginInputModel)" /> endpoint.</summary>
	public class LoginOutputModel
	{
		/// <summary>User's identifier.</summary>
		/// <value>An unique identifier representing the user that has been logged in.</value>
		/// <example>bc3d6721-453d-4d73-b296-7141bfb50710</example>
		public string Id { get; init; }
		/// <summary>User's name.</summary>
		/// <value>The username for the user that has been logged in.</value>
		/// <example>alice</example>
		public string Name { get; init; }
		/// <summary>User's email.</summary>
		/// <value>The email for the user that has been logged in.</value>
		/// <example>alice-mvc@my-fake-app.com</example>
		public string Email { get; init; }
		/// <summary>The URL to which the user should be redirected to.</summary>
		/// <value>The URL to which the user should be redirected to after a successful login.</value>
		/// <example>https://my-application/sign-in-callback/?state=something&amp;nonce=123abc</example>
		public string ReturnUrl { get; init; }
		/// <summary>The claims the logged-in user has.</summary>
		/// <value>An enumerable collection of <see cref="SerializableClaim"/> objects representing the user's claims, and their respective values.</value>
		public IEnumerable<SerializableClaim> Claims { get; init; }
	}
}