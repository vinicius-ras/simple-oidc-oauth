using Microsoft.AspNetCore.Mvc;

namespace SimpleOidcOauth.Controllers
{
	/// <summary>
	///     <para>
	///         Implements an exception handler which translates captured unhandled exceptions generated in API calls
	///         into "Problem Details for HTTP APIs" responses.
	///     </para>
	///     <para>
	///         "Problem Details for HTTP APIs" are described in <a href="https://tools.ietf.org/html/rfc7807">RFC 7807</a> as a way
	///         to homogenize/standardize message formats used to convey machine-readable details for errors carried in HTTP responses.
	///     </para>
	/// </summary>
	[ApiController]
	public class UnhandledExceptionsController : ControllerBase
	{
		// CONSTANTS
		/// <summary>The route which will be associated to the endpoint that treats errors and converts them to RFC 7807 compliant messages.</summary>
		public const string EXCEPTION_HANDLER_ROUTE = "/unhandled-exception";
		/// <summary>The default error title to be returned to the clients for reporting unhandled/unexpected exceptions.</summary>
		private const string DEFAULT_ERROR_TITLE = "Generic error.";
		/// <summary>The default error message to be returned to the clients for reporting unhandled/unexpected exceptions.</summary>
		private const string DEFAULT_ERROR_DESCRIPTION = "Unexpected server error. Please try again later.";





		// INSTANCE METHODS
		/// <summary>Called whenever an unhandled exception is fired and caught by the Exception Handler Middleware.</summary>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		[Route(EXCEPTION_HANDLER_ROUTE)]
		public IActionResult OnUnhandledException()
		{
			// Returns a generic "Problem Details" response
			return Problem(
				title: DEFAULT_ERROR_TITLE,
				detail: DEFAULT_ERROR_DESCRIPTION
			);
		}
	}
}