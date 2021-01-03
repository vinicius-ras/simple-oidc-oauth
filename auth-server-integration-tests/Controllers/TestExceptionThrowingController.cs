using System;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOidcOauth.Tests.Integration.Controllers
{
	/// <summary>A special controller used to fire an unhandled exception during the tests in order to trigger the Unhandled Exception Middleware.</summary>
	[Route(TestExceptionThrowingController.EndpointUri)]
	[ApiController]
	public class TestExceptionThrowingController : ControllerBase
	{
		// CONSTANTS
		/// <summary>The URI which corresponds to the endpoint in this controller that is used to simply fire an unhandled exception.</summary>
		public const string EndpointUri = "/exception-throwing-controller-92480771cd4847ef993b6bd96e679960";





		// INSTANCE METHODS
		/// <summary>This action is called to fire an unhandled exception for the integration tests.</summary>
		/// <returns>
		///     Returns an <see cref="IActionResult" /> object representing the server's response to the client.
		///     This action always throws an unhandled exception, so this endpoint will always trigger the "unhandled exception"-related middleware.
		/// </returns>
		[HttpGet]
		public IActionResult ThrowException() => throw new Exception($"Intentionally unhandled exception");
	}
}