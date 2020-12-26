using System;
using System.Net.Http;

namespace SimpleOidcOauth.Tests.Integration.Exceptions
{
	/// <summary>Exception thrown when there is an error/unexpected response when calling the Authorize Endpoint.</summary>
	class AuthorizeEndpointResponseException : IntegrationTestsBaseException
	{
		// INSTANCE PROPERTIES
		/// <summary>Reference to the response that generated this exception.</summary>
		public HttpResponseMessage Response { get; init; }
		/// <summary>Reference to the request URI which was used and led to the response that generated this exception.</summary>
		public string RequestUri { get; init; }





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		public AuthorizeEndpointResponseException() {}


		/// <summary>Constructor.</summary>
		/// <param name="message">The message that describes the error.</param>
		public AuthorizeEndpointResponseException(string message) : base(message) {}


		/// <summary>Constructor.</summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public AuthorizeEndpointResponseException(string message, Exception inner) : base(message, inner) {}
	}
}