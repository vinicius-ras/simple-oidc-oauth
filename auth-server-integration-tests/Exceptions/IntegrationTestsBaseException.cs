using System;

namespace SimpleOidcOauth.Tests.Integration.Exceptions
{
	/// <summary>Base class for custom exceptions defined for the Integration Tests project.</summary>
	public abstract class IntegrationTestsBaseException : Exception
	{
		// INSTANCE METHODS
		/// <summary>Default constructor.</summary>
		public IntegrationTestsBaseException() {}


		/// <summary>Constructor.</summary>
		/// <param name="message">The message that describes the error.</param>
		public IntegrationTestsBaseException(string message) : base(message) {}


		/// <summary>Constructor.</summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public IntegrationTestsBaseException(string message, Exception inner) : base(message, inner) {}
	}
}