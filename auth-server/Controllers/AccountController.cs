using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using System.Linq;
using SimpleOidcOauth.Models;
using Microsoft.AspNetCore.Authorization;
using IdentityServer4;
using System.Security.Claims;
using IdentityModel;

namespace SimpleOidcOauth.Controllers
{
	/// <summary>
	///    Controller which deals with matters related to user accounts.
	///    Such matters include user account registration, login/logout procedures, and general user management functionalities.
	///
	///    Notice this class uses the default name expected by ASP.NET Core default configurations for a controller that manages
	///    user account information.
	/// </summary>
	[Route("/api/account")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		// PRIVATE FIELDS
		/// <summary>Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</summary>
		private readonly ILogger<AccountController> _logger;
		/// <summary>Container-injected instance for the <see cref="SignInManager{TUser}" /> service.</summary>
		private readonly SignInManager<IdentityUser> _signInManager;
		/// <summary>Container-injected instance for the <see cref="UserManager{TUser}" /> service.</summary>
		private readonly UserManager<IdentityUser> _userManager;





		// PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		/// <param name="signInManager">Container-injected instance for the <see cref="SignInManager{TUser}" /> service.</param>
		/// <param name="userManager">Container-injected instance for the <see cref="UserManager{TUser}" /> service.</param>
		public AccountController(ILogger<AccountController> logger, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
		{
			_logger = logger;
			_signInManager = signInManager;
			_userManager = userManager;
		}



		/// <summary>Endpoint called to perform a user sign in, via the input of user credentials.</summary>
		/// <param name="inputData">User credentials data sent to this endpoint.</param>
		/// <returns>
		///     Returns an <see cref="IActionResult" /> object representing the server's response to the client.
		///     In case of success, an HTTP 200 (Ok) will be returned, wrapping a <see cref="LoginOutputModel" /> instance.
		///     In case of login failure (e.g., due to invalid credentials), an HTTP 401 (Unauthorized) response will be returned.
		/// </returns>
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginInputModel inputData)
		{
			// Sign out the user, if he/she is currently logged in
			var loggedInUser = this.User.Identities.Any(i => i.IsAuthenticated);
			if (loggedInUser)
				await _signInManager.SignOutAsync();

			// Check if the given user exists
			var user = await _userManager.FindByEmailAsync(inputData.Email);
			if (user == null)
				return Unauthorized();

			// Try to sign the user in
			var signInResult = await _signInManager.PasswordSignInAsync(user, inputData.Password, false, false);
			return signInResult.Succeeded
				? Ok(new LoginOutputModel {
					Id = user.Id,
					Name = user.UserName,
					Email = user.Email,
				})
				: (IActionResult) Unauthorized();
		}



		/// <summary>Endpoint to be called to sign a user out.</summary>
		/// <returns>
		///     Returns an <see cref="IActionResult" /> object representing the server's response to the client.
		///     This endpoint always returns an HTTP 200 (Ok) response.
		/// </returns>
		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return Ok();
		}



		/// <summary>
		///     Endpoint used to verify if the user has a valid session established with the authentication/authorization server.
		///
		///     The validity of the user's session is determined by the presence of a valid authentication, established by a cookie
		///     generated by the ASP.NET Core Identity framework when the user signs in (<see cref="Login(LoginInputModel)"/> endpoint).
		/// </summary>
		/// <returns>
		///     Returns an <see cref="IActionResult" /> object representing the server's response to the client.
		///
		///     This endpoint will return an HTTP 401 (Unauthorized) if the user doesn't have a valid session, indicating that it is necessary to restablish
		///     a session (reauthenticating though client's login) for accessing protected endpoints.
		///
		///     In case of success, an HTTP 200 (Ok) will be returned, wrapping a <see cref="LoginOutputModel" /> instance.
		/// </returns>
		[Authorize]
		[HttpGet("check-login")]
		public IActionResult CheckLogin()
		{
			var userIdentity = User.Identities
				.First(id => id.AuthenticationType.Equals(IdentityConstants.ApplicationScheme, System.StringComparison.Ordinal));
			var result = new LoginOutputModel {
				Id = userIdentity.Claims.First(c => c.Type == JwtClaimTypes.Subject).Value,
				Name = userIdentity.Claims.First(c => c.Type == JwtClaimTypes.Name).Value,
				Email = userIdentity.Claims.First(c => c.Type == JwtClaimTypes.Email).Value,
			};
			return Ok(result);
		}
	}
}