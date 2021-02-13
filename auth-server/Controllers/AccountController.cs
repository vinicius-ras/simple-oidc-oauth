using IdentityModel;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleOidcOauth.Data;
using SimpleOidcOauth.Data.Configuration;
using SimpleOidcOauth.Models;
using SimpleOidcOauth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleOidcOauth.Controllers
{
	/// <summary>
	///    Controller which deals with matters related to user accounts.
	///    Such matters include user account registration, login/logout procedures, and general user management functionalities.
	///
	///    Notice this class uses the default name expected by ASP.NET Core default configurations for a controller that manages
	///    user account information.
	/// </summary>
	[Route(AppEndpoints.AccountControllerUri)]
	[ApiController]
	public class AccountController : ControllerBase
	{
		// CONSTANTS AND STATIC READONLY
		/// <summary>
		///     Maps the types of errors that can happen during user's registration to each
		///     field to which these types of errors should be associated, allowing errors to be safely returned to client applications.
		/// </summary>
		/// <value>
		///     <para>A map associating the types of the possibly returned errors to a field, so that errors can be displayed correctly in the client application's UI.</para>
		///     <para>
		///         The map keys refer to the type of errors that can be returned by a call to <see cref="UserManager{TUser}.CreateAsync(TUser, string)"/>.
		///         The error names can be extracted from the <see cref="IdentityErrorDescriber"/> class to enable "type-safe" error checkings for the error names.
		///     </para>
		///     <para>
		///         The map values refer to the fields that are required to be sent to the <see cref="Register(AccountRegisterInputModel)"/> action/endpoint.
		///         These field names are described by and can be extracted from the <see cref="AccountRegisterInputModel"/> class.
		///     </para>
		/// </value>
		private static readonly IDictionary<string, string> UserRegistrationErrorCodeToFieldMap = new Dictionary<string, string>
		{
				{ nameof(IdentityErrorDescriber.DuplicateEmail), nameof(AccountRegisterInputModel.Email) },
				{ nameof(IdentityErrorDescriber.InvalidEmail), nameof(AccountRegisterInputModel.Email) },
				{ nameof(IdentityErrorDescriber.DuplicateUserName), nameof(AccountRegisterInputModel.Email) },
				{ nameof(IdentityErrorDescriber.InvalidUserName), nameof(AccountRegisterInputModel.Email) },
				{ nameof(IdentityErrorDescriber.PasswordRequiresDigit), nameof(AccountRegisterInputModel.Password) },
				{ nameof(IdentityErrorDescriber.PasswordRequiresLower), nameof(AccountRegisterInputModel.Password) },
				{ nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric), nameof(AccountRegisterInputModel.Password) },
				{ nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars), nameof(AccountRegisterInputModel.Password) },
				{ nameof(IdentityErrorDescriber.PasswordRequiresUpper), nameof(AccountRegisterInputModel.Password) },
				{ nameof(IdentityErrorDescriber.PasswordTooShort), nameof(AccountRegisterInputModel.Password) },
		};
		/// <summary>The name of the field to be associated with a given error, when that error's code has no associations described in the <see cref="UserRegistrationErrorCodeToFieldMap"/>.</summary>
		private const string UnknownErrorCodeFieldName = "_UNKNOWN_FIELDS";





		// PRIVATE FIELDS
		/// <summary>Container-injected application configurations.</summary>
		private readonly AppConfigs _appConfigs;
		/// <summary>Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</summary>
		private readonly ILogger<AccountController> _logger;
		/// <summary>Container-injected instance for the <see cref="SignInManager{TUser}" /> service.</summary>
		private readonly SignInManager<IdentityUser> _signInManager;
		/// <summary>Container-injected instance for the <see cref="UserManager{TUser}" /> service.</summary>
		private readonly UserManager<IdentityUser> _userManager;
		/// <summary>Container-injected instance for the <see cref="IIdentityServerInteractionService" /> service.</summary>
		private readonly IIdentityServerInteractionService _identServerInteractionService;
		/// <summary>Container-injected instance for the <see cref="IEmailService" /> service.</summary>
		private readonly IEmailService _emailService;





		// PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="appConfigs">Container-injected instance for the <see cref="IOptions{TOptions}" /> service.</param>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		/// <param name="signInManager">Container-injected instance for the <see cref="SignInManager{TUser}" /> service.</param>
		/// <param name="userManager">Container-injected instance for the <see cref="UserManager{TUser}" /> service.</param>
		/// <param name="identServerInteractionService">Container-injected instance for the <see cref="IIdentityServerInteractionService" /> service.</param>
		/// <param name="emailService">Container-injected instance for the <see cref="IEmailService" /> service.</param>
		public AccountController(
			IOptions<AppConfigs> appConfigs,
			ILogger<AccountController> logger,
			SignInManager<IdentityUser> signInManager,
			UserManager<IdentityUser> userManager,
			IIdentityServerInteractionService identServerInteractionService,
			IEmailService emailService)
		{
			_appConfigs = appConfigs.Value;
			_logger = logger;
			_signInManager = signInManager;
			_userManager = userManager;
			_identServerInteractionService = identServerInteractionService;
			_emailService = emailService;
		}



		/// <summary>Endpoint called to perform a user sign in, via the input of user credentials.</summary>
		/// <param name="inputData">User credentials data sent to this endpoint.</param>
		/// <returns>
		///     Returns an <see cref="IActionResult" /> object representing the server's response to the client.
		///     In case of success, an HTTP 200 (Ok) will be returned, wrapping a <see cref="LoginOutputModel" /> instance.
		///     In case of login failure (e.g., due to invalid credentials, invalid redirection target, etc), a generic HTTP 401 (Unauthorized) response will be returned.
		/// </returns>
		[HttpPost(AppEndpoints.LoginUri)]
		public async Task<IActionResult> Login([FromBody] LoginInputModel inputData)
		{
			// If there is a return URL, then there must be a valid Authorization Context (else, this might be a malicious redirection attempt)
			if (string.IsNullOrWhiteSpace(inputData.ReturnUrl) == false)
			{
				var authorizationContext = await _identServerInteractionService.GetAuthorizationContextAsync(inputData.ReturnUrl);
				if (authorizationContext == null)
				{
					_logger.LogDebug($"Failed to retrieve Authorization Context for return URL: {inputData.ReturnUrl}");
					return Unauthorized();
				}
			}

			// Sign the user out, if he/she is currently logged in
			var loggedInUser = User.Identities.Any(i => i.IsAuthenticated);
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
					ReturnUrl = inputData.ReturnUrl,
				})
				: (IActionResult) Unauthorized();
		}



		/// <summary>Endpoint to be called to sign a user out.</summary>
		/// <param name="logoutId">
		///     The identifier of the logout request.
		///     This identifier is generated by IdentityServer when the End Session Endpoint is called, and is used
		///     to associate logout requests to their respective contexts.
		/// </param>
		/// <returns>
		///     Returns an <see cref="IActionResult" /> object representing the server's response to the client.
		///     This endpoint returns a HTTP 200 (Ok) response to indicate that the user has successfuly signed-out, or
		///     that the is currently (already) logged out.
		///     This endpoint returns an HTTP 400 (Bad Request) response when an invalid <paramref name="logoutId"/> is provided.
		/// </returns>
		[HttpPost(AppEndpoints.LogoutUri)]
		public async Task<IActionResult> Logout(string logoutId)
		{
			// Verify if there is a user logged in, and if a logout context can be acquired
			// from the given "logout ID"
			if (User == null || User.Identities.All(userIdent => userIdent.IsAuthenticated == false))
				return Ok();

			if (logoutId == null)
				return BadRequest();

			var logoutContext = await _identServerInteractionService.GetLogoutContextAsync(logoutId);
			if (logoutContext == null)
			{
				// IdentityServer4 always returns an object, even though it might be empty.
				// The code should never enter this branch.
				throw new InvalidOperationException("Failed to retrieve logout context: IdentityServer4 returned NULL context.");
			}


			// Sign the user out
			await _signInManager.SignOutAsync();
			return Ok(new LogoutPostOutputModel
			{
				PostLogoutRedirectUri = logoutContext.PostLogoutRedirectUri,
				SignOutIFrameUrl = logoutContext.SignOutIFrameUrl,
			});
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
		[HttpGet(AppEndpoints.CheckLoginUri)]
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


		/// <summary>Endpoint used to register new users.</summary>
		/// <param name="registerData">Information about the user to be registered.</param>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///
		///     <para>In case of success, an HTTP 200 (Ok) will be returned, indicating that the new user has been registered.</para>
		///
		///     <para>
		///         An HTTP 400 (Bad Request) with a Problem Details (RFC-7807) body will be returned if the user's data is incorrect
		///         or not acceptable (e.g., malformed emails, weak passwords, etc). For security purposes, HTTP 400 (BadRequest) is also
		///         the returned code when the user already exists in the database, so that an attacker can not try to exploit that fact.
		///     </para>
		/// </returns>
		[HttpPost(AppEndpoints.RegisterUri)]
		public async Task<IActionResult> Register([FromBody] AccountRegisterInputModel registerData) {
			// Try to register the new user
			var newUser = new IdentityUser(registerData.UserName)
			{
				Email = registerData.Email,
			};
			var userCreateResult = await _userManager.CreateAsync(newUser, registerData.Password);
			if (userCreateResult.Succeeded == false)
			{
				var validationErrorsDictionary = userCreateResult.Errors
					.GroupBy(
						error => UserRegistrationErrorCodeToFieldMap[error.Code] ?? UnknownErrorCodeFieldName,
						error => error.Description)
					.ToDictionary(
						group => group.Key,
						group => group.ToArray()
					);
				var validationProblems = new ValidationProblemDetails(validationErrorsDictionary);
				return ValidationProblem(validationProblems);
			}

			// Send an email containing an email verification link to the user
			var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
			var queryBuilder = new QueryBuilder();
			queryBuilder.Add("user", newUser.Id);
			queryBuilder.Add("token", verificationToken);

			var accountVerificationLink = $"{Request.Scheme}://{Request.Host.Value}{Url.Action(nameof(VerifyAccount))}{queryBuilder.ToString()}";
			bool result = await _emailService.SendMessageFromResourceAsync(
				registerData.Email,
				"simple-oidc-oauth: Account verification",
				"EmailTemplates/AccountVerification.html",
				new Dictionary<string, object> {
					{"userName", registerData.UserName},
					{"accountVerificationLink", accountVerificationLink},
				});
			return Ok();
		}


		/// <summary>Endpoint used to verify the user's email.</summary>
		/// <param name="user">The identifier for the user that must be verified.</param>
		/// <param name="token">The email verification token generated when the user registered his/her account.</param>
		/// <returns>
		///     <para>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>In case of success, this endpoint returns an HTTP 200 (Ok) response.</para>
		///     <para>In case of failure, this endpoint returns an HTTP 403 (Forbidden) response.</para>
		/// </returns>
		[HttpGet(AppEndpoints.VerifyAccountUri)]
		public async Task<IActionResult> VerifyAccount([FromQuery] string user, [FromQuery] string token) {
			var targetUser = await _userManager.FindByIdAsync(user);
			if (targetUser == null)
				return Forbid();

			var emailConfirmationResult = await _userManager.ConfirmEmailAsync(targetUser, token);
			if (emailConfirmationResult.Succeeded == false)
				return Forbid();

			return Ok();
		}
	}
}