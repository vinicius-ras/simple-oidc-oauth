using AutoMapper;
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
using SimpleOidcOauth.Data.Security;
using SimpleOidcOauth.Data.Serialization;
using SimpleOidcOauth.Models;
using SimpleOidcOauth.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
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
	[SwaggerTag("Endpoints for user account registration and authentication.")]
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
		private readonly SignInManager<ApplicationUser> _signInManager;
		/// <summary>Container-injected instance for the <see cref="UserManager{TUser}" /> service.</summary>
		private readonly UserManager<ApplicationUser> _userManager;
		/// <summary>Container-injected instance for the <see cref="IIdentityServerInteractionService" /> service.</summary>
		private readonly IIdentityServerInteractionService _identServerInteractionService;
		/// <summary>Container-injected instance for the <see cref="IEmailService" /> service.</summary>
		private readonly IEmailService _emailService;
		/// <summary>Container-injected instance for the <see cref="IMapper" /> service.</summary>
		private readonly IMapper _mapper;





		// PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="appConfigs">Container-injected instance for the <see cref="IOptions{TOptions}" /> service.</param>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		/// <param name="signInManager">Container-injected instance for the <see cref="SignInManager{TUser}" /> service.</param>
		/// <param name="userManager">Container-injected instance for the <see cref="UserManager{TUser}" /> service.</param>
		/// <param name="identServerInteractionService">Container-injected instance for the <see cref="IIdentityServerInteractionService" /> service.</param>
		/// <param name="emailService">Container-injected instance for the <see cref="IEmailService" /> service.</param>
		/// <param name="mapper">Container-injected instance for the <see cref="IMapper" /> service.</param>
		public AccountController(
			IOptions<AppConfigs> appConfigs,
			ILogger<AccountController> logger,
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IIdentityServerInteractionService identServerInteractionService,
			IEmailService emailService,
			IMapper mapper)
		{
			_appConfigs = appConfigs.Value;
			_logger = logger;
			_signInManager = signInManager;
			_userManager = userManager;
			_identServerInteractionService = identServerInteractionService;
			_emailService = emailService;
			_mapper = mapper;
		}



		/// <summary>Performs user authentication.</summary>
		/// <remarks>
		///     <para>
		///         This endpoint issues cookies for identifying the authenticated user, which can be used to access
		///         other protected endpoints.
		///     </para>
		///     <para>
		///         The endpoint also integrates with the OAuth and OpenID Connect protocols, when a <see cref="LoginInputModel.ReturnUrl"/>
		///         is provided. This return URL should have any query parameters which are required for OAuth/OIDC authorization.
		///     </para>
		/// </remarks>
		/// <param name="inputData">User credentials data sent for performing authentication.</param>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">Indicates the login was successful. A <see cref="LoginOutputModel" /> instance will be returned in the response body.</response>
		/// <response code="401">Indicates the login procedure failed for some reason (e.g., invalid credentials, invalid redirection target, etc).</response>
		[HttpPost(AppEndpoints.LoginUri)]
		public async Task<ActionResult<LoginOutputModel>> Login([FromBody] LoginInputModel inputData)
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
			IEnumerable<Claim> userClaims = signInResult.Succeeded
				? await _userManager.GetClaimsAsync(user)
				: Enumerable.Empty<Claim>();
			return signInResult.Succeeded
				? Ok(new LoginOutputModel {
					Id = user.Id,
					Name = user.UserName,
					Email = user.Email,
					ReturnUrl = inputData.ReturnUrl,
					Claims = userClaims.Select(claim => _mapper.Map<SerializableClaim>(claim)),
				})
				: Unauthorized();
		}



		/// <summary>Performs a user sign out.</summary>
		/// <remarks>
		///     <para>Any issued authentication cookie will be cleared by this endpoint.</para>
		///     <para>
		///         The endpoint also integrates with the OAuth and OpenID Connect protocols, when a <paramref name="logoutId"/>
		///         is provided.
		///     </para>
		/// </remarks>
		/// <param name="logoutId" example="54fa540737444a2ca2d18cf6381aa52a">
		///     <para>An optional identifier for the logout request.</para>
		///     <para>
		///         This identifier is generated by IdentityServer when the End Session Endpoint is called, and is used
		///         to associate logout requests to their respective contexts.
		///     </para>
		/// </param>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">
		///     <para>For security reasons, this status code is always returned by this endpoint, even if the user is not currently logged in.</para>
		///     <para>After this endpoint's response, the user should be considered as "logged-out".</para>
		///     <para>A <see cref="LogoutPostOutputModel"/> object will be returned in the response's body.</para>
		/// </response>
		[HttpPost(AppEndpoints.LogoutUri)]
		public async Task<ActionResult<LogoutPostOutputModel>> Logout(string logoutId)
		{
			// If there is a logout ID, verify if it is valid.
			// NOTE: if there is no logout ID, this means that the user is navigating the auth-server front-end ("spa")
			// application (and not any of the client applications that use this IdP).
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



		/// <summary>Verifies if the user has a valid session established with the authentication/authorization server.</summary>
		/// <remarks>
		///     The validity of the user's session is determined by the presence of a valid authentication, established by a cookie
		///     generated by the ASP.NET Core Identity framework when the user signs in (<see cref="Login(LoginInputModel)"/> endpoint).
		/// </remarks>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">
		///     Indicates the user currently holds a valid session. A <see cref="LoginOutputModel" /> instance will
		///     be returned in the reponse body.
		/// </response>
		/// <response code="401">
		///     Indicates the user does not have a valid session. For accessing most of the authentication/authorization server's functionalities,
		///     a new session must be established through the <see cref="Login(LoginInputModel)"/> endpoint.
		/// </response>
		[Authorize]
		[HttpGet(AppEndpoints.CheckLoginUri)]
		public ActionResult<LoginOutputModel> CheckLogin()
		{
			var userIdentity = User.Identities
				.First(id => id.AuthenticationType.Equals(IdentityConstants.ApplicationScheme, System.StringComparison.Ordinal));
			var result = new LoginOutputModel {
				Id = userIdentity.Claims.First(c => c.Type == JwtClaimTypes.Subject).Value,
				Name = userIdentity.Claims.First(c => c.Type == JwtClaimTypes.Name).Value,
				Email = userIdentity.Claims.First(c => c.Type == JwtClaimTypes.Email).Value,
				Claims = userIdentity.Claims.Select(c => _mapper.Map<SerializableClaim>(c)),
			};
			return Ok(result);
		}


		/// <summary>Endpoint used to register new users.</summary>
		/// <param name="registerData">Information about the user to be registered.</param>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">Indicates that the operation was successful, and the new user was registered.</response>
		/// <response code="400">
		///     <para>
		///         Indicates the user data is invalid, incorrect, or not acceptable. Examples of such data might include (but are not limited to):
		///         malformed emails, weak passwords, an invalid user name, etc.
		///     </para>
		///     <para>
		///         For security reasons, HTTP 400 (Bad Request) is also the returned code when the user already exists in the database, as to avoid that an attacker tries to
		///         exploit that knowledge in any ways.
		///     </para>
		///     <para>
		///         A <see cref="ValidationProblemDetails"/> instance is returned in the response body describing the errors with the request's payload. Descriptions might
		///         be very superficial in some cases due to security restrictions.
		///     </para>
		/// </response>
		[HttpPost(AppEndpoints.RegisterUri)]
		[ProducesResponseType(typeof(ValidationProblemDetails), (int) HttpStatusCode.BadRequest)]
		public async Task<IActionResult> Register([FromBody] AccountRegisterInputModel registerData) {
			// Try to register the new user
			var newUser = new ApplicationUser
			{
				UserName = registerData.UserName,
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


		/// <summary>Endpoint used to verify (validate/confirm) the user's email.</summary>
		/// <param name="user" example="0785fcbe-2cc3-4f21-a1a8-6e6ecfe52544">
		///     The identifier for the user that must be verified.
		/// </param>
		/// <param name="token" example="fake-token-b8afec00-dfb0-4dfa-8d71-abab39557be0-very-fake">
		///     The email verification token generated when the user registered his/her account.
		/// </param>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		/// <response code="200">Informs that the user's email has been successfully verified.</response>
		/// <response code="403">
		///     Indicates failure in the operation.
		///     This effectivelly means the user's email (if any) has not been verified.
		/// </response>
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