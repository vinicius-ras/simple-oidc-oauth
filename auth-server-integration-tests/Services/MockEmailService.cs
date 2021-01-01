using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SimpleOidcOauth.Services;

namespace SimpleOidcOauth.Tests.Integration.Services
{
	/// <summary>
	///     <para>A mock implementation for the <see cref="IEmailService"/> to be used for the Integration Tests.</para>
	///     <para>
	///         This implementation always returns <c>true</c> to indicate success while sending emails.
	///         It is also registered as a Singleton, and captures the last email data object used to send emails, to be used for
	///         inspecting email data values during the tests.
	///     </para>
	/// </summary>
	public class MockEmailService : IEmailService
	{
		// INSTANCE FIELDS
		/// <summary>A lock for controlling concurrent access to the <see cref="LastSentEmailData"/> property.</summary>
		private object _lockLastSentEmailData = new object();
		/// <summary>Backing field for the <see cref="LastSentEmailData"/> property.</summary>
		private IDictionary<string, object> _lastSentEmailData;





		// INSTANCE PROPERTIES
		/// <summary>
		///     <para>Retrieves the data of the email that was sent last by the <see cref="IEmailService"/>.</para>
		///     <para>
		///         This dictionary was designed to be safe to use as long as each test suite/class uses its own instance of a <see cref="MockEmailService"/>,
		///         and as long as tests are executed sequentially inside a single test suite/class that share a single Test Host.
		///     </para>
		///     <para>
		///         Notice that the <see cref="TestSuites.IntegrationTestBase"/> class automatically takes care of that by instantiating a new Test Host for
		///         each test suite/class - which in turn creates a brand new and independent <see cref="MockEmailService"/> instance for each test suite/class.
		///     </para>
		/// </summary>
		public IDictionary<string, object> LastSentEmailData {
			get
			{
				lock (_lockLastSentEmailData)
				{
					return _lastSentEmailData;
				}
			}
			private set {
				lock (_lockLastSentEmailData)
				{
					_lastSentEmailData = value;
				}
			}
		}





		// INTERFACE IMPLEMENTATION: IEmailService
		public Task<bool> SendMessageFromResourceAsync(string to, string subject, string templateResourcePath, IDictionary<string, object> emailData = null, CancellationToken cancellationToken = default)
		{
			LastSentEmailData = emailData.ToImmutableDictionary();
			return Task.FromResult(true);
		}


		public Task<bool> SendMessageFromStringAsync(string to, string subject, string htmlBodyTemplate, IDictionary<string, object> emailData = null, CancellationToken cancellationToken = default)
		{
			LastSentEmailData = emailData.ToImmutableDictionary();
			return Task.FromResult(true);
		}
	}
}