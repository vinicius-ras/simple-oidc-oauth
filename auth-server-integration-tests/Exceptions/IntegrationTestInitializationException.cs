using System;

namespace SimpleOidcOauth.Tests.Integration.Exceptions
{
	/// <summary>An exception fired whenever an Integration Test fails for some reason during its initialization procedures.</summary>
	public class IntegrationTestInitializationException : IntegrationTestsBaseException
	{
		// INSTANCE METHODS
		/// <summary>Default constructor.</summary>
		public IntegrationTestInitializationException() {}


		/// <summary>Constructor.</summary>
		/// <param name="message">The message that describes the error.</param>
		public IntegrationTestInitializationException(string message) : base(message) {}


		/// <summary>Constructor.</summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public IntegrationTestInitializationException(string message, Exception inner) : base(message, inner) {}
	}
}