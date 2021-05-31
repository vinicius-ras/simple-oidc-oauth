using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace SimpleOidcOauth.Services
{
	/// <summary>A service which is able to communicate with SMTP servers in order to send emails.</summary>
	/// <remarks>
	///     <para>This service sends HTML emails only - plain text emails are not supported.</para>
	///     <para>
	///         Email templates must be used when sending the emails through this service. A template is basically an HTML5 document
	///         which can contain placeholders for data in the form <c>${key}</c>. When sending emails, you will be calling methods
	///         where you can pass <see cref="IDictionary{TKey, TValue}" /> objects, which you can specify the data to be used while
	///         rendering the email's body.
	///     </para>
	///     <para>
	///         IMPORTANT: key identifiers have a rule to be considered valid. They must start with ASCII letters <c>[a-z]</c> (either lower case or upper case) or underscores (<c>'_'</c>),
	///         and the remaining characters can be ASCII letters <c>[a-z]</c> (either lower case or upper case), underscores (<c>'_'</c>), or digits <c>[0-9]</c>. Letter accents and other
	///         symbols must not be used.
	///     </para>
	/// </remarks>
	public interface IEmailService
	{
		/// <summary>
		///     <para>
		///         Sends an email message through the application's configured SMTP server, allowing the message's template to be specified
		///         as a raw string value.
		///     </para>
		///     <para>
		///         This overload uses the default host, port, and credentials configured for the application when sending emails,
		///         while also building a <see cref="MimeMessage" /> with the given arguments, and using the application's configured "sender"
		///         email (<see cref="SimpleOidcOauth.Data.Configuration.EmailConfigs.DefaultSenderEmail" />).
		///     </para>
		/// </summary>
		/// <param name="to">The target recipient for the email.</param>
		/// <param name="subject">The email's subject.</param>
		/// <param name="htmlBodyTemplate">
		///     The HTML contents template for the email's body.
		///     For more information on how to build templates, please refer to the <see cref="IEmailService" /> interface's documentation.
		/// </param>
		/// <param name="emailData">
		///     A dictionary which maps keys that can appear in the email body's template to the respective values which will replace the occurences
		///     of these keys when rendering the email to be sent. For each key <c>K</c> contained in this dictionary which maps to a value <c>V</c>,
		///     every occurence of <c>${K}</c> in the email template will be replaced by the string representation of the value <c>V</c>.
		/// </param>
		/// <param name="cancellationToken">A token which can be used to cancel the sending operation.</param>
		/// <returns>A task which resolves to a flag representing if the operation was successful.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if any of the non-optional parameters (<paramref name="to"/>, <paramref name="subject"/>, <paramref name="htmlBodyTemplate"/>) is set to <c>null</c>.</exception>
		Task<bool> SendMessageFromStringAsync(string to, string subject, string htmlBodyTemplate, IDictionary<string, object> emailData = null, CancellationToken cancellationToken = default(CancellationToken));


		/// <summary>
		///     <para>
		///         Sends an email message through the application's configured SMTP server, allowing the message's template to be
		///         loaded from one of the application's embedded resources.
		///     </para>
		///     <para>
		///         This overload uses the default host, port, and credentials configured for the application when sending emails,
		///         while also building a <see cref="MimeMessage" /> with the given arguments, and using the application's configured "sender"
		///         email (<see cref="SimpleOidcOauth.Data.Configuration.EmailConfigs.DefaultSenderEmail" />).
		///     </para>
		/// </summary>
		/// <param name="to">The target recipient for the email.</param>
		/// <param name="subject">The email's subject.</param>
		/// <param name="templateResourcePath">
		///     The path of the resource containing the email template.
		///     This path will be used by the <see cref="IEmbeddedResourcesService" /> to load the email template from an embedded resource.
		///     For more information on how to build templates, please refer to the <see cref="IEmailService" /> interface's documentation.
		/// </param>
		/// <param name="emailData">
		///     A dictionary which maps keys that can appear in the email body's template to the respective values which will replace the occurences
		///     of these keys when rendering the email to be sent. For each key <c>K</c> contained in this dictionary which maps to a value <c>V</c>,
		///     every occurence of <c>${K}</c> in the email template will be replaced by the string representation of the value <c>V</c>.
		/// </param>
		/// <param name="cancellationToken">A token which can be used to cancel the sending operation.</param>
		/// <returns>A task which resolves to a flag representing if the operation was successful.</returns>
		/// <remarks>
		///     This method calls <see cref="SendMessageFromStringAsync(string, string, string, IDictionary{string, object}, CancellationToken)" /> once the resource's contents are loaded.
		///     Thus, any exception thrown by <see cref="SendMessageFromStringAsync(string, string, string, IDictionary{string, object}, CancellationToken)" /> might also be thrown by this method.
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">The <paramref name="templateResourcePath"/> parameter is <c>null</c>.</exception>
		Task<bool> SendMessageFromResourceAsync(string to, string subject, string templateResourcePath, IDictionary<string, object> emailData = null, CancellationToken cancellationToken = default(CancellationToken));
	}
}