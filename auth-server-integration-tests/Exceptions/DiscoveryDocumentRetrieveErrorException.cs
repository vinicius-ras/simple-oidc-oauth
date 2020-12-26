using System;
using IdentityModel.Client;

namespace SimpleOidcOauth.Tests.Integration.Exceptions
{
	/// <summary>Exception thrown when there is an error while trying to retrieve the IdP Discovery Document.</summary>
	class DiscoveryDocumentRetrieveErrorException : IntegrationTestsBaseException
	{
		// INSTANCE PROPERTIES
		/// <summary>Reference to the response that generated this exception.</summary>
		public DiscoveryDocumentResponse DiscoveryDocumentResponse { get; init; }





		// INSTANCE METHODS
		/// <summary>Constructor.</summary>
		public DiscoveryDocumentRetrieveErrorException() {}


		/// <summary>Constructor.</summary>
		/// <param name="message">The message that describes the error.</param>
		public DiscoveryDocumentRetrieveErrorException(string message) : base(message) {}


		/// <summary>Constructor.</summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public DiscoveryDocumentRetrieveErrorException(string message, Exception inner) : base(message, inner) {}
	}
}