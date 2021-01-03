using System;
using System.Net.Http;

namespace SimpleOidcOauth.Tests.Integration.Exceptions
{
	/// <summary>Exception thrown when there is an error/unexpected response when calling the Authorize Endpoint.</summary>
	class AuthorizeEndpointResponseException : IntegrationTestsBaseException
	{
		// INSTANCE PROPERTIES
		/// <summary>Reference to the response returned by the Authorize Endpoint, which will be a redirection to the IdP Error Endpoint.</summary>
		public HttpResponseMessage AuthorizeEndpointResponse { get; init; }
		/// <summary>Reference to the response returned by the IdP Error Endpoint, possibly having JSON data contents returned by the IdP Error Endpoint.</summary>
		public HttpResponseMessage ErrorEndpointResponse { get; init; }
		/// <summary>Contents of the response returned by the IdP Error Endpoint, in form of a string.</summary>
		public string ErrorEndpointResponseString { get; init; }
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