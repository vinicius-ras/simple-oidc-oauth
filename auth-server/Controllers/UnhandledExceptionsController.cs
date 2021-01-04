using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleOidcOauth.Data;
using System.Text;

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
		/// <summary>The default error title to be returned to the clients for reporting unhandled/unexpected exceptions.</summary>
		private const string DEFAULT_ERROR_TITLE = "Generic error.";
		/// <summary>The default error message to be returned to the clients for reporting unhandled/unexpected exceptions.</summary>
		private const string DEFAULT_ERROR_DESCRIPTION = "Unexpected server error. Please try again later.";





		// INSTANCE FIELDS
		/// <summary>Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</summary>
		private readonly ILogger<UnhandledExceptionsController> _logger;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="logger">Container-injected instance for the <see cref="ILogger{TCategoryName}" /> service.</param>
		public UnhandledExceptionsController(ILogger<UnhandledExceptionsController> logger)
		{
			_logger = logger;
		}





		/// <summary>Called whenever an unhandled exception is fired and caught by the Exception Handler Middleware.</summary>
		/// <returns>Returns an <see cref="IActionResult" /> object representing the server's response to the client.</returns>
		[Route(AppEndpoints.UnhandledExceptionUri)]
		public IActionResult OnUnhandledException()
		{
			// Retrieve and log the exception which generated the error
			var exceptionInfoFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			if (exceptionInfoFeature == null)
				_logger.LogError($"Unknown unhandled exception caught: {nameof(UnhandledExceptionsController)}.{nameof(OnUnhandledException)}() called without features of type {nameof(IExceptionHandlerPathFeature)}.");
			else
			{
				// Build an error message about the unhandled exception
				var errorMsgBuilder = new StringBuilder();
				errorMsgBuilder.Append("Unhandled");
				if (exceptionInfoFeature.Error != null)
					errorMsgBuilder.Append($" exception of type {exceptionInfoFeature.Error.GetType().Name}");
				else
					errorMsgBuilder.Append(" NULL/UNIDENTIFIED exception");

				errorMsgBuilder.Append(" on");
				if (exceptionInfoFeature.Path != null)
					errorMsgBuilder.Append($@" path ""{exceptionInfoFeature.Path}""");
				else
					errorMsgBuilder.Append($@" NULL/UNIDENTIFIED path.");

				// Log the built error message as an error
				if (exceptionInfoFeature.Error != null)
					_logger.LogError(exceptionInfoFeature.Error, errorMsgBuilder.ToString());
				else
					_logger.LogError(errorMsgBuilder.ToString());
			}


			// Returns a generic "Problem Details" response
			return Problem(
				title: DEFAULT_ERROR_TITLE,
				detail: DEFAULT_ERROR_DESCRIPTION
			);
		}
	}
}