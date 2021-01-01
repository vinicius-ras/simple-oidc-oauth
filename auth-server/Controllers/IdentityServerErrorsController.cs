
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOidcOauth.Controllers
{
	/// <summary>A controller which handles errors returned by the IdentityServer4 infrastructure.</summary>
	[Route("/api/idp-error")]
	public class IdentityServerErrorsController : ControllerBase
	{
		// INSTANCE FIELDS
		/// <summary>Container-injected instance for the <see cref="IIdentityServerInteractionService" /> service.</summary>
		private readonly IIdentityServerInteractionService _identServerInteractionService;





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		/// <param name="identServerInteractionService">Container-injected instance for the <see cref="IIdentityServerInteractionService" /> service.</param>
		public IdentityServerErrorsController(IIdentityServerInteractionService identServerInteractionService)
		{
			_identServerInteractionService = identServerInteractionService;
		}


		/// <summary>Called by IdentityServer4 whenever there is an error (e.g., invalid client IDs, invalid client credentials, etc).</summary>
		/// <param name="errorId">
		///     The error identifier generated by IdentityServer.
		///     This identifier will be used to retrieve more specific error information.
		/// </param>
		/// <returns>
		///     <para>Returns a task wrapping an <see cref="IActionResult" /> object representing the server's response to the client.</para>
		///     <para>In case of success, this endpoint returns an HTTP 200 (Ok) response.</para>
		///     <para>In case of failure, this endpoint returns an HTTP 400 (Bad Request) response.</para>
		/// </returns>
		[HttpGet]
		public async Task<IActionResult> Error(string errorId)
		{
			var errorCtx = await _identServerInteractionService.GetErrorContextAsync(errorId);
			if (errorCtx == null)
				return Problem(title: "Unknown IdP error.", detail: "Failed to recover IdP error information.");
			return new JsonResult(errorCtx);
		}
	}
}