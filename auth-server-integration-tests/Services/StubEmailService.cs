using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleOidcOauth.Services;

namespace SimpleOidcOauth.Tests.Integration.Services
{
	/// <summary>
	///     A stub implementation for the <see cref="IEmailService"/> to be used for the Integration Tests.
	///     This implementation always returns <c>true</c> to indicate success while sending emails.
	/// </summary>
	class StubEmailService : IEmailService
	{
		// INTERFACE IMPLEMENTATION: IEmailService
		public Task<bool> SendMessageFromResourceAsync(string to, string subject, string templateResourcePath, IDictionary<string, object> emailData = null, CancellationToken cancellationToken = default)
			=> Task.FromResult(true);


		public Task<bool> SendMessageFromStringAsync(string to, string subject, string htmlBodyTemplate, IDictionary<string, object> emailData = null, CancellationToken cancellationToken = default)
			=> Task.FromResult(true);
	}
}